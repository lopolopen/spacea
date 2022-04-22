using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SpaceA.Common;

namespace SpaceA.WebApi.Security
{
    public static class PasswordUtil
    {
        private const string CHARS = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz#@$^*%";

        public static string GenSalt()
        {
            var r = new Random();
            return r.NextString(CHARS, (4, 6), 1).First();
        }

        public static string HashPassword(string password, string salt)
        { 
            var md5 =  MD5.Create();
            var bytes = Encoding.UTF8.GetBytes($"{password}.{salt}");
            bytes =  md5.ComputeHash(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
