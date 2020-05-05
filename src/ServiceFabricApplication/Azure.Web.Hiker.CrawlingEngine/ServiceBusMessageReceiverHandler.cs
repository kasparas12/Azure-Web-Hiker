using System.Threading;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.AgentRegistrar.Services;
using Azure.Web.Hiker.Core.Common.Messages;
using Azure.Web.Hiker.Core.Common.QueueClient;
using Azure.Web.Hiker.Core.CrawlingEngine.Services;
using Azure.Web.Hiker.Infrastructure.ServiceBusClient.Extensions;

using Microsoft.Azure.ServiceBus;

using ServiceFabric.ServiceBus.Services.Netstd;
using ServiceFabric.ServiceBus.Services.Netstd.CommunicationListeners;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine
{
    public class ServiceBusMessageReceiverHandler : DefaultServiceBusMessageReceiver
    {
        private readonly IAgentRegistrarService _agentRegistrarService;
        private readonly IAgentController _agentController;
        private readonly IAgentProcessingQueueCreator _agentProcessingQueueCreator;
        private readonly IWebCrawlerQueueClient _webCrawlerQueueClient;
        public ServiceBusMessageReceiverHandler(
            IServiceBusCommunicationListener communicationListener,
            IAgentRegistrarService agentRegistrarService,
            IAgentController agentController,
            IAgentProcessingQueueCreator agentProcessingQueueCreator,
            IWebCrawlerQueueClient webCrawlerQueueClient) : base(communicationListener)
        {
            _agentRegistrarService = agentRegistrarService;
            _agentController = agentController;
            _agentProcessingQueueCreator = agentProcessingQueueCreator;
            _webCrawlerQueueClient = webCrawlerQueueClient;
        }

        protected override async Task ReceiveMessageImplAsync(Message message, CancellationToken cancellationToken)
        {
            var newAgentURL = message.GetDeserializedMessage<CreateNewAgentMessage>();

            if (!_agentRegistrarService.AgentExistsForGivenHostName(newAgentURL.GetHostOfPage()))
            {
                var createdEntry = _agentRegistrarService.CreateNewAgentRegistrarForHostName(newAgentURL.GetHostOfPage());

                await _agentProcessingQueueCreator.CreateNewProcessingQueueForAgent(newAgentURL.GetHostOfPage());
                await _agentController.SpawnNewAgentForHostnameAsync(createdEntry.AgentHost, createdEntry.AgentName);
                await _webCrawlerQueueClient.SendMessageToCrawlingAgentProcessingQueue(new AddNewURLToCrawlingAgentMessage(newAgentURL.NewUrl), newAgentURL.GetHostOfPage());
            }
        }
    }
}
