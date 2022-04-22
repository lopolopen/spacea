using SpaceA.Repository.Context;
using SpaceA.Model.Entities;
using SpaceA.Model.Mapper;
using SpaceA.WebApi.Options;
using SpaceA.Repository.Interfaces;
using SpaceA.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTO = SpaceA.Model.Dto;

namespace SpaceA.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly SpaceAContext _context;
        private readonly ILogger _logger;
        private readonly IMemberRepository _memberRepository;
        private readonly ITokenService _tokenService;
        private readonly MinioOptions _minioOptions;

        public MembersController(
            SpaceAContext context,
            ILogger<MembersController> logger,
            IMemberRepository memberRepository,
            ITokenService tokenService,
            IOptions<MinioOptions> minioOptions)
        {
            _context = context;
            _logger = logger;
            _memberRepository = memberRepository;
            _tokenService = tokenService;
            _minioOptions = minioOptions.Value;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMemberAsync(DTO.Member memberDto)
        {
            var member = memberDto.ToEntity();
            _memberRepository.Add(member);
            await _context.SaveChangesAsync();
            return Ok();
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMemberAsync(uint id, DTO.Member memberDto)
        {
            var member = memberDto.ToEntity();
            member.Id = id;
            var track = _context.Entry(member);
            track.Property(m => m.FirstName).IsModified = true;
            track.Property(m => m.LastName).IsModified = true;
            track.Property(m => m.Xing).IsModified = true;
            track.Property(m => m.Ming).IsModified = true;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("{id}/avatar")]
        public async Task<IActionResult> UploadAvatar(uint id, IFormFile file, string uid)
        {
            if (file == null) return BadRequest();
            bool existing = await _memberRepository.ExistsAsync(id);
            if (!existing)
            {
                return NoContent();
            }
            uid = uid ?? Guid.NewGuid().ToString().Replace("-", "").ToLower();
            await using (var trans = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var member = new Member
                    {
                        Id = id,
                        AvatarUid = uid
                    };
                    _context.Entry(member)
                        .Property(m => m.AvatarUid)
                        .IsModified = true;
                    await _context.SaveChangesAsync();
                    var opt = _minioOptions;
                    var bucket = opt.AccessKey;
                    var filePath = $"public/members/{id}/avatar";
                    var minio = new MinioClient(opt.Endpoint, opt.AccessKey, opt.SecretKey);
                    using (var stream = file.OpenReadStream())
                    {
                        await minio.PutObjectAsync(bucket, filePath, stream, file.Length, file.ContentType);
                    }
                    await trans.CommitAsync();
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex.Message);
                    throw ex;
                }
                var memberDto = new DTO.Member()
                {
                    Id = id,
                    AvatarUid = uid
                };
                return Ok(memberDto.AvatarUrl);
            }
        }

        [HttpDelete("{id}/avatar")]
        public async Task<IActionResult> RemoveAvatarAsync(uint id)
        {
            bool existing = await _memberRepository.ExistsAsync(id);
            if (!existing)
            {
                return NoContent();
            }
            var member = new Member
            {
                Id = id,
                AvatarUid = null
            };
            _context.Entry(member)
                .Property(m => m.AvatarUid)
                .IsModified = true;
            await _context.SaveChangesAsync();
            var opt = _minioOptions;
            var bucket = opt.AccessKey;
            var filePath = $"public/members/{id}/avatar";
            var minio = new MinioClient(opt.Endpoint, opt.AccessKey, opt.SecretKey);
            try
            {
                await minio.RemoveObjectAsync(bucket, filePath);
            }
            catch (MinioException ex)
            {
                _logger.LogError(ex.Message);
            }
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMemberAsync(uint id)
        {
            var member = new Member
            {
                Id = id,
            };
            _memberRepository.Remove(member);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMembersAsync()
        {
            var memberDTOs = (await _memberRepository.GetAllAsync())
                .Select(member => member.ToDto())
                .ToList();
            return Ok(memberDTOs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMemberAsync(uint id)
        {
            var member = await _memberRepository.GetAsync(id);
            var memberDTO = member.ToDto();
            return Ok(memberDTO);
        }

        [HttpPost("{id}/disable")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DisableMemberOrNotAsync(uint id, bool disabled)
        {
            bool existing = await _memberRepository.ExistsAsync(id);
            if (!existing)
            {
                return NoContent();
            }
            _memberRepository.DisableMemberOrNot(id, disabled);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("{id}/configs")]
        public async Task<IActionResult> GetConfig(uint id, [FromQuery] List<string> keys)
        {
            bool existing = await _memberRepository.ExistsAsync(id);
            if (!existing)
            {
                return NotFound();
            }
            var configs = await _memberRepository.GetConfigAsync(id, keys.ToArray());
            return Ok(configs);
        }

        [HttpPut("{id}/configs")]
        public async Task<IActionResult> SaveConfig(uint id, Dictionary<string, string> configDTO)
        {
            bool existing = await _memberRepository.ExistsAsync(id);
            if (!existing)
            {
                return NoContent();
            }
            foreach (var kv in configDTO)
            {
                var config = new Config
                {
                    Id1 = id,
                    Key = kv.Key,
                    Value = kv.Value,
                    IsShared = false
                };
                await _memberRepository.UpsertConfigAsync(config);
            }
            return Ok();
        }

        [HttpPut("{id}/configs/accesstoken")]
        public async Task<IActionResult> ShareAccessTokenOrNotAsync(uint id, bool isShared)
        {
            bool existing = await _memberRepository.ExistsAsync(id);
            if (!existing)
            {
                return NoContent();
            }
            _memberRepository.ShareOrNotAccessTokenAsync(id, isShared);
            await _context.SaveChangesAsync();
            return Ok();
        }


    }
}