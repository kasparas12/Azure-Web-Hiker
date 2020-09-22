using System;
using System.Threading;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.Common.Extensions;
using Azure.Web.Hiker.Core.Common.Messages;
using Azure.Web.Hiker.Core.Common.QueueClient;
using Azure.Web.Hiker.Core.CrawlingAgent.Models;
using Azure.Web.Hiker.Core.CrawlingAgent.PageCrawler;
using Azure.Web.Hiker.Core.CrawlingAgent.PageIndexer;
using Azure.Web.Hiker.Infrastructure.Abot2Crawler.Models;
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
        private readonly IWebCrawlerQueueClient _webCrawlerQueueClient;
        private readonly IPolitenessDeterminer _politenessDeterminer;

        public CrawlingQueueMessageHandler(IPageIndexer pageIndexer, IPageCrawler pageCrawler, ICrawlingAgentHost crawlingAgentHost, IWebCrawlerQueueClient webCrawlerQueueClient, IPolitenessDeterminer politenessDeterminer)
        {
            _pageIndexer = pageIndexer;
            _pageCrawler = pageCrawler;
            _crawlingAgentHost = crawlingAgentHost;
            _webCrawlerQueueClient = webCrawlerQueueClient;
            _politenessDeterminer = politenessDeterminer;
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
                var crawlDelay = await CalculateCrawlDelayAsync(pageToQueueMessage.NewUrl);

                if (crawlDelay == -1)
                {
                    var crawlResultItem = new AbotCrawlResult("CalculateDelayProhibited");
                    await _pageIndexer.MarkPageAsVisitedAsync(pageToQueueMessage.NewUrl, crawlResultItem);
                    return;
                }

                if (!await InvestigateCrawlingTimeAsync(pageToQueueMessage, crawlDelay))
                {
                    return;
                }

                await _pageCrawler.CrawlGivenWebPageAsync(pageToQueueMessage.NewUrl, crawlDelay);
            }
        }

        private async Task<double> CalculateCrawlDelayAsync(string pageUrl)
        {
            var crawlDelay = await _politenessDeterminer.CalculateHostCrawlDelayAsync(new Uri(pageUrl));

            return crawlDelay;
        }

        private async Task<bool> InvestigateCrawlingTimeAsync(AddNewURLToCrawlingAgentMessage message, double crawlDelay)
        {
            if (message.CrawlingDateTime is null)
            {
                message.CrawlingDateTime = DateTime.UtcNow.AddSeconds(crawlDelay);
                await _webCrawlerQueueClient.SendMessageToCrawlingAgentProcessingQueue(message, message.NewUrl.GetHostOfUrl());
                return false;
            }
            else
            {
                if (message.CrawlingDateTime >= DateTime.UtcNow)
                {
                    var difference = message.CrawlingDateTime.Value - DateTime.UtcNow;
                    await Task.Delay((int)Math.Round(difference.TotalMilliseconds));
                }
                return true;
            }
        }
    }

}