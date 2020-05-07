using Azure.Web.Hiker.Core.Common.QueueClient;

namespace Azure.Web.Hiker.Core.Common.Messages
{
    public class FrontQueueNewURLMessage : CommonNewUrlMessage, IBaseMessage
    {
        public FrontQueueNewURLMessage(string url) : base(url)
        {

        }
    }
}
