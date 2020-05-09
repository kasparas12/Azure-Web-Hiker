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
                var host = url.Host;
                int index = host.LastIndexOf('.'), last = 3;
                while (index > 0 && index >= last - 3)
                {
                    last = index;
                    index = host.LastIndexOf('.', last - 1);
                }
                var domain = host.Substring(index + 1);

                if (domain.ToLower() == "lt")
                {
                    filteredList.Add(url);
                }
            }

            return filteredList;
        }
    }
}
