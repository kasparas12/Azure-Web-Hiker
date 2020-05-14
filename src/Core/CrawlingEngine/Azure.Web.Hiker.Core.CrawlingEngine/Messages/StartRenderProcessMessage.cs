using Azure.Web.Hiker.Core.Common.QueueClient;

using Newtonsoft.Json;

namespace Azure.Web.Hiker.Core.CrawlingEngine.Messages
{
    public class StartRenderProcessMessage : IBaseMessage
    {
        [JsonProperty("startRendering")]
        public bool StartRendering { get; set; }
        public StartRenderProcessMessage()
        {

        }
    }
}
