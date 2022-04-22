
using SpaceA.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;

namespace SpaceA.WebApi.Services
{
    public class CipherService : ICipherService
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly string _key;

        public CipherService(IDataProtectionProvider dataProtectionProvider, IConfiguration configuration)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _key = configuration["Cipher:Secret"];
        }

        public string Encrypt(string plainText)
        {
            var protector = _dataProtectionProvider.CreateProtector(_key);
            return protector.Protect(plainText);
        }

        public string Decrypt(string cipherText)
        {
            var protector = _dataProtectionProvider.CreateProtector(_key);
            return protector.Unprotect(cipherText);
        }
    }
}