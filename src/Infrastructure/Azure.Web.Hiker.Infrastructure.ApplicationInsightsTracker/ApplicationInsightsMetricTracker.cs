using System;
using System.Collections.Generic;

using Azure.Web.Hiker.Core.Common.Metrics;

using Microsoft.ApplicationInsights;

namespace Azure.Web.Hiker.Infrastructure.ApplicationInsightsTracker
{
    public class ApplicationInsightsMetricTracker : IHttpVisitMetricTracker
    {
        private readonly TelemetryClient _telemetryClient;

        public ApplicationInsightsMetricTracker(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        public void TrackPageVisit(Uri url, DateTime visitedTime)
        {
            _telemetryClient.TrackEvent("Website visited", new Dictionary<string, string> { { "Hostname", url.Host }, { "VisitedAt", visitedTime.ToString() } });
        }

        public void TrackVisitDisallowed(Uri url, string reason)
        {
            _telemetryClient.TrackEvent("Disallowed website visit", new Dictionary<string, string> { { "Link", url.AbsoluteUri }, { "Disallowed reason", reason } });
        }

        public void TrackVisitTimeout(Uri url)
        {
            _telemetryClient.TrackEvent("Timeout for website", new Dictionary<string, string> { { "Link", url.AbsoluteUri } });
        }
    }
}
