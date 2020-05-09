using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.CrawlingAgent.PageCrawler;

namespace Azure.Web.Hiker.Core.CrawlingAgent.PageIndexer
{
    public interface IPageIndexer
    {
        Task<bool> IsPageUnvisitedAsync(string url);
        Task<bool> IsPageExistingInIndex(string url);
        Task MarkPageAsVisitedAsync(string url, IPageCrawlResult crawlResult);
        Task ProcessCrawledLinksAsync(IEnumerable<Uri> crawledLinks, string crawlerHost);
    }
}
