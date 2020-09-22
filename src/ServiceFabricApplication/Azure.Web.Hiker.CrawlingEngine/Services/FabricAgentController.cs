using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Text;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.AgentRegistrar;
using Azure.Web.Hiker.Core.CrawlingAgent.Models;

using Newtonsoft.Json;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine.Services
{
    public class FabricAgentController : IAgentController
    {
        private readonly FabricClient _fabricClient;
        private readonly StatelessServiceContext _statelessServiceContext;

        public FabricAgentController(FabricClient fabricClient, StatelessServiceContext statelessServiceContext)
        {
            _fabricClient = fabricClient;
            _statelessServiceContext = statelessServiceContext;
        }

        public async Task SpawnNewAgentForHostnameAsync(string serviceTypeName, string hostname, string serviceName)
        {
            var crawlerData = JsonConvert.SerializeObject(new CrawlerAgentInitializationData(hostname));

            var serviceDescriptions = new List<StatelessServiceDescription>();
            var statelessServiceDescription = new StatelessServiceDescription()
            {
                ApplicationName = new Uri(_statelessServiceContext.CodePackageActivationContext.ApplicationName),
                ServiceName = new Uri($"{_statelessServiceContext.CodePackageActivationContext.ApplicationName}/{serviceName}"),
                ServiceTypeName = serviceTypeName, // "Azure.Web.Hiker.ServiceFabricApplication.CrawlingAgentType",
                PartitionSchemeDescription = new SingletonPartitionSchemeDescription(),
                InitializationData = Encoding.ASCII.GetBytes(crawlerData),
                InstanceCount = 1,
            };

            serviceDescriptions.Add(statelessServiceDescription);
            // Further below we create the instances of the service.
            foreach (var serivceDescription in serviceDescriptions)
            {
                try
                {
                    await _fabricClient.ServiceManager.CreateServiceAsync(serivceDescription);
                }
                catch (Exception e)
                {
                    var b = e;
                }
            }
        }

        public async Task DeleteAgentForHostnameAsync(string serviceName)
        {
            var agentUri = new Uri($"{_statelessServiceContext.CodePackageActivationContext.ApplicationName}/{serviceName}");
            var deleteDescription = new DeleteServiceDescription(agentUri) { ForceDelete = true };

            await _fabricClient.ServiceManager.DeleteServiceAsync(deleteDescription);
        }
    }
}
