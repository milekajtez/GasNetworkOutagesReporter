using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Common.Abstractions;
using Common.Models;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace GasReporterCreatorService
{
    public class ReportCreator : IReportCreator
    {
        private IReliableStateManager StateManager { get; set; }

        public ReportCreator() { }

        public ReportCreator(IReliableStateManager stateManager)
        {
            StateManager = stateManager;
        }

        #region AddNewReport
        public async Task<bool> AddNewReport(string address, string details, string workerFullName, string steps)
        {
            var reportId = Guid.NewGuid();
            var report = new Report(reportId.ToString("N"), Guid.NewGuid(), address, details, workerFullName, steps, false);

            var reportsDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, Report>>("ReportsDictionary");

            using (var tx = StateManager.CreateTransaction())
            {
                await reportsDictionary.AddAsync(tx, reportId.ToString("N"), report);
                await tx.CommitAsync();
            }

            return true;
        }
        #endregion

        #region AddNewReportToStorage
        public async Task<bool> AddNewReportToStorage(CancellationToken cancellationToken)
        {
            var _storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["DataConnectionString"]);
            var tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            var _reportTable = tableClient.GetTableReference("ReportTableStorage");
            var _stepTable = tableClient.GetTableReference("StepTableStorage");
            var query = new TableQuery<Report>();
            var reportsDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, Report>>("ReportsDictionary");

            using (var tx = StateManager.CreateTransaction())
            {
                var reportEnumerable = await reportsDictionary.CreateEnumerableAsync(tx, key => key != "", EnumerationMode.Ordered);
                var asyncReportEnumerator = reportEnumerable.GetAsyncEnumerator();

                while (await asyncReportEnumerator.MoveNextAsync(cancellationToken))
                {
                    var insertOperation = TableOperation.InsertOrReplace(asyncReportEnumerator.Current.Value);
                    _reportTable.Execute(insertOperation);
                }
            }

            return true;
        }
        #endregion
    }
}
