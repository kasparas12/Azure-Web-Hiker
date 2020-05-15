using Azure.Web.Hiker.Core.CrawlingAgent.RenderingDecisionMaker;

using Microsoft.Azure.Cosmos.Table;

namespace Azure.Web.Hiker.Infrastructure.Persistence.AzureStorageTable.Models
{
    public class ScriptEntity : TableEntity, IScriptEntity
    {
        public ScriptEntity()
        {

        }

        public ScriptEntity(string host, string scriptFile, string checkSum, bool? renderDecision = null)
        {
            Host = host;
            ScriptFile = scriptFile;
            Checksum = checkSum;
            RenderDecision = renderDecision;
        }

        public string Host { get; set; }
        public string ScriptFile { get; set; }
        public string Checksum { get; set; }
        public bool? RenderDecision { get; set; }
    }
}
