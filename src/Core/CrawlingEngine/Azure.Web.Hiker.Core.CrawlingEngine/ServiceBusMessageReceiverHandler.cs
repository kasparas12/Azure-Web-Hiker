using System.Threading;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.AgentRegistrar.Services;
using Azure.Web.Hiker.Core.CrawlingEngine.Extensions;
using Azure.Web.Hiker.Core.CrawlingEngine.Messages;
using Azure.Web.Hiker.Core.CrawlingEngine.Services;

using Microsoft.Azure.ServiceBus;

using ServiceFabric.ServiceBus.Services.Netstd;
using ServiceFabric.ServiceBus.Services.Netstd.CommunicationListeners;

namespace Azure.Web.Hiker.Core.CrawlingEngine
{
    public class ServiceBusMessageReceiverHandler : DefaultServiceBusMessageReceiver
    {
        private readonly IAgentRegistrarService _agentRegistrarService;
        private readonly IAgentController _agentController;
        private readonly IAgentProcessingQueueCreator _agentProcessingQueueCreator;
        public ServiceBusMessageReceiverHandler(
            IServiceBusCommunicationListener communicationListener,
            IAgentRegistrarService agentRegistrarService,
            IAgentController agentController,
            IAgentProcessingQueueCreator agentProcessingQueueCreator) : base(communicationListener)
        {
            _agentRegistrarService = agentRegistrarService;
            _agentController = agentController;
            _agentProcessingQueueCreator = agentProcessingQueueCreator;
        }

        protected override async Task ReceiveMessageImplAsync(Message message, CancellationToken cancellationToken)
        {
            var newAgentURL = message.GetDeserializedMessage<CreateNewAgentMessage>();

            if (!_agentRegistrarService.AgentExistsForGivenHostName(newAgentURL.GetHostOfPage()))
            {
                var createdEntry = _agentRegistrarService.CreateNewAgentRegistrarForHostName(newAgentURL.GetHostOfPage());

                await _agentProcessingQueueCreator.CreateNewProcessingQueueForAgent(newAgentURL.GetHostOfPage());
                await _agentController.SpawnNewAgentForHostnameAsync(createdEntry.AgentHost, createdEntry.AgentName);
            }
        }
    }
}
