using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Abot2.Crawler;
using Abot2.Poco;

using Azure.Web.Hiker.Core.Common.Metrics;
using Azure.Web.Hiker.Core.CrawlingAgent.PageCrawler;
using Azure.Web.Hiker.Infrastructure.Abot2Crawler.Models;

namespace Azure.Web.Hiker.Infrastructure.Abot2Crawler
{
    public class AbotWebCrawler : IPageCrawler
    {
        private CrawledPage _crawledPage;
        private string _crawlerDisallowedReason;
        private readonly IHttpVisitMetricTracker _httpVisitMetricTracker;

        public AbotWebCrawler(IHttpVisitMetricTracker httpVisitMetricTracker)
        {
            _httpVisitMetricTracker = httpVisitMetricTracker;
        }

        public async Task<IPageCrawlResult> CrawlGivenWebPageAsync(string pageUrl)
        {
            var config = new CrawlConfiguration
            {
                CrawlTimeoutSeconds = 100,
                HttpRequestTimeoutInSeconds = 100,
                MaxPagesToCrawl = 1,
                MaxCrawlDepth = 3,
                MinCrawlDelayPerDomainMilliSeconds = 5000 //Wait this many millisecs between requests
            };
            var crawler = new PoliteWebCrawler(config);

            crawler.PageCrawlCompleted += PageCrawlCompleted;//Several events available...
            crawler.PageCrawlDisallowed += crawler_PageCrawlDisallowed;
            crawler.PageLinksCrawlDisallowed += crawler_PageLinksCrawlDisallowed;

            try
            {
                var crawlResult = await crawler.CrawlAsync(new Uri(pageUrl));
            }
            catch (HttpRequestException e)
            {
                if (e.InnerException is WebException webException && webException.Status == WebExceptionStatus.Timeout)
                {
                    return new AbotCrawlResult(HttpStatusCode.RequestTimeout, _crawledPage.Elapsed);
                }
            }

            if (_crawledPage is null)
            {
                return new AbotCrawlResult(_crawlerDisallowedReason);
            }

            return new AbotCrawlResult(_crawledPage);
        }

        private void PageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            _crawledPage = e.CrawledPage;
            _httpVisitMetricTracker.TrackPageVisit(e.CrawledPage.Uri, e.CrawledPage.RequestCompleted);
        }

        void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            _crawlerDisallowedReason = e.DisallowedReason;
            _httpVisitMetricTracker.TrackVisitDisallowed(e.CrawledPage.Uri, e.DisallowedReason);
        }

        private void crawler_PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
        {
            _crawlerDisallowedReason = e.DisallowedReason;
            _httpVisitMetricTracker.TrackVisitDisallowed(e.PageToCrawl.Uri, e.DisallowedReason);
        }
    }
}
