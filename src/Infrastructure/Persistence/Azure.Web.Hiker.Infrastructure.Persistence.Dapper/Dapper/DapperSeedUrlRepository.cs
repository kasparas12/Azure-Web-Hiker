using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.CrawlingEngine.Interfaces;
using Azure.Web.Hiker.Core.CrawlingEngine.Models;

using Dapper;

namespace Azure.Web.Hiker.Infrastructure.Persistence.Dapper.Dapper
{
    public class DapperSeedUrlRepository : ISeedUrlRepository
    {
        private readonly string _connectionString;

        public DapperSeedUrlRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<SeedUrlAddress>> GetListOfSeedUrls()
        {
            var sql = "SELECT id Id, url UrlAddress FROM dbo.seed_urls";

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    var a = await connection.QueryAsync<SeedUrlAddress>(sql);
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
