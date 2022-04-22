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
    public class FolderRepository : RepositoryBase<Folder, uint>, IFolderRepository
    {

        public FolderRepository(SpaceAContext context) :
            base(context, ctx => ctx.Folders)
        {
        }

        public async Task<IList<Folder>> GetFoldersAsync(uint projectId)
        {
            return await _entitySet
            .Where(e => e.ProjectId == projectId)
            .ToListAsync();
        }

        public async Task<Dictionary<uint, List<Folder>>> GetFolderMapAsync(params uint[] teamIds)
        {
            var xs = await _context.TeamFolders
            .Where(tf => teamIds.Contains(tf.Id1))
            .Include(tf => tf.Folder)
            .Select(tf => new
            {
                TeamId = tf.Id1,
                tf.Folder
            })
            .ToListAsync();
            return xs.GroupBy(x => x.TeamId)
            .ToDictionary(
                g => g.Key,
                g => g?.Select(x => x.Folder).ToList()
            );
        }

        public async Task<uint[]> WorkItemIdsRootedAtAsync(uint projectId, string path)
        {
            var ids = await
            (
                from wi in _context.WorkItems
                join f in _entitySet
                on wi.FolderId equals f.Id
                where f.Path == path || f.Path.StartsWith($"{path}/")
                where f.ProjectId == projectId
                select wi.Id
            )
            .ToArrayAsync();
            return ids;
        }

        public async Task TransferAsync(uint projectId, string fromPath, uint toId, uint changerId)
        {
            var path = $"{fromPath}/";
            var wis = from wi in _context.WorkItems
                      join f in _entitySet
                      on wi.FolderId equals f.Id
                      where f.Path == fromPath || f.Path.StartsWith(path)
                      where f.ProjectId == projectId
                      select wi;
            await wis.UpdateFromQueryAsync(
                wi => new WorkItem
                {
                    Rev = wi.Rev + 1,
                    ChangedDate = DateTime.Now,
                    ChangerId = changerId,
                    FolderId = toId
                });
        }

        public async Task RemoveWithSubsAsync(uint projectId, string rootPath)
        {
            var fs = _entitySet
            .Where(f => f.Path == rootPath || f.Path.StartsWith($"{rootPath}/"))
            .Where(f => f.ProjectId == projectId);
            await fs.DeleteFromQueryAsync();
        }
    }
}