using Azure.Web.Hiker.Core.Common.QueueClient;

namespace Azure.Web.Hiker.Core.Common.Messages
{
    public class CreateNewAgentMessage : CommonNewUrlMessage, IBaseMessage
    {
        public CreateNewAgentMessage(string url) : base(url)
        {
        }
    }
}
