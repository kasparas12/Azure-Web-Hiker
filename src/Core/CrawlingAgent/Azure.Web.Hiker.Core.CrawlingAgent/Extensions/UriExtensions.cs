using System;
using System.Text.RegularExpressions;

namespace Azure.Web.Hiker.Core.CrawlingAgent.Extensions
{
    public static class UriExtensions
    {
        /// <summary>
        /// File name from path regex
        /// </summary>
        public static Regex regex = new Regex(
          @"[^\\]*$",
          RegexOptions.IgnoreCase
        | RegexOptions.CultureInvariant
        | RegexOptions.IgnorePatternWhitespace
        | RegexOptions.Compiled
        );

        public static string ExtractScriptName(this Uri webPageLink)
        {
            var pageString = webPageLink.AbsoluteUri;

            var fileNameMatch = regex.Match(pageString);

            if (fileNameMatch.Success)
            {
                return fileNameMatch.Value;
            }

            return string.Empty;
        }
    }
}
