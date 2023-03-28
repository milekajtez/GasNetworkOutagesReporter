using Common.Helpers;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.Abstractions
{
    [ServiceContract]
    public interface ISummary
    {
        [OperationContract]
        List<SummaryData> GetSummaryData(bool isArchived);
    }
}
