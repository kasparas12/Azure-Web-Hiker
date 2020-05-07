using Azure.Web.Hiker.Core.Common;

namespace Azure.Web.Hiker.Core.AgentRegistrar.Models
{
    public interface IAgentRegistrarEntry
    {
        public string AgentName { get; set; }
        public string AgentHost { get; set; }
    }

    public class AgentRegistrarEntry : BaseEntity, IAgentRegistrarEntry
    {
        public AgentRegistrarEntry()
        {

        }

        public AgentRegistrarEntry(string name, string host)
        {
            AgentName = name;
            AgentHost = host;
        }

        public string AgentName { get; set; }
        public string AgentHost { get; set; }
    }
}
