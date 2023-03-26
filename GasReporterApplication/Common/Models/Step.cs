using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Common.Models
{
    public class Step : TableEntity
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public int StepOrderNumber { get; set; }
        public Guid ReportId { get; set; }

        public Step() { }

        public Step(string rowKey, Guid id, string text, int stepOrderNumber, Guid reportId)
        {
            PartitionKey = "StepTableStorage";
            RowKey = rowKey;
            Id = id;
            Text = text;
            StepOrderNumber = stepOrderNumber;
            ReportId = reportId;
        }
    }
}
