using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Common.Models
{
    public class Report : TableEntity
    {
        public Guid Id { get; set; }
        public string Address { get; set; }
        public string Details { get; set; }
        public string WorkerFullName { get; set; }
        public string Steps { get; set; }
        public bool IsArchived { get; set; }

        public Report() { }

        public Report(string rowKey, Guid id, string address, string details, string workerFullName, string steps, bool isArchived)
        {
            PartitionKey = "ReportTableStorage";
            RowKey = rowKey;
            Id = id;
            Address = address;
            Details = details;
            WorkerFullName = workerFullName;
            Steps = steps;
            IsArchived = isArchived;
        }
    }
}
