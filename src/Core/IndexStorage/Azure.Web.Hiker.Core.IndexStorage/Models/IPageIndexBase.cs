using System;
using System.Text;

namespace Azure.Web.Hiker.Core.IndexStorage.Models
{
    public interface IPageIndex
    {
        public string PageUrl { get; set; }
        public int HitCount { get; set; }
        public bool Visited { get; set; }
        public DateTime? VisitedTimestamp { get; set; }

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
        public PageIndex(string url, int hitcount, bool visited, DateTime? visitedAt = null)
        {
            PageUrl = url;
            HitCount = hitcount;
            Visited = visited;
            VisitedTimestamp = visitedAt;
        }

        public string PageUrl { get; set; }
        public int HitCount { get; set; }
        public bool Visited { get; set; }
        public DateTime? VisitedTimestamp { get; set; }
    }
}
