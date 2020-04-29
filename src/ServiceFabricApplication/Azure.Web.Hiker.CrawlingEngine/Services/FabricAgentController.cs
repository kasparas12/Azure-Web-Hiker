using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Text;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.Models;
using Azure.Web.Hiker.Core.Services.AgentController;

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

        public async Task SpawnNewAgentForHostnameAsync(string hostname, string serviceName)
        {
            var crawlerData = JsonConvert.SerializeObject(new CrawlerAgentInitializationData { AssignedHostNmae = hostname });

            var serviceDescriptions = new List<StatelessServiceDescription>();
            var statelessServiceDescription = new StatelessServiceDescription()
            {
                ApplicationName = new Uri(_statelessServiceContext.CodePackageActivationContext.ApplicationName),
                ServiceName = new Uri($"{_statelessServiceContext.CodePackageActivationContext.ApplicationName}/{serviceName}"),
                ServiceTypeName = "Azure.Web.Hiker.ServiceFabricApplication.CrawlingAgentType",
                PartitionSchemeDescription = new SingletonPartitionSchemeDescription(),
                InitializationData = Encoding.ASCII.GetBytes(crawlerData),
                InstanceCount = 1,
            };

            serviceDescriptions.Add(statelessServiceDescription);

            // Further below we create the instances of the service.
            foreach (var serivceDescription in serviceDescriptions)
            {
                await _fabricClient.ServiceManager.CreateServiceAsync(serivceDescription);
            }
        }
    }
}
