using SpaceA.Repository.Interfaces;
using SpaceA.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SpaceA.WebApi.Security
{
    public class ApikeyAuthenticationHandler : AuthenticationHandler<ApikeyOptions>
    {
        private IMemberRepository _memberRepository;

        public ApikeyAuthenticationHandler(
            IOptionsMonitor<ApikeyOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IMemberRepository memberRepository)
            : base(options, logger, encoder, clock)
        {
            _memberRepository = memberRepository;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authHeader = Context.Request.Headers["Authorization"].FirstOrDefault();
            var token = authHeader.Substring(ApikeyDefaults.AuthenticationScheme.Length + 1);
            var pat = await _memberRepository.GetPersonalAccessTokenAsync(token);
            if (pat == null)
            {
                return AuthenticateResult.Fail("Bad api key.");
            }
            if (pat.ExpiredAt != null && pat.ExpiredAt < DateTime.Now)
            {
                return AuthenticateResult.Fail("Api key is expired.");
            }
            var member = pat.Owner;
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, member.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sid, member.AccountName),
                new Claim(JwtRegisteredClaimNames.GivenName, member.FirstName),
                new Claim(JwtRegisteredClaimNames.FamilyName, member.LastName),
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new GenericPrincipal(identity, null);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}
