﻿using Common.Abstractions;
using Common.Helpers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace GasReporter.Api.Pages
{
    public class HistoryModel : PageModel
    {
        public List<SummaryData> ArchivedReports { get; set; }

        public void OnGet()
        {
            var myBinding = new NetTcpBinding(SecurityMode.None);
            var myEndpoint = new EndpointAddress("net.tcp://localhost:7001/SummaryServiceEndpoint");
            var validationFactory = new ChannelFactory<ISummary>(myBinding, myEndpoint);

            ISummary summary = null;
            try
            {
                summary = validationFactory.CreateChannel();
                ArchivedReports = summary.GetSummaryData(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                (summary as ICommunicationObject)?.Abort();
            }

            ((ICommunicationObject)summary).Close();
            validationFactory.Close();
        }
    }
}
