using Common.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Fabric;
using System.Threading.Tasks;

namespace GasReporter.Api.Pages
{
    public class ReportModel : PageModel
    {
        [BindProperty]
        public string Steps { get; set; }

        public void OnGet()
        {

        }

        #region OnPost
        public async Task<IActionResult> OnPost()
        {
            var reportDetails = Request.Form["details"];
            var reportAddress = Request.Form["address"];
            var workerFullName = Request.Form["worker"];
            var steps = Steps;
            string serviceUri = "fabric:/GasReporterApplication/GasReporterCreatorService";
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
                    client.Channel.AddNewReport(reportAddress, reportDetails, workerFullName, steps));

                index++;
            }

            return Redirect("Index");
        }
        #endregion
    }
}
