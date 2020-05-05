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
        public PageIndex(string url, int hitcount, bool visited, DateTime visitedAt) : this(url, hitcount, visited)
        {
            VisitedTimestamp = visitedAt;
        }

        public string PageUrl { get; set; }
        public int HitCount { get; set; }
        public bool Visited { get; set; }
        public DateTime? VisitedTimestamp { get; set; }
    }
}
