using System;
using System.Text;

namespace Azure.Web.Hiker.Core.Common.Extensions
{
    public static class StringExtensions
    {
        public static string GetHostOfUrl(this string url)
        {
            var uri = new Uri(url);
            return uri.Host;
        }

        public static string CalculateMD5HashOfUrl(this string url)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(url);
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
}
