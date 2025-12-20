using System.Security.Cryptography;
using System.Text;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Extensions
{
    public static class TextExtensions
    {
        #region Methods

        public static string CalculateHash(this string text)
        {
            // step 1, calculate MD5 hash from input
            var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(text);
            var hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            foreach (var t in hash)
                sb.Append(t.ToString("X2"));

            return sb.ToString();
        }

        #endregion
    }
}