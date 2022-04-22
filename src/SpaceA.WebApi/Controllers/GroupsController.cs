using System;
using System.Threading.Tasks;
using System.Linq;
using SpaceA.Model.Entities;
using Microsoft.AspNetCore.Mvc;
using DTO = SpaceA.Model.Dto;
using SpaceA.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using SpaceA.Model.Mapper;
using SpaceA.Repository.Context;

namespace SpaceA.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly SpaceAContext _context;
        private readonly IGroupRepository _groupRepository;

        public GroupsController(SpaceAContext context, IGroupRepository groupRepository)
        {
            _context = context;
            _groupRepository = groupRepository;
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateGroupAsync(uint id, DTO.Group groupDTO)
        {
            bool existing = await _groupRepository.ExistsAsync(id);
            if (!existing)
            {
                return NoContent();
            }
            if (groupDTO.Disabled)
            {
                _groupRepository.EnOrDisableGroup(id, true);
                await _context.SaveChangesAsync();
            }
            else
            {
                var group = new Group
                {
                    Id = id,
                    Name = groupDTO.Name,
                    Desc = groupDTO.Desc,
                    Acronym = groupDTO.Acronym,
                    Disabled = false
                };
                _groupRepository.UpdateGroup(group);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteGroupAsync(uint id)
        {
            bool existing = await _groupRepository.ExistsAsync(id);
            if (!existing)
            {
                return NoContent();
            }
            var group = new Group
            {
                Id = id,
            };
            _groupRepository.Remove(group);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGroupsAsync(bool? disabled)
        {
            //TODO: 可以直接GetGroupsWithMembersAsync
            var groups = await _groupRepository.GetAllAsync();
            var groupIds = groups.Select(g => g.Id).ToArray();
            var membersMap = await _groupRepository.GetMemberMapAsync(groupIds);
            var ungrouped = await _groupRepository.GetUngroupedMembersAsync();
            var groupDTOs = groups.Select(g =>
            {
                membersMap.TryGetValue(g.Id, out var members);
                return new DTO.Group
                {
                    Id = g.Id,
                    AccountName = g.AccountName,
                    Name = g.Name,
                    Acronym = g.Acronym,
                    Desc = g.Desc,
                    Disabled = g.Disabled,
                    LeaderId = g.LeaderId,
                    Members = members?.Select(m => m.ToDto())
                    .ToList()
                };
            })
            .Where(g => disabled == null || disabled.Value == g.Disabled)
            .Concat(new[]{new DTO.Group{
                Id = 0,
                AccountName = "_ungrouped",
                Name = "未分组",
                Members= ungrouped?.Select(m =>m.ToDto())
                    .ToList()
            }})
            .ToList();
            return Ok(groupDTOs);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGroupsAsync(uint id)
        {
            //TODO: 可以直接GetGroupsWithMembersAsync
            var groups = await _groupRepository.GetAsync(id);
            var membersMap = await _groupRepository.GetMemberMapAsync(groups.Id);
            membersMap.TryGetValue(groups.Id, out var members);
            var groupDTOs = new DTO.Group
            {
                Id = groups.Id,
                AccountName = groups.AccountName,
                Name = groups.Name,
                Desc = groups.Desc,
                LeaderId = groups.LeaderId,
                Members = members.Select(m => m.ToDto())
                .ToList()
            };
            return Ok(groupDTOs);
        }
    }
}