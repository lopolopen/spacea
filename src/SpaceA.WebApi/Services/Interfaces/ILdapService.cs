using System.Collections.Generic;
using System.Security.Claims;
using SpaceA.Model.Entities;

namespace SpaceA.WebApi.Services.Interfaces
{
    public interface ILdapService
    {
         bool Verify(string accountName, string password);

         Member GetMember(string accountName);
    }
}