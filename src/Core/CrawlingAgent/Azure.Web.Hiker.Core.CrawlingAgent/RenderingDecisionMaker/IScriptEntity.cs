namespace Azure.Web.Hiker.Core.CrawlingAgent.RenderingDecisionMaker
{
    public interface IScriptEntity
    {
        public string Host { get; set; }
        public string ScriptFile { get; set; }
        public string Checksum { get; set; }
        public bool? RenderDecision { get; set; }
    }
}
