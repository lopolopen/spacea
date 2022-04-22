using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpaceA.Model.Entities;

namespace SpaceA.Repository.Interfaces
{
    public interface IGroupRepository : IRepository<Group, uint>
    {
        Task<Dictionary<uint, List<Member>>> GetMemberMapAsync(params uint[] ids);

        Task<Dictionary<uint, Group>> GetGroupMapAsync(params uint[] ids);

        void EnOrDisableGroup(uint id, bool disabled);
        void UpdateGroup(Group group);

        Task<List<Member>> GetMembersAsync(uint id);

        Task<List<Member>> GetUngroupedMembersAsync();
    }
}