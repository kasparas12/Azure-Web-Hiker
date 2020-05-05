using System;
using System.Threading.Tasks;

using Microsoft.Azure.Cosmos.Table;

namespace Azure.Web.Hiker.Infrastructure.Persistence.AzureStorageTable.Config
{
    public static class PageIndexCloudTable
    {
        private const string TableName = "CrawlerIndexTable";

        public static async Task<CloudTable> SetupPageIndexCloudTable(string connectionString)
        {
            CloudStorageAccount storageAccount;

            try
            {
                storageAccount = CloudStorageAccount.Parse(connectionString);
            }

            catch (FormatException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }

            var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            // Create a table client for interacting with the table service 
            CloudTable table = tableClient.GetTableReference(TableName);

            await table.CreateIfNotExistsAsync();

            return table;
        }
    }
}
