using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.Common.Messages;
using Azure.Web.Hiker.Core.Common.QueueClient;
using Azure.Web.Hiker.Core.CrawlingAgent.Messages;
using Azure.Web.Hiker.Core.IndexStorage.Interfaces;
using Azure.Web.Hiker.Core.IndexStorage.Models;

namespace Azure.Web.Hiker.Core.CrawlingAgent.PageIndexer
{
    public class PageIndexer : IPageIndexer
    {
        private readonly IPageIndexStorageRepository _pageIndexStorageRepository;
        private readonly IWebCrawlerQueueClient _webCrawlerQueueClient;

        public PageIndexer(IPageIndexStorageRepository pageIndexStorageRepository, IWebCrawlerQueueClient webCrawlerQueueClient)
        {
            _pageIndexStorageRepository = pageIndexStorageRepository;
            _webCrawlerQueueClient = webCrawlerQueueClient;
        }

        public async Task<bool> IsPageUnvisitedAsync(string url)
        {
            var pageIndex = await _pageIndexStorageRepository.GetPageIndexByUrl(url);

            if (pageIndex is null)
            {
                await _pageIndexStorageRepository.InsertOrMergeNewPageIndex(new PageIndex(url, 0, false, null));
                return true;
            }
            else
            {
                if (!pageIndex.Visited)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task MarkPageAsVisitedAsync(string url)
        {
            var pageIndex = await _pageIndexStorageRepository.GetPageIndexByUrl(url);

            if (!(pageIndex is null))
            {
                pageIndex.Visited = true;
                pageIndex.VisitedTimestamp = DateTime.UtcNow;

                await _pageIndexStorageRepository.InsertOrMergeNewPageIndex(pageIndex);
            }
        }

        public async Task ProcessCrawledLinksAsync(IEnumerable<Uri> crawledLinks, string crawlerHost)
        {
            int index = 0;

            if (crawledLinks is null)
            {
                return;
            }

            foreach (var link in crawledLinks)
            {
                if (link.Host != crawlerHost)
                {
                    await SendExternalLinkToFrontQueueAsync(link);
                    continue;
                }

                var hitCount = await _webCrawlerQueueClient.GetNumberOfSameLinkMessagesInCrawlingAgentProcessingQueue<AddNewURLToCrawlingAgentMessage>(link);

                if (hitCount > 0)
                {
                    await UpdateIndexHitCount(link, hitCount);
                }
                else
                {
                    await _webCrawlerQueueClient.SendScheduledMessageToCrawlingAgentProcessingQueue(new AddNewURLToCrawlingAgentMessage(link.AbsoluteUri), link.Host, DateTime.UtcNow.AddSeconds(5 * (index + 1)));
                    await UpdateIndexHitCount(link, hitCount);
                }

                index++;
            }
        }

        private async Task SendExternalLinkToFrontQueueAsync(Uri externalLink)
        {
            await _webCrawlerQueueClient.SendMessageToCrawlingFrontQueue(new FrontQueueNewURLMessage(externalLink.AbsoluteUri));
        }

        private async Task UpdateIndexHitCount(Uri link, int incrementHitNumber)
        {
            var pageIndex = await _pageIndexStorageRepository.GetPageIndexByUrl(link.AbsoluteUri);

            if (pageIndex is null)
            {
                await _pageIndexStorageRepository.InsertOrMergeNewPageIndex(new PageIndex(link.AbsoluteUri, incrementHitNumber, false, null));
            }
            else
            {
                pageIndex.HitCount += incrementHitNumber;
                await _pageIndexStorageRepository.InsertOrMergeNewPageIndex(pageIndex);
            }
        }
    }
}