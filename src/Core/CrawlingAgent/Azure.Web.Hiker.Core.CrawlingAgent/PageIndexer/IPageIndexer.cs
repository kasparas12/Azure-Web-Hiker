using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azure.Web.Hiker.Core.CrawlingAgent.PageIndexer
{
    public interface IPageIndexer
    {
        Task<bool> IsPageUnvisitedAsync(string url);
        Task MarkPageAsVisitedAsync(string url);
        Task ProcessCrawledLinksAsync(IEnumerable<Uri> crawledLinks, string crawlerHost);
    }
}
