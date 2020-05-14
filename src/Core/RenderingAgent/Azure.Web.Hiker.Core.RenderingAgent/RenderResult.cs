using System;
using System.Collections.Generic;

namespace Azure.Web.Hiker.Core.RenderingAgent
{
    public class RenderResult
    {
        public RenderResult(RenderStatus status)
        {
            Status = status;
        }

        public RenderResult(RenderStatus status, IEnumerable<Uri> discoveredUrls)
        {
            Status = status;
            NewDiscoveredLinks = discoveredUrls;
        }

        public RenderStatus Status { get; }
        public IEnumerable<Uri> NewDiscoveredLinks { get; }
    }

    public enum RenderStatus
    {
        Undefined = 0,
        Ok = 1,
        Timeouted = 2,
        OtherFailure = 3
    }
}
