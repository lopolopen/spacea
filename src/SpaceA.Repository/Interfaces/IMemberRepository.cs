using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpaceA.Model.Entities;

namespace SpaceA.Repository.Interfaces
{
    public interface IMemberRepository : IRepository<Member, uint>
    {
        Task<Member> GetAsync(string accountName);

        Task<Dictionary<string, string>> GetConfigAsync(uint id, params string[] keys);

        Task<int> UpsertConfigAsync(Config config);

        void DisableMemberOrNot(uint id, bool disabled);

        Task<int> UpsertRefreshTokenAsync(uint id, string refreshToken);

        void ShareOrNotAccessTokenAsync(uint id, bool isShared);

        Task SavePersonalAccessTokenAsync(string token);

        Task<PersonalAccessToken> GetPersonalAccessTokenAsync(string token);
    }
}