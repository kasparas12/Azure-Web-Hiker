namespace Azure.Web.Hiker.Core.CrawlingAgent.Models
{
    public interface ICrawlingAgentHost
    {
        public string AssignedHostName { get; set; }
    }

    public class CrawlerAgentInitializationData : ICrawlingAgentHost
    {
        public CrawlerAgentInitializationData()
        {

        }

        public CrawlerAgentInitializationData(string host)
        {
            AssignedHostName = host;
        }
        public string AssignedHostName { get; set; }
    }
}
