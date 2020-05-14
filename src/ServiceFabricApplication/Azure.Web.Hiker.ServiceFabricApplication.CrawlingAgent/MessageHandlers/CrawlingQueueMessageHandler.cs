using System.Threading;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.Common.Messages;
using Azure.Web.Hiker.Core.CrawlingAgent.Models;
using Azure.Web.Hiker.Core.CrawlingAgent.PageCrawler;
using Azure.Web.Hiker.Core.CrawlingAgent.PageIndexer;
using Azure.Web.Hiker.Infrastructure.ServiceBusClient.Extensions;
using Azure.Web.Hiker.Infrastructure.ServiceFabric;

using Microsoft.Azure.ServiceBus;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingAgent.MessageHandlers
{
    public class CrawlingQueueMessageHandler : IMessageHandler
    {
        private readonly IPageCrawler _pageCrawler;
        private readonly IPageIndexer _pageIndexer;
        private readonly ICrawlingAgentHost _crawlingAgentHost;
        public CrawlingQueueMessageHandler(IPageIndexer pageIndexer, IPageCrawler pageCrawler, ICrawlingAgentHost crawlingAgentHost)
        {
            _pageIndexer = pageIndexer;
            _pageCrawler = pageCrawler;
            _crawlingAgentHost = crawlingAgentHost;
        }

        public async Task ReceiveMessageAsync(Message message, CancellationToken cancellationToken)
        {
            var pageToQueueMessage = message.GetDeserializedMessage<AddNewURLToCrawlingAgentMessage>();

            if (pageToQueueMessage.GetHostOfPage() != _crawlingAgentHost.AssignedHostName)
            {
                return;
            }

            if (await _pageIndexer.IsPageUnvisitedAsync(pageToQueueMessage.NewUrl))
            {
                await _pageCrawler.CrawlGivenWebPageAsync(pageToQueueMessage.NewUrl);
            }
        }
    }

}