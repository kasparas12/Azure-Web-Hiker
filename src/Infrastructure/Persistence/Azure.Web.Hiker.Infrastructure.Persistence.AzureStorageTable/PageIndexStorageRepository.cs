using System;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.Common.Extensions;
using Azure.Web.Hiker.Core.IndexStorage.Interfaces;
using Azure.Web.Hiker.Infrastructure.Persistence.AzureStorageTable.Models;

using Microsoft.Azure.Cosmos.Table;

namespace Azure.Web.Hiker.Infrastructure.Persistence.AzureStorageTable
{
    public class PageIndexStorageRepository : IPageIndexStorageRepository<PageIndex>
    {
        private readonly CloudTable _cloudTable;

        public PageIndexStorageRepository(CloudTable cloudTable)
        {
            _cloudTable = cloudTable;
        }

        public async Task<PageIndex> GetPageIndexByUrl(string url)
        {
            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<PageIndex>(url.GetHostOfUrl(), url.CalculateMD5HashOfUrl());
                TableResult result = await _cloudTable.ExecuteAsync(retrieveOperation);
                PageIndex index = result.Result as PageIndex;

                return index;
            }
            catch (StorageException)
            {
                throw;
            }
        }

        public async Task InsertNewPageIndex(PageIndex entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity cannot be null");
            }

            entity.PartitionKey = entity.PageUrl.GetHostOfUrl();
            entity.RowKey = entity.PageUrl.CalculateMD5HashOfUrl();

            try
            {
                // Create the InsertOrReplace table operation
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

                // Execute the operation.
                TableResult result = await _cloudTable.ExecuteAsync(insertOrMergeOperation);

            }
            catch (StorageException)
            {
                throw;
            }
        }

        public Task<bool> IsPageIndexAlreadyVisited(string url)
        {
            throw new NotImplementedException();
        }
    }
}
