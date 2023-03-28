using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Common.Abstractions;
using Common.Helpers;
using Common.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace SummaryService
{
    public class Summary : ISummary
    {
        public Summary() { }

        public List<SummaryData> GetSummaryData(bool isArchived)
        {
            string a = ConfigurationManager.AppSettings["DataConnectionString"];
            var _storageAccount = CloudStorageAccount.Parse(a);
            var tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            var _tableReport = tableClient.GetTableReference("ReportTableStorage");
            var _tableStep = tableClient.GetTableReference("StepTableStorage");

            var queryReports = new TableQuery<Report>();
            var querySteps = new TableQuery<Step>();
            var summaryData = new List<SummaryData>();

            if (isArchived)
            {
                summaryData = _tableReport.ExecuteQuery(queryReports)
                    .Where(r => r.IsArchived)
                    .Select(r => new SummaryData { Report = r })
                    .ToList();
            }
            else
            {
                summaryData = _tableReport.ExecuteQuery(queryReports)
                    .Select(r => new SummaryData { Report = r })
                    .ToList();
            }

            var allSteps = _tableStep.ExecuteQuery(querySteps).ToList();

            foreach (var data in summaryData)
            {
                var steps = allSteps
                    .Where(s => s.ReportId == data.Report.Id)
                    .ToList();

                data.Steps = steps;
            }
            
            return summaryData;
        }
    }
}
