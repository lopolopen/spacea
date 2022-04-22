using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpaceA.Repository.Context;
using SpaceA.Model.Entities;
using SpaceA.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SpaceA.Repository
{
    public class GroupRepository : RepositoryBase<Group, uint>, IGroupRepository
    {
        public GroupRepository(SpaceAContext context) :
            base(context, ctx => ctx.Groups)
        {
        }

        public async Task<List<Member>> GetMembersAsync(uint id)
        {
            var members = await _entitySet
            .Include(g => g.GroupMembers)
            .ThenInclude(gm => gm.Member)
            .Where(g => g.Id == id)
            .SelectMany(g => g.GroupMembers.Select(gm => gm.Member))
            .ToListAsync();
            return members;
        }

        public async Task<Dictionary<uint, List<Member>>> GetMemberMapAsync(params uint[] ids)
        {
            var xs = await _entitySet
            .Include(g => g.GroupMembers)
            .ThenInclude(gm => gm.Member)
            .Where(g => ids.Contains(g.Id))
            .Select(g => new
            {
                g.Id,
                g.GroupMembers
            })
            .ToListAsync();
            return xs.ToDictionary(
                x => x.Id,
                x => x.GroupMembers?.Select(gm => gm.Member)
                .ToList());
        }

        public async Task<List<Member>> GetUngroupedMembersAsync()
        {
            return await _context.Members
            .Include(m => m.GroupMembers)
            .Where(m => m.GroupMembers.Count == 0)
            .ToListAsync();
        }

        public async Task<Dictionary<uint, Group>> GetGroupMapAsync(params uint[] ids)
        {
            var xs = await _entitySet
            .Where(g => ids.Contains(g.Id))
            .Select(g =>
            new Group
            {
                Id = g.Id,
                AccountName = g.AccountName,
                Name = g.Name,
                Acronym = g.Acronym
            })
            .ToListAsync();
            return xs.ToDictionary(x => x.Id, x => x);
        }

        public void EnOrDisableGroup(uint id, bool disabled)
        {
            var group = new Group
            {
                Id = id,
                Disabled = disabled
            };
            _context.Attach(group);
            _context.Entry(group)
            .Property(p => p.Disabled)
            .IsModified = true;
        }

        public void UpdateGroup(Group group)
        {
            _context.Attach(group);
            _context.Entry(group)
            .Property(p => p.Name)
            .IsModified = true;
            _context.Entry(group)
            .Property(p => p.Acronym)
            .IsModified = true;
            _context.Entry(group)
            .Property(p => p.Desc)
            .IsModified = true;
            _context.Entry(group)
            .Property(p => p.Disabled)
            .IsModified = true;
        }
    }
}