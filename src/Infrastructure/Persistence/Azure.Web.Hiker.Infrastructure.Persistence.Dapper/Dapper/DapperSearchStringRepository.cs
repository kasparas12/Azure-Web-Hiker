using System.Collections.Generic;
using System.Data.SqlClient;

using Azure.Web.Hiker.Core.CrawlingAgent.RenderingDecisionMaker;

using Dapper;

namespace Azure.Web.Hiker.Infrastructure.Persistence.Dapper.Dapper
{
    public class DapperSearchStringRepository : ISearchStringRepository
    {
        protected string TableName => "dbo.search_strings";
        private readonly string _connectionString;
        public DapperSearchStringRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<SearchString> GetAllSearchStrings()
        {
            var sql = $"SELECT id Id, search_string SearchStringValue, framework Framework FROM {TableName}";

            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.Query<SearchString>(sql);
            }
        }
    }
}
