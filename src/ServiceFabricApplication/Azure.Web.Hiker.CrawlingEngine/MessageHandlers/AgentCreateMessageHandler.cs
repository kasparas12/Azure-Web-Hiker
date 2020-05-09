namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;

    using global::Azure.Web.Hiker.Core.AgentRegistrar.Services;
    using global::Azure.Web.Hiker.Core.Common.Extensions;
    using global::Azure.Web.Hiker.Core.Common.Messages;
    using global::Azure.Web.Hiker.Core.Common.QueueClient;
    using global::Azure.Web.Hiker.Core.CrawlingAgent.PageIndexer;
    using global::Azure.Web.Hiker.Infrastructure.ServiceBusClient.Extensions;

    using Microsoft.Azure.ServiceBus;

    namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine.MessageHandlers
    {
        public class AgentCreateMessageHandler : IMessageHandler
        {
            private readonly IAgentRegistrarService _agentRegistrarService;
            private readonly IPageIndexer _pageIndexer;
            private readonly IWebCrawlerQueueClient _webCrawlerQueueClient;

            public AgentCreateMessageHandler(IAgentRegistrarService agentRegistrarService, IPageIndexer pageIndexer, IWebCrawlerQueueClient webCrawlerQueueClient)
            {
                _agentRegistrarService = agentRegistrarService;
                _pageIndexer = pageIndexer;
                _webCrawlerQueueClient = webCrawlerQueueClient;
            }

            public async Task ReceiveMessageAsync(Message message, CancellationToken cancellationToken)
            {
                var newAgentRequestMessage = message.GetDeserializedMessage<CreateNewAgentForURLMessage>();

                if (string.IsNullOrWhiteSpace(newAgentRequestMessage.NewUrl))
                {
                    return;
                }

                if (!await _pageIndexer.IsPageUnvisitedAsync(newAgentRequestMessage.NewUrl))
                {
                    return;
                }

                if (_agentRegistrarService.AgentExistsForGivenHostName(newAgentRequestMessage.NewUrl))
                {
                    await _webCrawlerQueueClient.SendMessageToCrawlingAgentProcessingQueue(new AddNewURLToCrawlingAgentMessage(newAgentRequestMessage.NewUrl), newAgentRequestMessage.NewUrl.GetHostOfUrl());
                }
                else
                {
                    await TryToCreateNewAgentForUrlAsync(newAgentRequestMessage.NewUrl);
                }

            }

            private async Task TryToCreateNewAgentForUrlAsync(string url)
            {
                await _agentRegistrarService.CreateNewAgentForHostName(url.GetHostOfUrl());
                await _webCrawlerQueueClient.SendMessageToCrawlingAgentProcessingQueue(new AddNewURLToCrawlingAgentMessage(url), url.GetHostOfUrl());
            }
        }
    }

}
