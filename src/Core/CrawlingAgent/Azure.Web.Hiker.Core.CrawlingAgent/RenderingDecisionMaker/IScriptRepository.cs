using System.Threading.Tasks;

namespace Azure.Web.Hiker.Core.CrawlingAgent.RenderingDecisionMaker
{
    public interface IScriptRepository
    {
        Task<(bool, bool?)> IsCheckSumSameAsync(string hostName, string scriptName, string checkSum);
        Task InsertOrUpdateNewRenderStatusAsync(string hostName, string scriptName, string checkSum, bool renderStatus);
    }
}
