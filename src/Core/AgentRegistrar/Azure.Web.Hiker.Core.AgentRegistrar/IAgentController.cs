using System.Threading.Tasks;

namespace Azure.Web.Hiker.Core.AgentRegistrar
{
    public interface IAgentController
    {
        Task SpawnNewAgentForHostnameAsync(string serviceTypeName, string hostname, string serviceName);
        Task DeleteAgentForHostnameAsync(string serviceName);
    }
}
