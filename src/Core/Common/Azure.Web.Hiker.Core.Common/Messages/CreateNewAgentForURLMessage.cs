using Azure.Web.Hiker.Core.Common.QueueClient;

namespace Azure.Web.Hiker.Core.Common.Messages
{
    public class CreateNewAgentForURLMessage : CommonNewUrlMessage, IBaseMessage
    {
        public CreateNewAgentForURLMessage(string url) : base(url)
        {

        }
    }
}
