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

        public List<Report> GetReports(bool isArchived)
        {
            string a = ConfigurationManager.AppSettings["DataConnectionString"];
            var _storageAccount = CloudStorageAccount.Parse(a);
            var tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            var reports = new List<Report>();

            var _tableReport = tableClient.GetTableReference("ReportTableStorage");
            var queryReports = new TableQuery<Report>();
            if (isArchived)
            {
                reports = _tableReport.ExecuteQuery(queryReports)
                    .Where(r => r.IsArchived)
                    .ToList();
            }
            else
            {
                reports = _tableReport.ExecuteQuery(queryReports)
                    .ToList();
            }
            
            return reports;
        }
    }
}
