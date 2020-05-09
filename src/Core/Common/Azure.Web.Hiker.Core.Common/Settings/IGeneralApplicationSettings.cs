namespace Azure.Web.Hiker.Core.Common.Settings
{
    public interface IGeneralApplicationSettings
    {
        public int MaxNumberOfAgents { get; set; }
        public int AgentInactivityTimeoutValue { get; set; }
    }
}
