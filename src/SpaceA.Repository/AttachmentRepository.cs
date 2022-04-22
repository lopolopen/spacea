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
    public class AttachmentRepository : RepositoryBase<Attachment, Guid>, IAttachmentRepository
    {
        public AttachmentRepository(SpaceAContext context)
        : base(context, ctx => ctx.Attachments)
        {
        }

        public async Task UpdateWorkItemIdAsync(Guid id, uint? workItemId)
        {
            await _entitySet
            .Where(a => a.Id == id)
            .UpdateFromQueryAsync(a =>
            new Attachment
            {
                WorkItemId = workItemId
            });
        }
    }
}