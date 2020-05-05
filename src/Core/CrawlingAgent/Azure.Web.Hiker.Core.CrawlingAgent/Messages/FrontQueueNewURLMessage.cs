using Azure.Web.Hiker.Core.Common.Messages;
using Azure.Web.Hiker.Core.Common.QueueClient;

namespace Azure.Web.Hiker.Core.CrawlingAgent.Messages
{
    public class FrontQueueNewURLMessage : CommonNewUrlMessage, IBaseMessage
    {
    }
}
