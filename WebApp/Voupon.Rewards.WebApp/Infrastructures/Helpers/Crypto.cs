using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Voupon.Rewards.WebApp.Infrastructures.Helpers
{
    public static class Crypto
    {
        public static class SHA_256
        {
            public static string GenerateSHA256String(string inputString)
            {
                SHA256 SHA256 = SHA256Managed.Create();
                byte[] bytes = Encoding.UTF8.GetBytes(inputString);
                byte[] hash = SHA256.ComputeHash(bytes);
                return GetStringFromHash(hash);
            }

            private static string GetStringFromHash(byte[] hash)
            {
                StringBuilder result = new StringBuilder();

                for (int i = 0; i < hash.Length; i++)
                {
                    result.Append(hash[i].ToString("X2"));
                }
                return result.ToString().ToLower();
            }

        }
    }
}
