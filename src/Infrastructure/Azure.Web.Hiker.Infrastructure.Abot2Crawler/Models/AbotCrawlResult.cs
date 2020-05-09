using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using Abot2.Poco;

using Azure.Web.Hiker.Core.CrawlingAgent.PageCrawler;

namespace Azure.Web.Hiker.Infrastructure.Abot2Crawler.Models
{
    public class AbotCrawlResult : IPageCrawlResult
    {
        public AbotCrawlResult(string crawlerDisallowedReason)
        {
            if (!string.IsNullOrWhiteSpace(crawlerDisallowedReason))
            {
                DisallowedCrawlingMessage = crawlerDisallowedReason;
            }
        }

        public AbotCrawlResult(CrawledPage crawledPage)
        {
            if (crawledPage.HttpRequestException != null)
            {
                StatusCode = HttpStatusCode.RequestTimeout;
                ElapsedMilliseconds = crawledPage.Elapsed;
            }
            else
            {
                StatusCode = crawledPage.HttpResponseMessage.StatusCode;
                PageLinks = crawledPage.ParsedLinks != null ? crawledPage.ParsedLinks.Select(x => x.HrefValue) : null;
                HTMLContent = crawledPage.Content.Text;
                ElapsedMilliseconds = crawledPage.Elapsed;
            }
        }

        public AbotCrawlResult(HttpStatusCode statusCode, double elapsed)
        {
            StatusCode = statusCode;
            ElapsedMilliseconds = elapsed;
        }

        public string DisallowedCrawlingMessage { get; set; }

        public HttpStatusCode? StatusCode { get; set; }
        public IEnumerable<Uri> PageLinks { get; set; }
        public string HTMLContent { get; set; }
        public double ElapsedMilliseconds { get; set; }
    }
}
