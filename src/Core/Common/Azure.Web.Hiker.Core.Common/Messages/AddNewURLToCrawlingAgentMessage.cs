using Azure.Web.Hiker.Core.Common.QueueClient;

namespace Azure.Web.Hiker.Core.Common.Messages
{
    public class AddNewURLToCrawlingAgentMessage : CommonNewUrlMessage, IBaseMessage
    {
        public AddNewURLToCrawlingAgentMessage(string url) : base(url)
        {
        }
    }
}
