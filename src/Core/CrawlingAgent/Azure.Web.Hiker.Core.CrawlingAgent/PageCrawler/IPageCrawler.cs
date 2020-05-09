using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Azure.Web.Hiker.Core.CrawlingAgent.PageCrawler
{
    public interface IPageCrawlResult
    {
        public HttpStatusCode? StatusCode { get; set; }
        public IEnumerable<Uri> PageLinks { get; set; }
        public string HTMLContent { get; set; }
        public double ElapsedMilliseconds { get; set; }
        public string DisallowedCrawlingMessage { get; set; }
    }

    public interface IPageCrawler
    {
        Task CrawlGivenWebPageAsync(string pageUrl);
    }
}
