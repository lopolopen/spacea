using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpaceA.Model.Entities;

namespace SpaceA.Repository.Interfaces
{
    public interface IAttachmentRepository : IRepository<Attachment, Guid>
    {
        Task UpdateWorkItemIdAsync(Guid id, uint? workItemId);
    }
}