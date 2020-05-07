using System;

using Azure.Web.Hiker.Core.IndexStorage.Models;

using Microsoft.Azure.Cosmos.Table;

namespace Azure.Web.Hiker.Infrastructure.Persistence.AzureStorageTable.Models
{
    public class PageIndex : TableEntity, IPageIndex
    {
        public PageIndex()
        {

        }

        public PageIndex(string url, int hitcount, bool visited)
        {
            PageUrl = url;
            HitCount = hitcount;
            Visited = visited;
        }
        public PageIndex(string url, int hitcount, bool visited, DateTime? visitedAt) : this(url, hitcount, visited)
        {
            VisitedTimestamp = visitedAt;
        }

        public PageIndex(string url, int hitcount, bool visited, DateTime? visitedAt, int? statusCode, string disallowedReason) : this(url, hitcount, visited, visitedAt)
        {
            StatusCode = statusCode;
            DisallowedCrawlReason = disallowedReason;
        }
        public string PageUrl { get; set; }
        public int HitCount { get; set; }
        public bool Visited { get; set; }
        public DateTime? VisitedTimestamp { get; set; }
        public int? StatusCode { get; set; }
        public string DisallowedCrawlReason { get; set; }
    }
}
