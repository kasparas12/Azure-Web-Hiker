using System;

namespace Azure.Web.Hiker.Core.Common.Metrics
{
    public interface IHttpVisitMetricTracker
    {
        void TrackPageVisit(Uri url, DateTime visitedAt);
        void TrackVisitDisallowed(Uri url, string reason);
    }
}
