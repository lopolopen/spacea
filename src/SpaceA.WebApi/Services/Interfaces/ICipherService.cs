using System.Collections.Generic;
using System.Security.Claims;
using SpaceA.Model.Entities;

namespace SpaceA.WebApi.Services.Interfaces
{
    public interface ICipherService
    {
        string Encrypt(string plainText);

        string Decrypt(string cipherText);
    }
}