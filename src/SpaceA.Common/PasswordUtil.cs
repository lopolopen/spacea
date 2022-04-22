using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SpaceA.Common
{
    public static class PasswordUtil
    {
        private const string CHARS = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz#@$^*%";

        public static string GenSalt()
        {
            var r = new Random();
            return r.NextString(CHARS, (4, 6), 1).First();
        }

        public static string SaltHashPasswordWithSHA1(string password, string salt)
        {

            var sha1 = SHA1.Create();
            var bytes = Encoding.UTF8.GetBytes($"{password}.{salt}");
            bytes = sha1.ComputeHash(bytes);
            return Convert.ToBase64String(bytes);
        }

        public static string HashPasswordWithMD5(string password)
        {
            var md5 = MD5.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            bytes = md5.ComputeHash(bytes);
            var sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
