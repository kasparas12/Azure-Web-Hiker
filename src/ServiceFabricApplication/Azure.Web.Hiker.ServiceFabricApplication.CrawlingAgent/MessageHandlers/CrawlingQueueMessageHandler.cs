using System.Threading;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.Common.Messages;
using Azure.Web.Hiker.Core.CrawlingAgent.Models;
using Azure.Web.Hiker.Core.CrawlingAgent.PageCrawler;
using Azure.Web.Hiker.Core.CrawlingAgent.PageIndexer;
using Azure.Web.Hiker.Infrastructure.ServiceBusClient.Extensions;

using Microsoft.Azure.ServiceBus;

using ServiceFabric.ServiceBus.Services.Netstd;
using ServiceFabric.ServiceBus.Services.Netstd.CommunicationListeners;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingAgent.MessageHandlers
{
    public class CrawlingQueueMessageHandler : DefaultServiceBusMessageReceiver
    {
        private readonly IPageCrawler _pageCrawler;
        private readonly IPageIndexer _pageIndexer;
        private readonly ICrawlingAgentHost _crawlingAgentHost;
        public CrawlingQueueMessageHandler(
            IServiceBusCommunicationListener communicationListener, IPageIndexer pageIndexer, IPageCrawler pageCrawler, ICrawlingAgentHost crawlingAgentHost) : base(communicationListener)
        {
            _pageIndexer = pageIndexer;
            _pageCrawler = pageCrawler;
            _crawlingAgentHost = crawlingAgentHost;
        }

        protected override async Task ReceiveMessageImplAsync(Message message, CancellationToken cancellationToken)
        {
            var pageToQueueMessage = message.GetDeserializedMessage<AddNewURLToCrawlingAgentMessage>();

            if (pageToQueueMessage.GetHostOfPage() != _crawlingAgentHost.AssignedHostName)
            {
                return;
            }

            if (await _pageIndexer.IsPageUnvisitedAsync(pageToQueueMessage.NewUrl))
            {
                var crawlResult = await _pageCrawler.CrawlGivenWebPageAsync(pageToQueueMessage.NewUrl);

                await _pageIndexer.ProcessCrawledLinksAsync(crawlResult.PageLinks, _crawlingAgentHost.AssignedHostName);
                await _pageIndexer.MarkPageAsVisitedAsync(pageToQueueMessage.NewUrl);
            }
        }
    }

}