using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SpaceA.Model.Entities;
using SpaceA.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace SpaceA.WebApi.Services
{
    public class TokenService : ITokenService
    {
        // private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly TokenConfig _tokenConfig;

        public TokenService(IConfiguration configuration, IWebHostEnvironment env)
        {
            // _configuration = configuration;
            _tokenConfig = configuration.GetSection("Token").Get<TokenConfig>();
            _environment = env;
        }

        public string GenerateAccessToken(Member member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            return GenerateAccessToken(
                new Claim(JwtRegisteredClaimNames.Sub, member.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sid, member.AccountName),
                new Claim(JwtRegisteredClaimNames.GivenName, member.FirstName ?? ""),
                new Claim(JwtRegisteredClaimNames.FamilyName, member.LastName ?? "")
            );
        }

        public string GenerateAccessToken(params Claim[] claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_tokenConfig.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = "Kede SpaceA",
                Audience = "Anyone",
                Subject = new ClaimsIdentity(claims),
                // NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddSeconds(_tokenConfig.TimeToLive),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenConfig.Secret)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public Member GetMember(ClaimsPrincipal user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            var sub = user.FindFirst(JwtRegisteredClaimNames.Sub);
            if (sub == null)
            {
                if (_environment.IsDevelopment())
                {
                    return new Member
                    {
                        Id = 1,
                        AccountName = "?",
                        FirstName = "?",
                        LastName = "?"
                    };
                }

            }
            if (!uint.TryParse(sub.Value, out var id))
            {
                throw new Exception("invalid claim: sub");
            }
            var accountName = user.FindFirst(JwtRegisteredClaimNames.Sid);
            var firstName = user.FindFirst(JwtRegisteredClaimNames.GivenName);
            var lastName = user.FindFirst(JwtRegisteredClaimNames.FamilyName);
            return new Member
            {
                Id = id,
                AccountName = accountName?.Value,
                FirstName = firstName?.Value,
                LastName = lastName?.Value
            };
        }
    }

    class TokenConfig
    {
        //seconds
        public int TimeToLive { get; set; }
        public string Secret { get; set; }
    }
}