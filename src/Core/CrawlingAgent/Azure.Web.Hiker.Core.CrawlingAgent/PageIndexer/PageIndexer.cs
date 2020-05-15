using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.AgentRegistrar.Services;
using Azure.Web.Hiker.Core.Common.Extensions;
using Azure.Web.Hiker.Core.Common.Messages;
using Azure.Web.Hiker.Core.Common.QueueClient;
using Azure.Web.Hiker.Core.CrawlingAgent.PageCrawler;
using Azure.Web.Hiker.Core.IndexStorage.Interfaces;
using Azure.Web.Hiker.Core.IndexStorage.Models;
using Azure.Web.Hiker.Core.RenderingAgent;

namespace Azure.Web.Hiker.Core.CrawlingAgent.PageIndexer
{
    public class PageIndexer : IPageIndexer
    {
        private readonly IPageIndexStorageRepository _pageIndexStorageRepository;
        private readonly IWebCrawlerQueueClient _webCrawlerQueueClient;
        private readonly IAgentRegistrarService _agentRegistrarService;
        private readonly IEnumerable<IPageLinksFilter> _filters;

        public PageIndexer(IPageIndexStorageRepository pageIndexStorageRepository, IWebCrawlerQueueClient webCrawlerQueueClient, IAgentRegistrarService agentRegistrarService, IEnumerable<IPageLinksFilter> filters)
        {
            _pageIndexStorageRepository = pageIndexStorageRepository;
            _webCrawlerQueueClient = webCrawlerQueueClient;
            _agentRegistrarService = agentRegistrarService;
            _filters = filters;
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
        public async Task<bool> IsPageExistingInIndex(string url)
        {
            var pageIndex = await _pageIndexStorageRepository.GetPageIndexByUrl(url);

            if (pageIndex is null)
            {
                await _pageIndexStorageRepository.InsertOrMergeNewPageIndex(new PageIndex(url, 0, false, null));
                return false;
            }

            return true;
        }

        public async Task MarkPageAsVisitedAsync(string url, IPageCrawlResult crawlResult)
        {
            var pageIndex = await _pageIndexStorageRepository.GetPageIndexByUrl(url);

            if (!(pageIndex is null))
            {
                pageIndex.Visited = true;
                pageIndex.VisitedTimestamp = DateTime.UtcNow;
                pageIndex.StatusCode = Convert.ToInt32(crawlResult.StatusCode);
                pageIndex.DisallowedCrawlReason = crawlResult.DisallowedCrawlingMessage;

                await _pageIndexStorageRepository.InsertOrMergeNewPageIndex(pageIndex);
            }
            else
            {

            }
        }

        public async Task ProcessCrawledLinksAsync(IEnumerable<Uri> crawledLinks, string crawlerHost)
        {
            int index = 0;

            if (crawledLinks is null)
            {
                return;
            }

            var filterResult = FilterLinks(crawledLinks);

            var nonExistingLinks = new List<string>();

            foreach (var link in filterResult)
            {
                var uri = new UriBuilder(link);
                uri.Host = link.Host.Replace("www.", "");

                if (await IsPageExistingInIndex(uri.Uri.AbsoluteUri))
                {
                    continue;
                }

                nonExistingLinks.Add(uri.Uri.AbsoluteUri);
                await _pageIndexStorageRepository.InsertOrMergeNewPageIndex(new PageIndex(uri.Uri.AbsoluteUri, 0, false));

                index++;
            }

            var sameHostNonExistingLinks = nonExistingLinks.Where(x => x.GetHostOfUrl() == crawlerHost);
            var differentHostNonExistingLinks = nonExistingLinks.Except(sameHostNonExistingLinks);

            await _webCrawlerQueueClient.SendMessagesToCrawlingAgentProcessingQueue(sameHostNonExistingLinks.Select(x => new AddNewURLToCrawlingAgentMessage(x)), crawlerHost);

            foreach (var link in differentHostNonExistingLinks)
            {
                if (_agentRegistrarService.AgentExistsForGivenHostName(link.GetHostOfUrl()))
                {
                    await _webCrawlerQueueClient.SendMessageToCrawlingAgentProcessingQueue(new AddNewURLToCrawlingAgentMessage(link), link.GetHostOfUrl());
                }
                else
                {
                    await _webCrawlerQueueClient.SendMessageToAgentCreateQueue(new CreateNewAgentForURLMessage(link));
                }
            }
        }

        private IEnumerable<Uri> FilterLinks(IEnumerable<Uri> urls)
        {
            var listOfFilteredUlrs = urls;

            foreach (var filter in _filters)
            {
                var filteredResult = filter.FilterLinks(listOfFilteredUlrs);
                listOfFilteredUlrs = filteredResult;
            }

            return listOfFilteredUlrs;
        }

        public async Task<bool> IsPageRenderedAsync(string url)
        {
            var pageIndex = await _pageIndexStorageRepository.GetPageIndexByUrl(url);
            return pageIndex != null && pageIndex.RenderStatus != null;
        }

        public async Task MarkPageAsRenderedAsync(string url, RenderStatus status)
        {

            var pageIndex = await _pageIndexStorageRepository.GetPageIndexByUrl(url);

            if (!(pageIndex is null))
            {
                pageIndex.RenderStatus = status;
                await _pageIndexStorageRepository.InsertOrMergeNewPageIndex(pageIndex);
            }
        }
    }
}