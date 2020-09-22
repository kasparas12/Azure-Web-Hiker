using Azure.Web.Hiker.Core.Common.QueueClient;

using Newtonsoft.Json;

namespace Azure.Web.Hiker.Core.CrawlingEngine.Messages
{
    public class StartCrawlProcessMessage : IBaseMessage
    {
        [JsonProperty("startCrawling")]
        public bool StartCrawling { get; set; }
        public StartCrawlProcessMessage()
        {

        }
    }
}
