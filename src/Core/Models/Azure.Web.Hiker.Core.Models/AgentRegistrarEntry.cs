using Azure.Web.Hiker.Core.Models.Interfaces;

namespace Azure.Web.Hiker.Core.Models
{
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
