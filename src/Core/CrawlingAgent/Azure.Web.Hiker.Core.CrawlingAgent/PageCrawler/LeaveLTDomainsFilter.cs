using System;
using System.Collections.Generic;

namespace Azure.Web.Hiker.Core.CrawlingAgent.PageCrawler
{
    public class LeaveLTDomainsFilter : IPageLinksFilter
    {
        public IEnumerable<Uri> FilterLinks(IEnumerable<Uri> urls)
        {
            var filteredList = new List<Uri>();

            foreach (var url in urls)
            {
                if (url.Host.Contains(".lt"))
                {
                    filteredList.Add(url);
                }
            }

            return filteredList;
        }
    }
}
