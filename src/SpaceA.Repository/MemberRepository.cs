using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpaceA.Repository.Context;
using SpaceA.Model;
using SpaceA.Model.Entities;
using SpaceA.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SpaceA.Repository
{
    public class MemberRepository : RepositoryBase<Member, uint>, IMemberRepository
    {
        public MemberRepository(SpaceAContext context) :
            base(context, ctx => ctx.Members)
        {
        }

        public async Task<Member> GetAsync(string accountName)
        {
            return await _entitySet
            .Where(m => m.AccountName.Equals(accountName))
            .FirstOrDefaultAsync();
        }

        public async Task<Dictionary<string, string>> GetConfigAsync(uint id, params string[] keys)
        {
            var configs = await _context.Configs
            .Where(c => c.Id1.Equals(id) && keys.Contains(c.Id2))
            .ToDictionaryAsync(c => c.Key, c => c.Value);
            return configs;
        }

        public async Task<int> UpsertConfigAsync(Config config)
        {
            return await _context.Configs
            .Upsert(config)
            .On(c => new { c.Id1, c.Id2 })
            .RunAsync();
        }

        public void DisableMemberOrNot(uint id, bool disabled)
        {
            var member = new Member
            {
                Id = id,
                Disabled = disabled
            };
            _context.Attach(member);
            _context.Entry(member)
            .Property(m => m.Disabled)
            .IsModified = true;
        }

        public async Task<int> UpsertRefreshTokenAsync(uint id, string refreshToken)
        {
            var member = new Member
            {
                Id = id,
                RefreshToken = refreshToken
            };
            return await _entitySet
            .Upsert(member)
            .On(m => new { m.Id })
            .RunAsync();
        }
        //???

        public void ShareOrNotAccessTokenAsync(uint id, bool isShared)
        {
            var accessToken = new Config
            {
                Id1 = id,
                Id2 = ConfigKeys.ACCESS_TOKEN,
                IsShared = isShared 
            };
            TryAttach(accessToken);
            _context.Entry(accessToken)
            .Property(cf => cf.IsShared)
            .IsModified = true;
        }

        public async Task<PersonalAccessToken> GetPersonalAccessTokenAsync(string token)
        {
            var pat = await _context.PersonalAccessTokens
                .Where(pat => pat.Id == token)
                .Include(pat => pat.Owner)
                .FirstOrDefaultAsync();
            return pat;
        }

        public Task SavePersonalAccessTokenAsync(string token)
        {
            throw new System.NotImplementedException();
        }
    }
}