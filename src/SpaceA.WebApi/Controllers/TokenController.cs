using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using SpaceA.Model.Mapper;
using SpaceA.Repository.Interfaces;
using SpaceA.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using DTO = SpaceA.Model.Dto;
using SpaceA.Model.Entities;
using SpaceA.Repository.Context;

namespace SpaceA.WebApi.Controllers
{
    [Route("api/[action]_[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private IWebHostEnvironment Environment { get; }
        private readonly SpaceAContext _context;
        private readonly ITokenService _tokenService;
        private readonly IMemberRepository _memberRepository;
        private readonly Lazy<Member> _me;

        public TokenController(
            IWebHostEnvironment environment,
            SpaceAContext context,
            ITokenService tokenService,
            IMemberRepository memberRepository)
        {
            Environment = environment;
            _context = context;
            _tokenService = tokenService;
            _memberRepository = memberRepository;
            _me = new Lazy<Member>(() => tokenService.GetMember(User));
        }

        [HttpGet]
        public async Task<IActionResult> Auth()
        {
            var member = _me.Value;
            if (member == null)
            {
                return Forbid();
            }
            member = await _memberRepository.GetAsync(member.Id);
            return Ok(member.ToDto());
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Refresh(DTO.Tokens tokens)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(tokens.Token);
            if (!uint.TryParse(principal.FindFirst(JwtRegisteredClaimNames.Sub).Value, out uint memberId))
            {
                return BadRequest();
            }
            var member = await _memberRepository.GetAsync(memberId);
            if (member == null || member.RefreshToken != tokens.RefreshToken)
            {
                return BadRequest();
            }

            var newToken = _tokenService.GenerateAccessToken(principal.Claims.ToArray());
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            member.RefreshToken = newRefreshToken;
            await _context.SaveChangesAsync();

            return Ok(new DTO.Tokens
            {
                Token = newToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost]
        public async Task<IActionResult> Revoke()
        {
            await _memberRepository.UpsertRefreshTokenAsync(_me.Value.Id, null);
            return Ok();
        }

    }
}