using System.Threading;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.AgentRegistrar.Services;
using Azure.Web.Hiker.Core.Common.Messages;
using Azure.Web.Hiker.Core.Common.QueueClient;
using Azure.Web.Hiker.Core.CrawlingAgent.Messages;
using Azure.Web.Hiker.Infrastructure.ServiceBusClient.Extensions;

using Microsoft.Azure.ServiceBus;

using ServiceFabric.ServiceBus.Services.Netstd;
using ServiceFabric.ServiceBus.Services.Netstd.CommunicationListeners;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingAgent.MessageHandlers
{
    public class FrontQueueMessageHandler : DefaultServiceBusMessageReceiver
    {
        private readonly IAgentRegistrarService _agentRegistrarService;
        private readonly IWebCrawlerQueueClient _webCrawlerQueueClient;
        public FrontQueueMessageHandler(
            IServiceBusCommunicationListener communicationListener,
            IAgentRegistrarService agentRegistrarService,
            IWebCrawlerQueueClient webCrawlerQueueClient) : base(communicationListener)
        {
            _agentRegistrarService = agentRegistrarService;
            _webCrawlerQueueClient = webCrawlerQueueClient;
        }

        protected override async Task ReceiveMessageImplAsync(Message message, CancellationToken cancellationToken)
        {
            var frontQueueMessage = message.GetDeserializedMessage<FrontQueueNewURLMessage>();

            bool activeAgentRegistered = _agentRegistrarService.AgentExistsForGivenHostName(frontQueueMessage.GetHostOfPage());

            if (activeAgentRegistered)
            {
                await SendURLToCrawlingAgentProcessingQueue(frontQueueMessage.NewUrl, frontQueueMessage.GetHostOfPage());
            }
            else
            {
                await SendRequestToCreateNewAgentForHost(frontQueueMessage.NewUrl);
            }
        }

        private async Task SendURLToCrawlingAgentProcessingQueue(string url, string host)
        {
            await _webCrawlerQueueClient.SendMessageToCrawlingAgentProcessingQueue(new AddNewURLToCrawlingAgentMessage(url), host);
        }

        private async Task SendRequestToCreateNewAgentForHost(string url)
        {
            await _webCrawlerQueueClient.SendMessageToCreateNewAgentQueue(new CreateNewAgentMessage(url));
        }
    }
}
