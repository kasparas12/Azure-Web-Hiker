using System;

using Azure.Web.Hiker.Core.Common.QueueClient;

using Newtonsoft.Json;

namespace Azure.Web.Hiker.Core.Common.Messages
{
    public abstract class CommonNewUrlMessage : IBaseMessage
    {
        public CommonNewUrlMessage()
        {

        }

        public CommonNewUrlMessage(string url, DateTime? crawlingDateTime = null)
        {
            NewUrl = url;
            CrawlingDateTime = crawlingDateTime;
        }

        [JsonProperty("newUrl")]
        public string NewUrl { get; set; }

        [JsonProperty("crawlingDateTime")]
        public DateTime? CrawlingDateTime { get; set; }

        public string GetHostOfPage()
        {
            var hostUri = new Uri(NewUrl);
            return hostUri.Host;
        }
    }
}
