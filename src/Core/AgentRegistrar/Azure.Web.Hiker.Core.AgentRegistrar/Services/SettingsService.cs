using System;

using Azure.Web.Hiker.Core.AgentRegistrar.Persistence;

namespace Azure.Web.Hiker.Core.AgentRegistrar.Services
{
    public interface ISettingsService
    {
        T GetSettingValue<T>(string settingName);
    }
    public class SettingsService : ISettingsService
    {
        private readonly ISettingsRepository _settingsRepository;

        public SettingsService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public T GetSettingValue<T>(string settingName)
        {
            var setting = _settingsRepository.GetSettingByName(settingName);

            try
            {
                return (T)Convert.ChangeType(setting.Value, typeof(T));
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}
