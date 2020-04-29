using System.Threading;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.Models.Messages;
using Azure.Web.Hiker.Core.Services.AgentController;
using Azure.Web.Hiker.Core.Services.AgentRegistrar;
using Azure.Web.Hiker.ServiceFabricApplication.Extensions;

using Microsoft.Azure.ServiceBus;

using ServiceFabric.ServiceBus.Services.Netstd;
using ServiceFabric.ServiceBus.Services.Netstd.CommunicationListeners;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine
{
    public class ServiceBusMessageReceiverHandler : DefaultServiceBusMessageReceiver
    {
        private readonly IAgentRegistrarService _agentRegistrarService;
        private readonly IAgentController _agentController;
        public ServiceBusMessageReceiverHandler(
            IServiceBusCommunicationListener communicationListener,
            IAgentRegistrarService agentRegistrarService,
            IAgentController agentController) : base(communicationListener)
        {
            _agentRegistrarService = agentRegistrarService;
            _agentController = agentController;
        }

        protected override async Task ReceiveMessageImplAsync(Message message, CancellationToken cancellationToken)
        {
            var urlToCrawl = message.GetDeserializedMessage<URLToCrawlMessage>();

            bool agentIsRegisteredInRegisrar = _agentRegistrarService.AgentExistsForGivenHostName(urlToCrawl.GetHostOfPage());

            if (!agentIsRegisteredInRegisrar)
            {
                var createdEntry = _agentRegistrarService.CreateNewAgentRegistrarForHostName(urlToCrawl.GetHostOfPage());

                await _agentController.SpawnNewAgentForHostnameAsync(createdEntry.AgentHost, createdEntry.AgentName);
            }
        }
    }
}
