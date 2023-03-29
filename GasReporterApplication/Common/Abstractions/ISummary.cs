using Common.Models;
using System.Collections.Generic;
using System.ServiceModel;

namespace Common.Abstractions
{
    [ServiceContract]
    public interface ISummary
    {
        [OperationContract]
        List<Report> GetReports(bool isArchived);
    }
}
