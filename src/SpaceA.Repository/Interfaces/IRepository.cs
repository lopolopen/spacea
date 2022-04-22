using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpaceA.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace SpaceA.Repository.Interfaces
{
    public interface IRepository<TEntity>
        where TEntity : class
    {
        DbSet<TEntity> DbSet { get; }

        Task<IList<TEntity>> GetAllAsync();
        void Add(TEntity entity);

        void AddRange(IList<TEntity> entities);

        void Update(TEntity entity);

        void UpdateRange(IList<TEntity> entities);

        void Remove(TEntity entity);

        void RemoveRange(IList<TEntity> entities);

        void PartialUpdate(TEntity entity, Func<string, bool> isModified);
    }

    public interface IRepository<TEntity, TId> : IRepository<TEntity>
        where TEntity : class, IEntity<TId>, new()
    {
        Task<bool> ExistsAsync(TId id);

        Task<TEntity> GetAsync(TId id);

        void Remove(TId id);

        Task<int> ArchiveAsync<THistory>(TId id);

        Task<int> ArchiveAsync<THistory>(params TId[] ids);
    }

    public interface IRepository<TEntity, TId1, TId2> : IRepository<TEntity>
       where TEntity : class, IEntity<TId1, TId2>, new()
    {
        Task<bool> ExistsAsync(TId1 id1, TId2 id2);

        Task<TEntity> GetAsync(TId1 id1, TId2 id2);

        void Remove(TId1 id1, TId2 id2);
    }
}