using System.Threading.Tasks;

namespace Azure.Web.Hiker.Core.Services.AgentController
{
    public interface IAgentController
    {
        Task SpawnNewAgentForHostnameAsync(string hostname, string serviceName);
    }
}
