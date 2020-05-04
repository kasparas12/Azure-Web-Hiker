using System.Threading.Tasks;

namespace Azure.Web.Hiker.Core.CrawlingEngine.Services
{
    public interface IAgentController
    {
        Task SpawnNewAgentForHostnameAsync(string hostname, string serviceName);
    }
}
