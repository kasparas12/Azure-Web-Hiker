using System;

using Azure.Web.Hiker.Core.Common.QueueClient;

namespace Azure.Web.Hiker.Core.Common.Messages
{
    public class AddNewURLToCrawlingAgentMessage : CommonNewUrlMessage, IBaseMessage
    {
        public AddNewURLToCrawlingAgentMessage() : base()
        {

        }

        public AddNewURLToCrawlingAgentMessage(string url, DateTime? crawlingDateTime) : base(url, crawlingDateTime)
        {
        }
        public AddNewURLToCrawlingAgentMessage(string url) : base(url)
        {
        }
    }
}
