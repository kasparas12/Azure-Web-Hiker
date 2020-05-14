using System;
using System.Data.SqlClient;

using Azure.Web.Hiker.Core.AgentRegistrar.Models;
using Azure.Web.Hiker.Core.AgentRegistrar.Persistence;

using Dapper;

namespace Azure.Web.Hiker.Infrastructure.Persistence.Dapper.Dapper
{
    public class DapperSettingsRepository : ISettingsRepository
    {
        private readonly string _connectionString;

        public DapperSettingsRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Setting GetSettingByName(string settingName)
        {
            var sql = "SELECT id Id, setting_name Name, setting_value Value FROM dbo.Settings WHERE setting_name = @SettingName";

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    var result = connection.QueryFirstOrDefault<Setting>(sql, new { SettingName = settingName });
                    return result;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}
