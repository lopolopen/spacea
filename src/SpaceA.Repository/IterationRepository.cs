using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpaceA.Repository.Context;
using SpaceA.Model.Entities;
using SpaceA.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SpaceA.Repository
{
    public class IterationRepository : RepositoryBase<Iteration, uint>, IIterationRepository
    {

        public IterationRepository(SpaceAContext context) :
            base(context, ctx => ctx.Iterations)
        {
        }

        public async Task<IList<Iteration>> GetIterationsAsync(uint projectId)
        {
            return await _entitySet
            .Where(e => e.ProjectId == projectId)
            .ToListAsync();
        }

        public async Task<IList<Iteration>> GetIterationsOfTeamAsync(uint teamId)
        {
            return await _context.TeamIterations
            .Where(ti => ti.Id1 == teamId)
            .Include(i => i.Iteration)
            .Select(ti => ti.Iteration)
            .ToListAsync();
        }

        public async Task<Dictionary<uint, List<Iteration>>> GetIterationMapAsync(params uint[] teamIds)
        {
            var xs = await _context.TeamIterations
            .Where(ti => teamIds.Contains(ti.Id1))
            .Include(ti => ti.Iteration)
            .Select(ti => new
            {
                TeamId = ti.Id1,
                ti.Iteration
            })
            .ToListAsync();
            return xs.GroupBy(x => x.TeamId)
            .ToDictionary(
                g => g.Key,
                g => g?.Select(x => x.Iteration).ToList()
            );
        }

        public async Task<Dictionary<uint, List<uint>>> GetIterationIdMapAsync(params uint[] teamIds)
        {
            var xs = await _context.TeamIterations
            .Where(ti => teamIds.Contains(ti.Id1))
            .Select(ti => new
            {
                TeamId = ti.Id1,
                IterationId = ti.Id2
            })
            .ToListAsync();
            return xs.GroupBy(x => x.TeamId)
            .ToDictionary(
                g => g.Key,
                g => g?.Select(x => x.IterationId).ToList()
            );
        }

        public async Task<uint[]> WorkItemIdsRootedAtAsync(uint projectId, string path)
        {
            var ids = await
            (
                from wi in _context.WorkItems
                join i in _entitySet
                on wi.IterationId equals i.Id
                where i.Path == path || i.Path.StartsWith($"{path}/")
                where i.ProjectId == projectId
                select wi.Id
            )
            .ToArrayAsync();
            return ids;
        }

        public async Task TransferAsync(uint projectId, string fromPath, uint toId, uint changerId)
        {
            var path = $"{fromPath}/";
            var wis = from wi in _context.WorkItems
                      join i in _entitySet
                      on wi.IterationId equals i.Id
                      where i.Path == fromPath || i.Path.StartsWith(path)
                      where i.ProjectId == projectId
                      select wi;
            await wis.UpdateFromQueryAsync(
                wi => new WorkItem
                {
                    Rev = wi.Rev + 1,
                    ChangedDate = DateTime.Now,
                    ChangerId = changerId,
                    IterationId = toId
                });
        }

        public async Task RemoveWithSubsAsync(uint projectId, string rootPath)
        {
            var @is = _entitySet
            .Where(i => i.Path == rootPath || i.Path.StartsWith($"{rootPath}/"))
            .Where(i => i.ProjectId == projectId);
            await @is.DeleteFromQueryAsync();
        }
    }
}