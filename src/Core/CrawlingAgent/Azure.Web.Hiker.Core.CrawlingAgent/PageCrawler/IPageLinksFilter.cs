using System;
using System.Collections.Generic;

namespace Azure.Web.Hiker.Core.CrawlingAgent.PageCrawler
{
    public interface IPageLinksFilter
    {
        IEnumerable<Uri> FilterLinks(IEnumerable<Uri> urls);
    }
}
