using System;

using Azure.Web.Hiker.Core.Common.Enum;

namespace Azure.Web.Hiker.Core.Common.Metrics
{
    public interface IHttpVisitMetricTracker
    {
        void TrackPageVisit(Uri url, DateTime visitedAt);
        void TrackVisitDisallowed(Uri url, string reason);
        void TrackVisitTimeout(Uri url);
        void TrackFrameworkDiscovered(Uri url, JavascriptFrameworks framework);
    }
}
