using System.Collections.Generic;

using Azure.Web.Hiker.Core.Common.Enum;

namespace Azure.Web.Hiker.Core.CrawlingAgent.RenderingDecisionMaker
{
    public class SearchString
    {
        public SearchString()
        {

        }

        public SearchString(string searchString, JavascriptFrameworks framework)
        {
            SearchStringValue = searchString;
            Framework = framework;
        }

        public int Id { get; set; }
        public string SearchStringValue { get; set; }
        public JavascriptFrameworks Framework { get; set; }
    }

    public interface ISearchStringRepository
    {
        IEnumerable<SearchString> GetAllSearchStrings();
    }
}
