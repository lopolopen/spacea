using System.Collections.Generic;
using System.Security.Claims;
using SpaceA.Model.Entities;

namespace SpaceA.WebApi.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(params Claim[] claims);
        string GenerateAccessToken(Member member);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        Member GetMember(ClaimsPrincipal user);
    }
}