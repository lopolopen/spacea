using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpaceA.Repository.Context;
using SpaceA.Model.Entities;
using SpaceA.Repository.Interfaces;

namespace SpaceA.Repository
{
    public class TagRepository : RepositoryBase<Tag, uint>, ITagRepository
    {
        public TagRepository(SpaceAContext context) :
            base(context, ctx => ctx.Tags)
        {
        }
    }
}