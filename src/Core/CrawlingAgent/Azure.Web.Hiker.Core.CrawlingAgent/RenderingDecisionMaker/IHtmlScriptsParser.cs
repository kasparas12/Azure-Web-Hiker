using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AngleSharp;
using AngleSharp.Html.Parser;

namespace Azure.Web.Hiker.Core.CrawlingAgent.RenderingDecisionMaker
{
    public interface IHtmlScriptsParser
    {
        Task<IEnumerable<Uri>> ParseAllScriptLinksAsync(string hostName, string HTMLContent);
    }

    public class AngleSharpHtmlParser : IHtmlScriptsParser
    {
        public async Task<IEnumerable<Uri>> ParseAllScriptLinksAsync(string hostName, string HTMLContent)
        {
            //Use the default configuration for AngleSharp
            var config = Configuration.Default;

            //Create a new context for evaluating webpages with the given config
            var context = BrowsingContext.New(config);

            var parser = context.GetService<IHtmlParser>();

            var document = await parser.ParseDocumentAsync(HTMLContent);

            var allNodes = document.QuerySelectorAll("script[src]");

            List<Uri> parsedUrls = new List<Uri>();

            foreach (var node in allNodes)
            {
                var linkString = node.Attributes["src"].Value;

                if (linkString.StartsWith('/'))
                {
                    linkString = $"{hostName}{linkString}";
                }

                if (!linkString.Contains("https") && !linkString.Contains("http"))
                {
                    linkString = $"{hostName}/{linkString}";
                }
                parsedUrls.Add(new Uri(linkString));
            }

            return parsedUrls;
        }
    }
}
