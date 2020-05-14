using Azure.Web.Hiker.Core.AgentRegistrar.Models;

namespace Azure.Web.Hiker.Core.AgentRegistrar.Persistence
{
    public interface ISettingsRepository
    {
        Setting GetSettingByName(string settingName);
    }
}
