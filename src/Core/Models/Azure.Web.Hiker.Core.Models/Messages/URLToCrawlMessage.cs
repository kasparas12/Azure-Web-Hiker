using System;

using Newtonsoft.Json;

namespace Azure.Web.Hiker.Core.Models.Messages
{
    public class URLToCrawlMessage
    {
        public URLToCrawlMessage()
        {

        }

        public URLToCrawlMessage(string url)
        {
            PageToCrawl = url;
        }

        [JsonProperty("pageToCrawl")]
        public string PageToCrawl { get; set; }

        public string GetHostOfPage()
        {
            var hostUri = new Uri(PageToCrawl);
            return hostUri.Host;
        }
    }
}
