using Azure.Web.Hiker.Core.Common.QueueClient;

using Newtonsoft.Json;

namespace Azure.Web.Hiker.Core.CrawlingEngine.Messages
{
    public class StopCrawlProcessMessage : IBaseMessage
    {
        [JsonProperty("stopCrawling")]
        public bool StopCrawling { get; set; }
        public StopCrawlProcessMessage()
        {

        }
    }
}
