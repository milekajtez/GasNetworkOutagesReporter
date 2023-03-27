using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Common.Abstractions;
using Common.Helpers;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Runtime;

namespace EmailService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class EmailService : StatelessService
    {
        public string ImapServer { get; set; }
        public string ImapUsername { get; set; }
        public string ImapPassword { get; set; }
        public int ImapPort { get; set; }
        public string EmailSubject { get; set; }

        public EmailService(StatelessServiceContext context)
            : base(context)
        {
            ImapServer = "imap.gmail.com";
            ImapUsername = "milekajtez@gmail.com";
            ImapPassword = "ewmphobdtfadsiqh";
            ImapPort = 993;
            EmailSubject = "Create new report";
        }

        #region CreateServiceInstanceListeners
        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }
        #endregion

        #region RunAsync
        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            long iterations = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await LoadEmails();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                ServiceEventSource.Current.ServiceMessage(Context, "MailService-{0}", ++iterations);
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
        }
        #endregion

        #region LoadEmails
        private async Task LoadEmails()
        {
            var imapClient = new ImapClient();
            imapClient.Connect(ImapServer, ImapPort, SecureSocketOptions.SslOnConnect);
            imapClient.Authenticate(ImapUsername, ImapPassword);
            var inbox = imapClient.Inbox;
            inbox.Open(FolderAccess.ReadWrite);
            IList<UniqueId> uids = inbox.Search(SearchQuery.NotFlags(MessageFlags.Answered));

            string serviceUri = "fabric:/GasReporterApplication/GasReporterCreatorService";
            var fabricClient = new FabricClient();
            int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri(serviceUri))).Count;
            var binding = WcfUtility.CreateTcpClientBinding();
            int index = 0;

            foreach (UniqueId uid in uids)
            {
                var message = inbox.GetMessage(uid);
                if (message.Subject.Contains(EmailSubject))
                {
                    for (int i = 0; i < partitionsNumber; i++)
                    {
                        var servicePartitionClient = new ServicePartitionClient<WcfCommunicationClient<IReportCreator>>(
                            new WcfCommunicationClientFactory<IReportCreator>(clientBinding: binding),
                            new Uri(serviceUri),
                            new ServicePartitionKey(index % partitionsNumber));

                        var emailData = LoadEmailBody(message.TextBody);
                        await servicePartitionClient.InvokeWithRetryAsync(client =>
                            client.Channel.AddNewReport(emailData.ReportAddress, emailData.ReportDetails, emailData.WorkerFullName, emailData.Steps));

                        index++;
                    }

                    await inbox.SetFlagsAsync(uid, MessageFlags.Answered, true);
                }
            }

            index = 0;
            inbox.Close();
        }

        private EmailData LoadEmailBody(string textBody)
        {
            var mainArray = textBody.Replace("\r", string.Empty).Split('\n');
            return new EmailData()
            {
                ReportAddress = mainArray[0],
                ReportDetails = mainArray[1],
                WorkerFullName = mainArray[2],
                Steps = mainArray[3]
            };
        }
    }
    #endregion
}
