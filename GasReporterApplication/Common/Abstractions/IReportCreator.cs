using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.Abstractions
{
    [ServiceContract]
    public interface IReportCreator
    {
        [OperationContract]
        Task<bool> AddNewReport(string address, string details, string workerFullName, string steps);
    }
}
