using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

using Azure.Web.Hiker.Core.Common.Metrics;

using Ganss.Text;

namespace Azure.Web.Hiker.Core.CrawlingAgent.RenderingDecisionMaker
{
    public interface IStringSearcher
    {
        bool StringsMatchFound(Uri webPage);
    }

    public class StringSearcher : IStringSearcher
    {
        private readonly IHttpVisitMetricTracker _httpVisitMetricTracker;
        private readonly IEnumerable<SearchString> _searchStrings;
        private readonly AhoCorasick _matcher;

        public StringSearcher(ISearchStringRepository repository, IHttpVisitMetricTracker httpVisitMetricTracker)
        {
            _searchStrings = repository.GetAllSearchStrings();
            _matcher = new AhoCorasick(_searchStrings.Select(x => x.SearchStringValue));
            _httpVisitMetricTracker = httpVisitMetricTracker;
        }

        public bool StringsMatchFound(Uri webPage)
        {
            using (var cl = new WebClient())
            using (var str = cl.OpenRead(webPage))
            using (var reader = new StreamReader(str))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var matches = _matcher.Search(line);

                    if (matches.Count() > 1)
                    {
                        var searchString = _searchStrings.Where(x => x.SearchStringValue.Contains(matches.First().Word)).FirstOrDefault().Framework;
                        _httpVisitMetricTracker.TrackFrameworkDiscovered(webPage, searchString);

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
