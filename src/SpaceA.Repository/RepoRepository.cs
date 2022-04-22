using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpaceA.Repository.Context;
using SpaceA.Model.Entities;
using SpaceA.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SpaceA.Repository
{
    public class RepoRepository : RepositoryBase<Repo, uint>, IRepoRepository
    {
        public RepoRepository(SpaceAContext context) :
            base(context, ctx => ctx.Repos)
        {
        }
    }
}