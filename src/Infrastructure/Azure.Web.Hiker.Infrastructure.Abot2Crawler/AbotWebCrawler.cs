using System;
using System.Threading.Tasks;

using Abot2.Crawler;
using Abot2.Poco;

using Azure.Web.Hiker.Core.CrawlingAgent.PageCrawler;
using Azure.Web.Hiker.Infrastructure.Abot2Crawler.Models;

namespace Azure.Web.Hiker.Infrastructure.Abot2Crawler
{
    public class AbotWebCrawler : IPageCrawler
    {
        private CrawledPage _crawledPage;

        public async Task<IPageCrawlResult> CrawlGivenWebPageAsync(string pageUrl)
        {
            var config = new CrawlConfiguration
            {
                MaxPagesToCrawl = 1,
                MaxCrawlDepth = 3,
                MinCrawlDelayPerDomainMilliSeconds = 3000 //Wait this many millisecs between requests
            };
            var crawler = new PoliteWebCrawler(config);

            crawler.PageCrawlCompleted += PageCrawlCompleted;//Several events available...

            var crawlResult = await crawler.CrawlAsync(new Uri(pageUrl));

            return new AbotCrawlResult(_crawledPage);
        }

        private void PageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            _crawledPage = e.CrawledPage;
        }
    }
}
