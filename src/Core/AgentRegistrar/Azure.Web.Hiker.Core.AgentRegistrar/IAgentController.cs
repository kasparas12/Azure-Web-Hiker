using System.Threading.Tasks;

namespace Azure.Web.Hiker.Core.AgentRegistrar
{
    public interface IAgentController
    {
        Task SpawnNewAgentForHostnameAsync(string hostname, string serviceName);
    }
}
