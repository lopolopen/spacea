using System;
using System.Threading.Tasks;
using System.Linq;
using SpaceA.Model.Entities;
using Microsoft.AspNetCore.Mvc;
using DTO = SpaceA.Model.Dto;
using SpaceA.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using SpaceA.Repository.Context;
using System.Text.RegularExpressions;
using SpaceA.WebApi.Services.Interfaces;

namespace SpaceA.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoldersController : ControllerBase
    {
        private readonly SpaceAContext _context;
        private readonly Lazy<Member> _me;
        private readonly IFolderRepository _folderRepository;
        private readonly IWorkItemRepository _workItemRepository;
        public FoldersController(
            SpaceAContext context,
            ITokenService tokenService,
            IFolderRepository folderRepository,
            IWorkItemRepository workItemRepository)
        {
            _context = context;
            _folderRepository = folderRepository;
            _workItemRepository = workItemRepository;
            _me = new Lazy<Member>(() => tokenService.GetMember(User));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFolderAsync(uint id, DTO.Folder folderDto)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            if (folderDto == null || string.IsNullOrEmpty(folderDto.Name))
            {
                return BadRequest();
            }
            // bool existing = await _folderRepository.ExistsAsync(id);
            // if (!existing)
            // {
            //     return NoContent();
            // }
            var origin = await _folderRepository.GetAsync(id);
            if (origin == null)
            {
                return NoContent();
            }
            string originalPath = origin.Path;

            origin.Name = folderDto.Name;
            origin.Path = folderDto.Path;
            using (var trans = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _folderRepository.DbSet
                    .Where(f => f.Id == id)
                    .UpdateFromQueryAsync(f =>
                    new Folder
                    {
                        Name = folderDto.Name,
                        Path = folderDto.Path
                    });
                    var newPath = folderDto.Path;
                    var subIterations = await _folderRepository.DbSet
                    .Where(i => i.ProjectId == origin.ProjectId && i.Path.StartsWith(originalPath) && i.Id != id)
                    .ToListAsync();
                    subIterations.ForEach(i => i.Path = Regex.Replace(i.Path, $"^{originalPath}/", $"{newPath}/"));
                    await _context.SaveChangesAsync();
                    await trans.CommitAsync();
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    throw ex;
                }
            }
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveAsync(uint id, uint? toId)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            var from = await _folderRepository.GetAsync(id);
            if (from == null)
            {
                return Ok();
            }
            var projectId = from.ProjectId;
            var fromPath = from.Path;
            var member = _me.Value;
            using (var trans = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (toId.HasValue)
                    {
                        var ids = await _folderRepository.WorkItemIdsRootedAtAsync(projectId, fromPath);
                        await _workItemRepository.ArchiveAsync<WorkItemHistory>(ids);
                        await _folderRepository.TransferAsync(projectId, fromPath, toId.Value, member.Id);
                    }
                    await _folderRepository.RemoveWithSubsAsync(projectId, fromPath);
                    await _context.SaveChangesAsync();
                    await trans.CommitAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    throw ex;
                }
            }
        }
    }
}