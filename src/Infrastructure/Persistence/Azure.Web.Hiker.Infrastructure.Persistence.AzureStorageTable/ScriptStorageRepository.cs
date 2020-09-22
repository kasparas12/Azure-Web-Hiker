using System.Threading.Tasks;

using Azure.Web.Hiker.Core.Common.Extensions;
using Azure.Web.Hiker.Core.CrawlingAgent.RenderingDecisionMaker;
using Azure.Web.Hiker.Infrastructure.Persistence.AzureStorageTable.Models;

using Microsoft.Azure.Cosmos.Table;

namespace Azure.Web.Hiker.Infrastructure.Persistence.AzureStorageTable
{
    public class ScriptStorageRepository : IScriptRepository
    {
        private readonly CloudTable _cloudTable;

        public ScriptStorageRepository(CloudTable cloudTable)
        {
            _cloudTable = cloudTable;
        }

        public async Task InsertOrUpdateNewRenderStatusAsync(string hostName, string scriptName, string checkSum, bool renderStatus)
        {
            var script = new ScriptEntity(hostName, scriptName.CalculateMD5HashOfUrl(), checkSum, renderStatus);
            script.PartitionKey = hostName;
            script.RowKey = scriptName.CalculateMD5HashOfUrl();

            try
            {
                // Create the InsertOrReplace table operation
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(script);

                // Execute the operation.
                TableResult result = await _cloudTable.ExecuteAsync(insertOrMergeOperation);

            }
            catch (StorageException)
            {
                throw;
            }
        }

        public async Task<(bool, bool?)> IsCheckSumSameAsync(string hostName, string scriptName, string checkSum)
        {
            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<ScriptEntity>(hostName, scriptName.CalculateMD5HashOfUrl());
                TableResult result = await _cloudTable.ExecuteAsync(retrieveOperation);

                ScriptEntity script = result.Result as ScriptEntity;

                if (script is null)
                {
                    return (false, null);
                }

                var isChecksumSame = checkSum == script.Checksum;

                if (isChecksumSame)
                {
                    return (true, script.RenderDecision);
                }

                return (false, null);
            }
            catch (StorageException)
            {
                throw;
            }
        }
    }
}