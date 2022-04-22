using SpaceA.Repository.Context;
using SpaceA.Model.Entities;
using SpaceA.Repository.Interfaces;
using SpaceA.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DTO = SpaceA.Model.Dto;

namespace SpaceA.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IterationsController : ControllerBase
    {
        private readonly SpaceAContext _context;
        private readonly Lazy<Member> _me;
        private readonly IIterationRepository _iterationRepository;
        private readonly IWorkItemRepository _workItemRepository;
        public IterationsController(
            SpaceAContext context,
            ITokenService tokenService,
            IIterationRepository iterationRepository,
            IWorkItemRepository workItemRepository)
        {
            _context = context;
            _iterationRepository = iterationRepository;
            _workItemRepository = workItemRepository;
            _me = new Lazy<Member>(() => tokenService.GetMember(User));
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PartialUpdateIterationAsync(uint id, DTO.Iteration iterationDto)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            if (iterationDto == null || string.IsNullOrEmpty(iterationDto.Name))
            {
                return BadRequest();
            }
            // bool existing = await _iterationRepository.ExistsAsync(id);
            // if (!existing)
            // {
            //     return NoContent();
            // }
            var origin = await _iterationRepository.GetAsync(id);
            if (origin == null)
            {
                return NoContent();
            }
            string originalPath = origin.Path;
            origin.Name = iterationDto.Name;
            origin.Path = iterationDto.Path;
            origin.StartDate = iterationDto.StartDate;
            origin.EndDate = iterationDto.EndDate;
            await using (var trans = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _iterationRepository.PartialUpdate(origin, iterationDto.Allow);
                    if (iterationDto.Allow(i => i.Path))
                    {
                        var newPath = iterationDto.Path;
                        var subIterations = await _iterationRepository.DbSet
                            .Where(i => i.ProjectId == origin.ProjectId && i.Path.StartsWith(originalPath) &&
                                        i.Id != id)
                            .ToListAsync();
                        subIterations.ForEach(i => i.Path = Regex.Replace(i.Path, $"^{originalPath}/", $"{newPath}/"));
                    }

                    await _context.SaveChangesAsync();
                    await trans.CommitAsync();
                }
                catch (Exception)
                {
                    await trans.RollbackAsync();
                    throw;
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
            var from = await _iterationRepository.GetAsync(id);
            if (from == null)
            {
                return Ok();
            }
            var projectId = from.ProjectId;
            var fromPath = from.Path;
            var member = _me.Value;
            await using (var trans = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (toId.HasValue)
                    {
                        var ids = await _iterationRepository.WorkItemIdsRootedAtAsync(projectId, fromPath);
                        await _workItemRepository.ArchiveAsync<WorkItemHistory>(ids);
                        await _iterationRepository.TransferAsync(projectId, fromPath, toId.Value, member.Id);
                    }

                    await _iterationRepository.RemoveWithSubsAsync(projectId, fromPath);
                    await _context.SaveChangesAsync();
                    await trans.CommitAsync();
                }
                catch (Exception)
                {
                    await trans.RollbackAsync();
                    throw;
                }
            }
            return Ok();
        }
    }
}