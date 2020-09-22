using System;
using System.Threading;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.AgentRegistrar.Persistence;
using Azure.Web.Hiker.Core.Common.Extensions;
using Azure.Web.Hiker.Core.CrawlingAgent.PageIndexer;
using Azure.Web.Hiker.Core.RenderingAgent;
using Azure.Web.Hiker.Core.RenderingAgent.Messages;
using Azure.Web.Hiker.Infrastructure.ServiceBusClient.Extensions;
using Azure.Web.Hiker.Infrastructure.ServiceFabric;

using Microsoft.Azure.ServiceBus;

namespace RenderAgent.Handlers
{
    public class RenderingAgentQueueProcessingHandler : IMessageHandler
    {
        private readonly IWebsiteRenderer _websiteRenderer;
        private readonly IPageIndexer _pageIndexer;
        private readonly IRenderingAgentRepository _renderingAgentRepository;

        public RenderingAgentQueueProcessingHandler(IWebsiteRenderer websiteRenderer, IPageIndexer pageIndexer, IRenderingAgentRepository renderingAgentRepository)
        {
            _websiteRenderer = websiteRenderer;
            _pageIndexer = pageIndexer;
            _renderingAgentRepository = renderingAgentRepository;
        }

        public async Task ReceiveMessageAsync(Message message, CancellationToken cancellationToken)
        {
            var renderMessage = message.GetDeserializedMessage<RenderingQueueMessage>();

            bool isPageAlreadyRendered = await _pageIndexer.IsPageRenderedAsync(renderMessage.UrlToRender).ConfigureAwait(false);

            if (isPageAlreadyRendered)
            {
                return;
            }

            var result = await _websiteRenderer.RenderPageAsync(new Uri(renderMessage.UrlToRender), 100000).ConfigureAwait(false);

            // await _pageIndexer.ProcessCrawledLinksAsync(result.NewDiscoveredLinks, renderMessage.UrlToRender.GetHostOfUrl());

            _renderingAgentRepository.UpdateAgentActivityTime(renderMessage.UrlToRender.GetHostOfUrl(), DateTime.UtcNow);

            await _pageIndexer.MarkPageAsRenderedAsync(renderMessage.UrlToRender, result.Status).ConfigureAwait(false);
        }
    }
}
