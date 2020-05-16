using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

using Azure.Web.Hiker.Core.AgentRegistrar.Models;

using Dapper;

namespace Azure.Web.Hiker.Infrastructure.Persistence.Dapper.Dapper
{
    public abstract class AgentRegistrarRepositoryBase
    {
        private readonly string _connectionString;
        protected virtual string TableName => null;
        protected virtual string AgentSequenceName => null;

        public AgentRegistrarRepositoryBase(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool AgentForSpecificHostExists(string tableName, string hostname)
        {
            return !(GetAgentRegistrarByHostname(tableName, hostname) is null);
        }

        public void DeleteAgentEntry(string tableName, string hostname)
        {
            var sql = $"UPDATE {tableName} SET is_deleted = 1 WHERE agent_host = @AgentHost";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute(sql, new { AgentHost = hostname });
            }
        }

        public IAgentRegistrarEntry GetAgentForSpecificHost(string tableName, string hostname)
        {
            return GetAgentRegistrarByHostname(tableName, hostname);
        }

        public IEnumerable<(string, string)> GetHostsForWhichAgentsAreFree(string tableName, DateTime timeoutDate)
        {
            var sql = $"SELECT agent_host, agent_name FROM {tableName} WHERE is_deleted = 0 and (last_activity < @TimeoutDate or last_activity IS NULL)";

            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.Query<(string, string)>(sql, new { TimeoutDate = timeoutDate });
            }
        }

        public int GetNextAgentCounterNumber(string sequenceName)
        {
            var sql = $"SELECT next value for {sequenceName}";

            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.QueryFirstOrDefault<int>(sql);
            }
        }

        public int GetNumberOfActiveAgents(string tableName)
        {
            var sql = $"SELECT COUNT(*) FROM {tableName} WHERE is_deleted = 0";

            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.QueryFirstOrDefault<int>(sql);
            }
        }

        public double? GetPrecalculatedCrawlDelay(string tableName, string hostName)
        {
            var sql = $"SELECT crawl_delay FROM {tableName} WHERE agent_host = @AgentHost AND is_deleted = 0";

            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.Query<double?>(sql, new { AgentHost = hostName }).Single();
            }
        }

        public void InsertCalculatedCrawlDelay(string tableName, string hostName, double crawlDelay)
        {
            var sql = $"UPDATE {tableName} SET crawl_delay = @CrawlDelay WHERE agent_host = @AgentHost AND is_deleted = 0";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute(sql, new { CrawlDelay = crawlDelay, AgentHost = hostName });
            }
        }

        public void InsertNewAgent(string tableName, IAgentRegistrarEntry newEntry)
        {
            var sql = $"INSERT INTO {tableName}(agent_name,agent_host,is_deleted,created_at) VALUES (@Name, @Host, 0, @CreatedAt)";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute(sql, new { Name = newEntry.AgentName, Host = newEntry.AgentHost, CreatedAt = DateTime.Now });
            }
        }

        public void UpdateAgentActivityTime(string tableName, string hostName, DateTime lastActivity)
        {
            var sql = $"UPDATE {tableName} SET last_activity = @LastActivity WHERE agent_host = @AgentHost";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute(sql, new { LastActivity = lastActivity, AgentHost = hostName });
            }
        }

        private AgentRegistrarEntry GetAgentRegistrarByHostname(string tableName, string hostname)
        {
            var sql = $"SELECT id Id, agent_name AgentName, agent_host AgentHost, is_deleted IsDeleted, created_at CreatedAt, deleted_at DeletedAt FROM {tableName} WHERE is_deleted = 0 AND agent_host = @AgentHost";

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
