using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Abot2.Crawler;
using Abot2.Poco;

using Azure.Web.Hiker.Core.AgentRegistrar.Persistence;
using Azure.Web.Hiker.Core.Common.Metrics;
using Azure.Web.Hiker.Core.CrawlingAgent.PageCrawler;
using Azure.Web.Hiker.Core.CrawlingAgent.PageIndexer;
using Azure.Web.Hiker.Infrastructure.Abot2Crawler.Models;

namespace Azure.Web.Hiker.Infrastructure.Abot2Crawler
{
    public class AbotWebCrawler : IPageCrawler
    {
        private readonly IHttpVisitMetricTracker _httpVisitMetricTracker;
        private readonly IPageIndexer _pageIndexer;
        private readonly IAgentRegistrarRepository _repository;

        public AbotWebCrawler(IHttpVisitMetricTracker httpVisitMetricTracker, IPageIndexer pageIndexer, IAgentRegistrarRepository repository)
        {
            _httpVisitMetricTracker = httpVisitMetricTracker;
            _pageIndexer = pageIndexer;
            _repository = repository;
        }

        public async Task CrawlGivenWebPageAsync(string pageUrl)
        {
            var config = new CrawlConfiguration
            {
                CrawlTimeoutSeconds = 100,
                HttpRequestTimeoutInSeconds = 100,
                MaxPagesToCrawl = 100,
                MinCrawlDelayPerDomainMilliSeconds = 1500 //Wait this many millisecs between requests
            };
            var crawler = new PoliteWebCrawler(config);

            crawler.PageCrawlCompleted += PageCrawlCompleted;//Several events available...
            crawler.PageCrawlDisallowed += crawler_PageCrawlDisallowed;
            crawler.PageLinksCrawlDisallowed += crawler_PageLinksCrawlDisallowed;

            var crawlResult = await crawler.CrawlAsync(new Uri(pageUrl));

        }

        private async void PageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {

            if (e.CrawledPage.HttpRequestException != null)
            {
                if (e.CrawledPage.HttpRequestException.InnerException is WebException webException && webException.Status == WebExceptionStatus.Timeout)
                {
                    var result = new AbotCrawlResult(HttpStatusCode.RequestTimeout, e.CrawledPage.Elapsed);
                    await _pageIndexer.MarkPageAsVisitedAsync(e.CrawledPage.Uri.AbsoluteUri, result);
                }
                else
                {
                    var result = new AbotCrawlResult(HttpStatusCode.RequestTimeout, e.CrawledPage.Elapsed);
                    await _pageIndexer.MarkPageAsVisitedAsync(e.CrawledPage.Uri.AbsoluteUri, result);
                }

                return;
            }
            var links = e.CrawledPage.ParsedLinks != null ? e.CrawledPage.ParsedLinks
                .Select(x => x.HrefValue)
                .Where(y => y.Scheme == "https" || y.Scheme == "http") : null;

            var crawlResult = new AbotCrawlResult(e.CrawledPage);

            if (links != null)
            {
                await _pageIndexer.ProcessCrawledLinksAsync(links, e.CrawledPage.Uri.Host);
            }

            await _pageIndexer.MarkPageAsVisitedAsync(e.CrawledPage.Uri.AbsoluteUri, crawlResult);

            _httpVisitMetricTracker.TrackPageVisit(e.CrawledPage.Uri, e.CrawledPage.RequestCompleted);
            _repository.UpdateAgentActivityTime(e.CrawledPage.Uri.Host, DateTime.UtcNow);
        }

        private async void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {

            var crawlResult = new AbotCrawlResult(e.DisallowedReason);
            await _pageIndexer.MarkPageAsVisitedAsync(e.CrawledPage.Uri.AbsoluteUri, crawlResult);
            _httpVisitMetricTracker.TrackVisitDisallowed(e.CrawledPage.Uri, e.DisallowedReason);
            _repository.UpdateAgentActivityTime(e.CrawledPage.Uri.Host, DateTime.UtcNow);
        }

        private async void crawler_PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
        {
            var crawlResult = new AbotCrawlResult(e.DisallowedReason);
            await _pageIndexer.MarkPageAsVisitedAsync(e.PageToCrawl.Uri.AbsoluteUri, crawlResult);

            _httpVisitMetricTracker.TrackVisitDisallowed(e.PageToCrawl.Uri, e.DisallowedReason);
            _repository.UpdateAgentActivityTime(e.PageToCrawl.Uri.Host, DateTime.UtcNow);
        }
    }
}
