using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Security
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public static class TokenGenerator
    {
        private const string SecretKey = "NegarArya9598";

        public static string GenerateToken(string tokenName)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var rawData = $"{tokenName}:{timestamp}";

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return $"{Convert.ToBase64String(hash)}:{timestamp}";
            }
        }

        public static bool IsValidToken(string token, string publicKey)
        {
            var parts = token.Split(':');
            if (parts.Length != 2) return false;

            var providedHash = parts[0];
            var timestamp = long.Parse(parts[1]);

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (now - timestamp > 300) return false;

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey)))
            {
                var rawData = $"{publicKey}:{timestamp}";
                var expectedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData)));

                return expectedHash == providedHash;
            }
        }
    }
}
