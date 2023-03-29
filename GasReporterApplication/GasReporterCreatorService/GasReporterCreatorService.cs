using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Common.Abstractions;
using Common.Models;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace GasReporterCreatorService
{
    internal sealed class GasReporterCreatorService : StatefulService
    {
        public GasReporterCreatorService(StatefulServiceContext context) : base(context) { }

        #region CreateServiceReplicaListeners
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
           return new[] 
           {
               new ServiceReplicaListener(context => CreateInternalListener(context), "GasReporterCreatorService")
           };
        }
        
        private ICommunicationListener CreateInternalListener(ServiceContext context)
        {
            var internalEndpoint = context.CodePackageActivationContext.GetEndpoint("GasReporterCreatorService");
            string uriPrefix = string.Format(
                "{0}://{1}:{2}/{3}/{4}-{5}/",
                internalEndpoint.Protocol,
                FabricRuntime.GetNodeContext().IPAddressOrFQDN,
                internalEndpoint.Port,
                context.PartitionId,
                context.ReplicaOrInstanceId,
                Guid.NewGuid());
            
            return new WcfCommunicationListener<IReportCreator>(context, new ReportCreator(StateManager), WcfUtility.CreateTcpListenerBinding(), uriPrefix);
        }
        #endregion

        #region RunAsync
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var myDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");
            var reportsDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, Report>>("ReportsDictionary");
            var reportCreator = new ReportCreator(StateManager);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (var tx = StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");
                    ServiceEventSource.Current.ServiceMessage(Context, "Current Counter Value: {0}", result.HasValue ? result.Value.ToString() : "Value does not exist.");
                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                await reportCreator.AddNewReportToStorage(cancellationToken);
            }
        }
        #endregion
    }
}
