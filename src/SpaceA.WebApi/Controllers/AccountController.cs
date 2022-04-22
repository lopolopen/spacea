using System;
using System.Threading.Tasks;
using SpaceA.Model.Entities;
using Microsoft.AspNetCore.Mvc;
using DTO = SpaceA.Model.Dto;
using SpaceA.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SpaceA.WebApi.Services.Interfaces;
using SpaceA.Model.Mapper;
using Microsoft.AspNetCore.Hosting;
using SpaceA.Repository.Context;
using SpaceA.Common;

namespace SpaceA.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private IConfiguration Configuration { get; }
        private IWebHostEnvironment Environment { get; }
        private readonly SpaceAContext _context;
        private readonly IMemberRepository _memberRepository;
        private readonly ITokenService _tokenService;
        private readonly ILdapService _ldapService;

        public AccountsController(
            IConfiguration configuration,
            IWebHostEnvironment environment,
            SpaceAContext context,
            ITokenService tokenService,
            ILdapService ldapService,
            IMemberRepository memberRepository)
        {
            Configuration = configuration;
            Environment = environment;
            _context = context;
            _tokenService = tokenService;
            _ldapService = ldapService;
            _memberRepository = memberRepository;
        }

        [AllowAnonymous]
        [HttpPost("sign_in")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SignIn(DTO.SignInModel signIn)
        {
            var accountName = signIn.AccountName;
            var password = signIn.Password;
            return await SignInLocalAsync(accountName, password);
        }

        [Authorize]
        private async Task<IActionResult> SignInLocalAsync(string accountName, string password)
        {
            if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(password))
            {
                return BadRequest();
            }

            var me = await _memberRepository.GetAsync(accountName);
            if (me == null || me.Disabled)
            {
                return Forbid();
            }

            var pwd = PasswordUtil.SaltHashPasswordWithSHA1(password, me.Salt);
            if (pwd != me.Password)
            {
                return Forbid();
            }

            var token = _tokenService.GenerateAccessToken(me);
            var refreshToken = _tokenService.GenerateRefreshToken();
            me.RefreshToken = refreshToken;
            await _context.SaveChangesAsync();
            return Ok(new
            {
                Member = me.ToDto(),
                Tokens = new DTO.Tokens
                {
                    Token = token,
                    RefreshToken = refreshToken
                }
            });
        }
    }
}