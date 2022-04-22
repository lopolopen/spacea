using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using SpaceA.Repository.Context;
using SpaceA.Model;
using SpaceA.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SpaceA.Repository
{
    public abstract class RepositoryBase<TEntity> : IRepository<TEntity>
        where TEntity : class, new()
    {
        public DbSet<TEntity> DbSet => _entitySet;

        protected readonly SpaceAContext _context;
        protected readonly DbSet<TEntity> _entitySet;

        protected bool TryAttach<T>(T entity)
        {
            if (_context.Entry(entity).State != EntityState.Detached) return false;
            _context.Attach(entity);
            return true;
        }

        protected bool TryAttachRange(params object[] entities)
        {
            bool flag = true;
            foreach (var e in entities)
            {
                flag = TryAttach(e) && false;
            }
            return flag;
        }

        public RepositoryBase(SpaceAContext context,
            Func<SpaceAContext, DbSet<TEntity>> getEntity)
        {
            _context = context;
            _entitySet = getEntity(context);
        }

        public void Add(TEntity entity)
        {
            _entitySet.Add(entity);
        }

        public void AddRange(IList<TEntity> entities)
        {
            _entitySet.AddRange(entities.ToArray());
        }

        public virtual async Task<IList<TEntity>> GetAllAsync()
        {
            return await _entitySet.ToListAsync();
        }

        public void Remove(TEntity entity)
        {
            _entitySet.Remove(entity);
        }

        public void RemoveRange(IList<TEntity> entities)
        {
            _entitySet.RemoveRange(entities.ToList());
        }

        public void Update(TEntity entity)
        {
            _entitySet.Update(entity);
        }

        public void UpdateRange(IList<TEntity> entities)
        {
            _entitySet.UpdateRange(entities.ToList());
        }

        public void PartialUpdate(TEntity entity, Func<string, bool> isModified)
        {
            TryAttach(entity);
            var type = entity.GetType();
            var properties = type.GetProperties();
            foreach (var p in properties)
            {
                if (!isModified(p.Name)) continue;
                _context.Entry(entity)
                .Property(p.Name)
                .IsModified = true;
            }
        }
    }

    public class RepositoryBase<TEntity, TId> : RepositoryBase<TEntity>, IRepository<TEntity, TId>
        where TEntity : class, IEntity<TId>, new()
    {
        public RepositoryBase(SpaceAContext context, Func<SpaceAContext, DbSet<TEntity>> getEntity) :
            base(context, getEntity)
        {
        }

        public async Task<int> ArchiveAsync<THistory>(TId id)
        {
            var from = _context.Model.FindEntityType(typeof(TEntity)).GetTableName();
            var to = _context.Model.FindEntityType(typeof(THistory)).GetTableName();
            return await _context.Database.ExecuteSqlRawAsync(
                $"INSERT INTO `{to}` SELECT * FROM `{from}` WHERE `Id` = @p0", id);
        }

        public async Task<int> ArchiveAsync<THistory>(params TId[] ids)
        {
            if (ids.Length == 0) return 0;
            var from = _context.Model.FindEntityType(typeof(TEntity)).GetTableName();
            var to = _context.Model.FindEntityType(typeof(THistory)).GetTableName();
            var paramsLst = string.Join(',', ids.Select((_, idx) => $"@p{idx}"));
            var @params = ids.Cast<object>();
            return await _context.Database.ExecuteSqlRawAsync(
                $"INSERT INTO `{to}` SELECT * FROM `{from}` WHERE `Id` IN ({paramsLst})", @params);
        }

        public virtual async Task<bool> ExistsAsync(TId id)
        {
            return await _entitySet
            .AnyAsync(e => e.Id.Equals(id));
        }

        public async Task<TEntity> GetAsync(TId id)
        {
            return await _entitySet
            .Where(e => e.Id.Equals(id))
            .SingleOrDefaultAsync();
        }

        public void Remove(TId id)
        {
            this.Remove(new TEntity { Id = id });
        }
    }

    public class RepositoryBase<TEntity, TId1, TId2> : RepositoryBase<TEntity>, IRepository<TEntity, TId1, TId2>
        where TEntity : class, IEntity<TId1, TId2>, new()
    {
        public RepositoryBase(SpaceAContext context, Func<SpaceAContext, DbSet<TEntity>> getEntity) :
            base(context, getEntity)
        {
        }

        public virtual async Task<bool> ExistsAsync(TId1 id1, TId2 id2)
        {
            return await _entitySet
            .AnyAsync(e => e.Id1.Equals(id1) && e.Id2.Equals(id2));
        }

        public async Task<TEntity> GetAsync(TId1 id1, TId2 id2)
        {
            return await _entitySet
            .Where(e => e.Id1.Equals(id1) && e.Id2.Equals(2))
            .SingleOrDefaultAsync();
        }

        public void Remove(TId1 id1, TId2 id2)
        {
            this.Remove(
                new TEntity
                {
                    Id1 = id1,
                    Id2 = id2
                });
        }
    }
}