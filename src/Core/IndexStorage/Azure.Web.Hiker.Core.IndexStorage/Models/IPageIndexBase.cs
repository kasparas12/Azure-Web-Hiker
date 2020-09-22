using System;
using System.Text;

using Azure.Web.Hiker.Core.RenderingAgent;

namespace Azure.Web.Hiker.Core.IndexStorage.Models
{
    public interface IPageIndex
    {
        public string PageUrl { get; set; }
        public int HitCount { get; set; }
        public bool Visited { get; set; }
        public DateTime? VisitedTimestamp { get; set; }
        public int? StatusCode { get; set; }
        public string DisallowedCrawlReason { get; set; }
        public RenderStatus? RenderStatus { get; set; }

        public string CreateMD5HashOfUrl()
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(PageUrl);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }

    public class PageIndex : IPageIndex
    {
        public PageIndex(string url, int hitcount, bool visited, DateTime? visitedAt = null, int? statusCode = null, string disallowedReason = null, RenderStatus? status = null)
        {
            PageUrl = url;
            HitCount = hitcount;
            Visited = visited;
            VisitedTimestamp = visitedAt;
            StatusCode = statusCode;
            DisallowedCrawlReason = disallowedReason;
            RenderStatus = status;
        }

        public string PageUrl { get; set; }
        public int HitCount { get; set; }
        public bool Visited { get; set; }
        public DateTime? VisitedTimestamp { get; set; }
        public int? StatusCode { get; set; }
        public string DisallowedCrawlReason { get; set; }
        public RenderStatus? RenderStatus { get; set; }

    }
}
