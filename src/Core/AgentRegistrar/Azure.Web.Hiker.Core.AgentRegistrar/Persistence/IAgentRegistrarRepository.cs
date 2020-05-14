using System;
using System.Collections.Generic;

using Azure.Web.Hiker.Core.AgentRegistrar.Models;

namespace Azure.Web.Hiker.Core.AgentRegistrar.Persistence
{
    public interface IAgentRegistrarRepository
    {
        bool AgentForSpecificHostExists(string hostname);
        IAgentRegistrarEntry GetAgentForSpecificHost(string hostname);
        void InsertNewAgent(IAgentRegistrarEntry newEntry);
        void DeleteAgentEntry(string hostname);
        int GetNextAgentCounterNumber();
        int GetNumberOfActiveAgents();
        IEnumerable<(string, string)> GetHostsForWhichAgentsAreFree(DateTime timeoutDate);
        void UpdateAgentActivityTime(string hostName, DateTime lastActivity);
        double? GetPrecalculatedCrawlDelay(string hostName);
        void InsertCalculatedCrawlDelay(string hostName, double crawlDelay);
    }
}
