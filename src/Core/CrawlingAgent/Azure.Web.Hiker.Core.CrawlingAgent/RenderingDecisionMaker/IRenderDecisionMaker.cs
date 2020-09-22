using System;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.CrawlingAgent.Extensions;

namespace Azure.Web.Hiker.Core.CrawlingAgent.RenderingDecisionMaker
{
    public interface IRenderDecisionMaker
    {
        Task<bool> ShouldWebPageBeRenderedWithBrowser(Uri webPage, string HTMLContent);
    }

    public class RenderDecisionMaker : IRenderDecisionMaker
    {
        private readonly IHtmlScriptsParser _htmlParser;
        private readonly IStringSearcher _stringSearcher;
        private readonly IScriptRepository _scriptRepository;
        private readonly ICheckSumCalculator _checkSumCalculator;

        public RenderDecisionMaker(IHtmlScriptsParser htmlParser, IStringSearcher stringSearcher, IScriptRepository scriptRepository, ICheckSumCalculator checkSumCalculator)
        {
            _htmlParser = htmlParser;
            _stringSearcher = stringSearcher;
            _scriptRepository = scriptRepository;
            _checkSumCalculator = checkSumCalculator;
        }

        public async Task<bool> ShouldWebPageBeRenderedWithBrowser(Uri webPage, string HTMLContent)
        {
            var scriptLinks = await _htmlParser.ParseAllScriptLinksAsync($"{webPage.Scheme}://{webPage.Host.Replace("www.", "")}", HTMLContent);

            foreach (var scriptLink in scriptLinks)
            {
                string scriptFileChecksum;
                try
                {
                    scriptFileChecksum = _checkSumCalculator.CalulateChecksum(scriptLink);
                }
                catch (Exception)
                {
                    return false;
                }

                var isCheckSumSame = await _scriptRepository.IsCheckSumSameAsync(webPage.Host, scriptLink.ExtractScriptName(), scriptFileChecksum);

                if (isCheckSumSame.Item1 && isCheckSumSame.Item2.HasValue && isCheckSumSame.Item2.Value)
                {
                    return true;
                }

                if (isCheckSumSame.Item1 && isCheckSumSame.Item2.HasValue && !isCheckSumSame.Item2.Value)
                {
                    continue;
                }

                bool matchFound;

                try
                {
                    matchFound = _stringSearcher.StringsMatchFound(scriptLink);
                }
                catch (Exception)
                {
                    return false;
                }

                await _scriptRepository.InsertOrUpdateNewRenderStatusAsync(webPage.Host.Replace("www.", ""), scriptLink.ExtractScriptName(), scriptFileChecksum, matchFound);

                if (matchFound)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
