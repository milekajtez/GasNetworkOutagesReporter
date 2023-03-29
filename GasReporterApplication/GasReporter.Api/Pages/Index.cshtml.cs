using Common.Abstractions;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace GasReporter.Api.Pages
{
    public class IndexModel : PageModel
    {
        public List<Report> Reports { get; set; }

        public void OnGet()
        {
            var myBinding = new NetTcpBinding(SecurityMode.None);
            var myEndpoint = new EndpointAddress("net.tcp://localhost:7001/SummaryServiceEndpoint");
            var validationFactory = new ChannelFactory<ISummary>(myBinding, myEndpoint);

            ISummary summary = null;
            try
            {
                summary = validationFactory.CreateChannel();
                Reports = summary.GetReports(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                (summary as ICommunicationObject)?.Abort();
            }

            ((ICommunicationObject)summary).Close();
            validationFactory.Close();
        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        public void ArchiveReport()
        {
            var reportId = Request.Form["archive"];

            /*string serviceUri = "fabric:/GasReporterApplication/GasReporterCreatorService";
            var fabricClient = new FabricClient();
            int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri(serviceUri))).Count;
            var binding = WcfUtility.CreateTcpClientBinding();
            int index = 0;

            for (int i = 0; i < partitionsNumber; i++)
            {
                var servicePartitionClient = new ServicePartitionClient<WcfCommunicationClient<IReportCreator>>(
                    new WcfCommunicationClientFactory<IReportCreator>(clientBinding: binding),
                    new Uri(serviceUri),
                    new ServicePartitionKey(index % partitionsNumber));

                await servicePartitionClient.InvokeWithRetryAsync(client =>
                    client.Channel.ArchiveReport(reportId));

                index++;
            }*/
        }
    }
}
