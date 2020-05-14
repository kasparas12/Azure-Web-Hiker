using System;
using System.Collections.Generic;

using Azure.Web.Hiker.Core.AgentRegistrar.Models;
using Azure.Web.Hiker.Core.AgentRegistrar.Persistence;
using Azure.Web.Hiker.Infrastructure.Persistence.Dapper.Dapper;

namespace Azure.Web.Hiker.Infrastructure.Persistence.Dapper
{
    public class DapperAgentRegistrarRepository : AgentRegistrarRepositoryBase, IRenderingAgentRepository
    {
        protected override string TableName => "dbo.agent_registrar";
        protected override string AgentSequenceName => "AgentNameNumberCounter";

        public DapperAgentRegistrarRepository(string connectionString) : base(connectionString)
        {

        }

        public bool AgentForSpecificHostExists(string hostname)
        {
            return AgentForSpecificHostExists(TableName, hostname);
        }

        public void DeleteAgentEntry(string hostname)
        {
            DeleteAgentEntry(TableName, hostname);
        }

        public IAgentRegistrarEntry GetAgentForSpecificHost(string hostname)
        {
            return GetAgentForSpecificHost(TableName, hostname);
        }

        public IEnumerable<(string, string)> GetHostsForWhichAgentsAreFree(DateTime timeoutDate)
        {
            return GetHostsForWhichAgentsAreFree(TableName, timeoutDate);
        }

        public int GetNextAgentCounterNumber()
        {
            return GetNextAgentCounterNumber(AgentSequenceName);
        }

        public int GetNumberOfActiveAgents()
        {
            return GetNumberOfActiveAgents(TableName);
        }

        public double? GetPrecalculatedCrawlDelay(string hostName)
        {
            return GetPrecalculatedCrawlDelay(TableName, hostName);
        }

        public void InsertCalculatedCrawlDelay(string hostName, double crawlDelay)
        {
            InsertCalculatedCrawlDelay(TableName, hostName, crawlDelay);
        }

        public void InsertNewAgent(IAgentRegistrarEntry newEntry)
        {
            InsertNewAgent(TableName, newEntry);
        }

        public void UpdateAgentActivityTime(string hostName, DateTime lastActivity)
        {
            UpdateAgentActivityTime(TableName, hostName, lastActivity);
        }
    }
}
