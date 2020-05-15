using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.CrawlingAgent.PageCrawler;
using Azure.Web.Hiker.Core.RenderingAgent;

namespace Azure.Web.Hiker.Core.CrawlingAgent.PageIndexer
{
    public interface IPageIndexer
    {
        Task<bool> IsPageUnvisitedAsync(string url);
        Task<bool> IsPageExistingInIndex(string url);
        Task<bool> IsPageRenderedAsync(string url);

        Task MarkPageAsVisitedAsync(string url, IPageCrawlResult crawlResult);
        Task MarkPageAsRenderedAsync(string url, RenderStatus status);

        Task ProcessCrawledLinksAsync(IEnumerable<Uri> crawledLinks, string crawlerHost);
    }
}
