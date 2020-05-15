using System;
using System.Collections.Generic;

using Azure.Web.Hiker.Core.Common.Extensions;

namespace Azure.Web.Hiker.Core.CrawlingAgent.PageCrawler
{
    public class AvoidPopularSitesFilter : IPageLinksFilter
    {
        private readonly string[] PopularSites = { "facebook", "youtube", "google", "goo.gl", "twitter", "instagram", "delfi", "lrytas", "lrt", "15min" };
        public IEnumerable<Uri> FilterLinks(IEnumerable<Uri> urls)
        {
            var filteredList = new List<Uri>();

            foreach (var url in urls)
            {
                if (!url.AbsoluteUri.ToLower().ContainsAny(PopularSites))
                {
                    filteredList.Add(url);
                }
            }

            return filteredList;
        }
    }
}
