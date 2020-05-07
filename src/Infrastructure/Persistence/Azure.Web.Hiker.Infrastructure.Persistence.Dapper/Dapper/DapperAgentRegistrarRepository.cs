using System;
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

        public void DeleteAgent(int agentId)
        {
            var sql = "UPADATE dbo.agent_registrar SET is_deleted = 1 WHERE Id = @Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute(sql, new { Id = agentId });
            }
        }

        public IAgentRegistrarEntry GetAgentForSpecificHost(string hostname)
        {
            return GetAgentRegistrarByHostname(hostname);
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

        private AgentRegistrarEntry GetAgentRegistrarByHostname(string hostname)
        {
            var sql = "SELECT id Id, agent_name AgentName, agent_host AgentHost, is_deleted IsDeleted, created_at CreatedAt, deleted_at DeletedAt FROM dbo.agent_registrar WHERE agent_host = @AgentHost";

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
