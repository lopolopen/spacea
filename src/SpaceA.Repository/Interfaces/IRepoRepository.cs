using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpaceA.Model.Entities;

namespace SpaceA.Repository.Interfaces
{
    public interface IRepoRepository : IRepository<Repo, uint>
    {
    }
}