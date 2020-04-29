
using Azure.Web.Hiker.Core.Models.Interfaces;

namespace Azure.Web.Hiker.Core.Persistence.Interfaces
{
    public interface IAgentRegistrarRepository
    {
        bool AgentForSpecificHostExists(string hostname);
        IAgentRegistrarEntry GetAgentForSpecificHost(string hostname);
        void InsertNewAgent(IAgentRegistrarEntry newEntry);
        void DeleteAgent(int agentId);
        int GetNextAgentCounterNumber();
    }
}
