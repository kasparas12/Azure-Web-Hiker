using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using Azure.Web.Hiker.Core.AgentRegistrar.Models;
using Azure.Web.Hiker.Core.AgentRegistrar.Persistence;

using Dapper;

namespace Azure.Web.Hiker.Infrastructure.Persistence.Dapper
{
    public class DapperAgentRegistrarRepository : IAgentRegistrarRepository
    {
        private readonly string _connectionString;

        public DapperAgentRegistrarRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool AgentForSpecificHostExists(string hostname)
        {
            return !(GetAgentRegistrarByHostname(hostname) is null);
        }

        public void DeleteAgentEntry(string hostname)
        {
            var sql = "UPDATE dbo.agent_registrar SET is_deleted = 1 WHERE agent_host = @AgentHost";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute(sql, new { AgentHost = hostname });
            }
        }

        public IAgentRegistrarEntry GetAgentForSpecificHost(string hostname)
        {
            return GetAgentRegistrarByHostname(hostname);
        }

        public IEnumerable<(string, string)> GetHostsForWhichAgentsAreFree(DateTime timeoutDate)
        {
            var sql = "SELECT agent_host, agent_name FROM dbo.agent_registrar WHERE is_deleted = 0 and last_activity < @TimeoutDate";

            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.Query<(string, string)>(sql, new { TimeoutDate = timeoutDate });
            }
        }

        public int GetNextAgentCounterNumber()
        {
            var sql = "SELECT next value for AgentNameNumberCounter";

            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.QueryFirstOrDefault<int>(sql);
            }
        }

        public int GetNumberOfActiveAgents()
        {
            var sql = "SELECT COUNT(*) FROM dbo.agent_registrar WHERE is_deleted = 0";

            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.QueryFirstOrDefault<int>(sql);
            }
        }

        public void InsertNewAgent(IAgentRegistrarEntry newEntry)
        {
            var sql = "INSERT INTO dbo.agent_registrar(agent_name,agent_host,is_deleted,created_at) VALUES (@Name, @Host, 0, @CreatedAt)";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute(sql, new { Name = newEntry.AgentName, Host = newEntry.AgentHost, CreatedAt = DateTime.Now });
            }
        }

        public void UpdateAgentActivityTime(string hostName, DateTime lastActivity)
        {
            var sql = "UPDATE dbo.agent_registrar SET last_activity = @LastActivity WHERE agent_host = @AgentHost";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute(sql, new { LastActivity = lastActivity, AgentHost = hostName });
            }
        }

        private AgentRegistrarEntry GetAgentRegistrarByHostname(string hostname)
        {
            var sql = "SELECT id Id, agent_name AgentName, agent_host AgentHost, is_deleted IsDeleted, created_at CreatedAt, deleted_at DeletedAt FROM dbo.agent_registrar WHERE is_deleted = 0 AND agent_host = @AgentHost";

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    var a = connection.QueryFirstOrDefault<AgentRegistrarEntry>(sql, new { AgentHost = hostname });
                    return a;
                }
                catch (Exception e)
                {
                    var a = e;
                    return null;
                }
            }
        }
    }
}
