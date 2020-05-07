using System;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.Common.Extensions;
using Azure.Web.Hiker.Core.IndexStorage.Interfaces;
using Azure.Web.Hiker.Core.IndexStorage.Models;

using Microsoft.Azure.Cosmos.Table;

using PageIndex = Azure.Web.Hiker.Infrastructure.Persistence.AzureStorageTable.Models.PageIndex;

namespace Azure.Web.Hiker.Infrastructure.Persistence.AzureStorageTable
{
    public class PageIndexStorageRepository : IPageIndexStorageRepository
    {
        private readonly CloudTable _cloudTable;

        public PageIndexStorageRepository(CloudTable cloudTable)
        {
            _cloudTable = cloudTable;
        }

        public async Task<IPageIndex> GetPageIndexByUrl(string url)
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

        public async Task InsertOrMergeNewPageIndex(IPageIndex entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity cannot be null");
            }

            var index = new PageIndex(entity.PageUrl, entity.HitCount, entity.Visited, entity.VisitedTimestamp);
            index.PartitionKey = entity.PageUrl.GetHostOfUrl();
            index.RowKey = entity.PageUrl.CalculateMD5HashOfUrl();

            try
            {
                // Create the InsertOrReplace table operation
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(index);

                // Execute the operation.
                TableResult result = await _cloudTable.ExecuteAsync(insertOrMergeOperation);

            }
            catch (StorageException)
            {
                throw;
            }
        }
    }
}
