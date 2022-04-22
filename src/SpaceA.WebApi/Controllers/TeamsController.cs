using System;
using System.Threading.Tasks;
using System.Linq;
using SpaceA.Model.Entities;
using Microsoft.AspNetCore.Mvc;
using DTO = SpaceA.Model.Dto;
using SpaceA.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SpaceA.Model.Mapper;
using SpaceA.Repository.Context;

namespace SpaceA.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly SpaceAContext _context;
        private readonly ITeamRepository _teamRepository;
        private readonly IWorkItemRepository _workItemRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IMemberRepository _memberRepository;

        public TeamsController(
            SpaceAContext context,
            ITeamRepository teamRepository,
            IWorkItemRepository workItemRepository,
            IGroupRepository groupRepository,
            IMemberRepository memberRepository)
        {
            _context = context;
            _teamRepository = teamRepository;
            _workItemRepository = workItemRepository;
            _groupRepository = groupRepository;
            _memberRepository = memberRepository;
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateTeamAsync(uint id, DTO.Team teamDTO)
        {
            bool existing = await _teamRepository.ExistsAsync(id);
            if (!existing)
            {
                return NoContent();
            }
            var team = new Team
            {
                Id = id,
                Name = teamDTO.Name,
                Acronym = teamDTO.Acronym,
                Desc = teamDTO.Desc
            };
            _teamRepository.UpdateTeam(team);
            if (teamDTO.MemberIds != null)
            {
                var members = teamDTO.MemberIds
                ?.Select(mid => new Member { Id = mid })
                .ToArray();
                _teamRepository.RemoveAllMembers(team);
                _teamRepository.AddMembersToTeam(team, members);
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict();
            }
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeamAsync(uint id)
        {
            var team = new Team
            {
                Id = id,
            };
            _teamRepository.Remove(team);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("{id}/members")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMembersAsync(uint id)
        {
            bool existing = await _teamRepository.ExistsAsync(id);
            if (!existing)
            {
                return NotFound();
            }
            var members = await _teamRepository.GetMembersAsync(id);
            var memberDTOs = members.Select(m => m.ToDto())
            .ToList();
            return Ok(memberDTOs);
        }

        [HttpPost("{id}/selectiteration")]
        public async Task<IActionResult> SelectIterationAsync(uint id, uint iterationId)
        {
            if (id == 0 || iterationId == 0)
            {
                return BadRequest();
            }
            bool existing = await _teamRepository.ExistsAsync(id);
            if (!existing)
            {
                return NotFound();
            }
            await _teamRepository.AddIterationToTeamAsync(id, iterationId);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}/deselectiteration")]
        public async Task<IActionResult> DeselectIterationAsync(uint id, uint iterationId)
        {
            if (id == 0 || iterationId == 0)
            {
                return BadRequest();
            }
            bool existing = await _teamRepository.ExistsAsync(id);
            if (!existing)
            {
                return NotFound();
            }
            await _teamRepository.RemoveIterationFromTeamAsync(id, iterationId);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}/defaultiteration/{defIterationId}")]
        public async Task<IActionResult> UpdateDefaultIterationAsync(uint id, uint defIterationId)
        {
            if (id <= 0 || defIterationId == 0)
            {
                return BadRequest();
            }
            bool existing = await _teamRepository.ExistsAsync(id);
            if (!existing)
            {
                return NotFound();
            }
            using (var trans = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _teamRepository.UpdateDefaultIterationOfTeamAsync(id, defIterationId);
                    await _teamRepository.RemoveNotDescendantsAsync(id, defIterationId);
                    await _context.SaveChangesAsync();
                    await trans.CommitAsync();
                }
                catch (DbUpdateException)
                {
                    await trans.RollbackAsync();
                    return Conflict();
                }
            }
            return Ok();
        }

        [HttpPut("{id}/defaultfolder/{defFolderId}")]
        public async Task<IActionResult> UpdateDefaultFolderAsync(uint id, uint defFolderId)
        {
            if (id <= 0 || defFolderId <= 0)
            {
                return BadRequest();
            }
            bool existing = await _teamRepository.ExistsAsync(id);
            if (!existing)
            {
                return NotFound();
            }
            var team = new Team { Id = id };
            var folder = new Folder { Id = defFolderId };
            using (var trans = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (!await _context.TeamFolders
                        .Where(ti => ti.Id1 == id && ti.Id2 == defFolderId)
                        .AnyAsync())
                    {
                        _teamRepository.AddFolderToTeamAsync(team, folder);
                    }
                    team.DefaultFolderId = defFolderId;
                    _teamRepository.UpdateDefaultFolderOfTeamAsync(team);
                    await _context.SaveChangesAsync();
                    await trans.CommitAsync();
                }
                catch (DbUpdateException)
                {
                    await trans.RollbackAsync();
                    return Conflict();
                }
            }
            return Ok();
        }

        [HttpPut("{id}/selectfolder")]
        public async Task<IActionResult> SelectFolderAsync(uint id, uint folderId)
        {
            //TODO: transaction
            await _teamRepository.DbSet
            .Where(t => t.Id == id && t.DefaultFolderId == null)
            .UpdateFromQueryAsync(t => new Team { DefaultFolderId = folderId });
            await _teamRepository.AddFolderToTeamAsync(id, folderId);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}/deselectfolder")]
        public async Task<IActionResult> DeselectFolderAsync(uint id, uint folderId)
        {
            //TODO: if remove def folder
            if (id == 0 || folderId == 0)
            {
                return BadRequest();
            }
            bool existing = await _teamRepository.ExistsAsync(id);
            if (!existing)
            {
                return NotFound();
            }
            await _teamRepository.RemoveFolderFromTeamAsync(id, folderId);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("{id}/iterations/{iterationId}/capacities")]
        public async Task<IActionResult> GetMemberCapacitiesAsync(uint id, uint iterationId)
        {
            var capacities = await _context.MemberCapacities
            .Where(c => c.TeamId == id && c.IterationId == iterationId)
            .ToListAsync();
            var memberCapacities = capacities
            .GroupBy(c => c.MemberId)
            .Select(g =>
            new DTO.MemberCapacities
            {
                MemberId = g.Key,
                Capacities = g.Select(c =>
                new DTO.Capacity
                {
                    OwnerId = g.Key,
                    Type = c.Type,
                    HoursPerDay = c.HoursPerDay
                })
                .ToList()
            });
            return Ok(memberCapacities);
        }

        [HttpPut("{id}/iterations/{iterationId}/capacities")]
        public async Task<IActionResult> UpdateMemberCapacitiesAsync(uint id, uint iterationId, List<DTO.MemberCapacities> memberCapacities)
        {
            var capacities = memberCapacities.SelectMany(mc =>
            mc.Capacities.Select(c =>
            new MemberCapacity
            {
                TeamId = id,
                IterationId = iterationId,
                MemberId = mc.MemberId,
                Type = c.Type,
                HoursPerDay = c.HoursPerDay
            }));
            using (var trans = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.MemberCapacities
                    .Where(c => c.TeamId == id && c.IterationId == iterationId)
                    .DeleteFromQuery();
                    await _context.MemberCapacities.AddRangeAsync(capacities);
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
    }
}