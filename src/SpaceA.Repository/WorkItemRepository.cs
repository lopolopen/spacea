using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using SpaceA.Repository.Context;
using SpaceA.Model.Entities;
using SpaceA.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using SpaceA.Model;

namespace SpaceA.Repository
{
    [Table("WorkItem")]
    public class WorkItemRepository : RepositoryBase<WorkItem, uint>, IWorkItemRepository
    {

        public WorkItemRepository(SpaceAContext context) :
            base(context, ctx => ctx.WorkItems)
        {
        }

        public async Task<int> GetRevAsync(uint id)
        {
            return await _entitySet
            .Where(wi => wi.Id == id)
            .Select(wi => wi.Rev)
            .SingleOrDefaultAsync();
        }

        public async Task<WorkItem> GetWorkItemAsync(uint id)
        {
            var workItem = await _entitySet
            .Include(wi => wi.AssignedTo)
            .Include(wi => wi.Changer)
            .Include(wi => wi.Folder)
            .Include(wi => wi.Attachments)
            .ThenInclude(a => a.Creator)
            .Where(wi => wi.Id == id)
            .FirstOrDefaultAsync();
            return workItem;
        }

        public async Task<WorkItem> GetWorkItemDetailAsync(uint id)
        {
            var detail = await _entitySet
            .Where(wi => wi.Id == id)
            .Include(wi => wi.Attachments)
            .ThenInclude(a => a.Creator)
            .Select(wi => new WorkItem
            {
                Id = wi.Id,
                Desc = wi.Desc,
                AcceptCriteria = wi.AcceptCriteria,
                ReproSteps = wi.ReproSteps,
                Attachments = wi.Attachments
            })
            .FirstOrDefaultAsync();
            return detail;
        }

        public async Task<IList<WorkItem>> GetInProgressWorkItemsToMe(uint memberId)
        {
            return await _entitySet.Include(wi => wi.Project)
            .Where(wi => wi.AssigneeId == memberId)
            .Where(wi => wi.State == WorkItemState.New || wi.State == WorkItemState.Active)
            .Where(wi => wi.Project.DeletedFlag == Guid.Empty)
            .Select(wi => new WorkItem
            {
                Id = wi.Id,
                Title = wi.Title,
                Type = wi.Type,
                State = wi.State,
                Priority = wi.Priority,
                ChangedDate = wi.ChangedDate,
                Project = new Project
                {
                    Id = wi.Project.Id,
                    Name = wi.Project.Name
                }
            })
            .ToListAsync();
        }

        public async Task<IList<WorkItem>> GetInProgressWorkItemsByMe(uint memberId)
        {
            return await _entitySet.Include(wi => wi.Project)
            .Where(wi => wi.CreatorId == memberId)
            .Where(wi => wi.State == WorkItemState.New || wi.State == WorkItemState.Active)
            .Where(wi => wi.Project.DeletedFlag == Guid.Empty)
            .Select(wi => new WorkItem
            {
                Id = wi.Id,
                Title = wi.Title,
                Type = wi.Type,
                State = wi.State,
                Priority = wi.Priority,
                ChangedDate = wi.ChangedDate,
                Project = new Project
                {
                    Id = wi.Project.Id,
                    Name = wi.Project.Name
                }
            })
            .ToListAsync();
        }

        public async Task<List<Tag>> GetTagsAsync(uint id)
        {
            return await _entitySet.Include(wi => wi.Tags)
            .Where(wi => wi.Id == id)
            .SelectMany(wi => wi.Tags)
            .ToListAsync();
        }

        public async Task<Dictionary<uint, List<Tag>>> GetTagsAsync(params uint[] ids)
        {
            var xs = await _entitySet.Include(wi => wi.Tags)
            .Where(wi => ids.Contains(wi.Id))
            .Select(wi => new { wi.Id, wi.Tags })
            .ToListAsync();
            return xs.ToDictionary(wi => wi.Id, wi => wi.Tags?.ToList());
        }

        public void UpdateParentId(uint id, uint? parentId)
        {
            var workItem = new WorkItem
            {
                Id = id,
                ParentId = parentId
            };
            _context.Attach(workItem);
            _context.Entry(workItem)
            .Property(wi => wi.ParentId)
            .IsModified = true;
        }

        public async Task<WorkItem> GetWorkItemSummaryAsync(uint id)
        {
            return await _entitySet
            .Include(wi => wi.AssignedTo)
            .Where(wi => wi.Id == id)
            .Select(wi => new WorkItem
            {
                Id = wi.Id,
                Type = wi.Type,
                Title = wi.Title,
                State = wi.State,
                Priority = wi.Priority,
                AssignedTo = wi.AssignedTo,
                ChangedDate = wi.ChangedDate
            })
            .FirstOrDefaultAsync();
        }

        public async Task<List<WorkItem>> GetChildrenSummaryAsync(uint id)
        {
            return await _entitySet
            .Include(wi => wi.AssignedTo)
            .Where(wi => wi.ParentId == id)
            .Select(wi => new WorkItem
            {
                Id = wi.Id,
                Order = wi.Order,
                Type = wi.Type,
                Title = wi.Title,
                State = wi.State,
                Priority = wi.Priority,
                AssignedTo = wi.AssignedTo,
                ChangedDate = wi.ChangedDate
            })
            .ToListAsync();
        }

        public async Task<bool> ExistsTestSuiteAsync(uint? parentId)
        {
            return await _entitySet
            .Where(e => e.ParentId.Equals(parentId))
            .AnyAsync(e => e.Type.Equals(WorkItemType.TestSuite));
        }

        public async Task<List<WorkItemHistory>> GetHistoriesAsync(uint id)
        {
            return await _context.WorkItemHistories
            .Where(wih => wih.Id == id)
            .OrderBy(wi => wi.Rev)
            .ToListAsync();
        }

        public async Task<List<WorkItem>> GetProdBugsAsync(DateTime? from, DateTime? to, bool showClosedItem)
        {
            to = to?.AddDays(1);
            return await _context.WorkItems
            .Include(wi => wi.Project)
            .ThenInclude(p => p.Owner)
            .Include(wi => wi.AssignedTo)
            .Include(wi => wi.Creator)
            .Where(wi => wi.Type == WorkItemType.Bug)
            .Where(wi => wi.Environment == "Production")
            .Where(wi => wi.State != WorkItemState.Removed)
            .Where(wi => showClosedItem || wi.State != WorkItemState.Closed)
            .Where(wi => from == null || to == null || (wi.CreatedDate >= from && wi.CreatedDate < to))
            .ToListAsync();
        }

        public async Task<List<WorkItem>> GetCodeReviewsAsync(DateTime? from, DateTime? to, bool showClosedItem)
        {
            to = to?.AddDays(1);
            return await _context.WorkItems
            // .Include(wi => wi.Project)
            // .ThenInclude(p => p.Owner)
            .Include(wi => wi.AssignedTo)
            .Include(wi => wi.Creator)
            .Include(wi => wi.Iteration)
            //TODO: CR暂时没有强约束
            .Where(wi => wi.ProjectId == 68)
            .Where(wi => wi.State != WorkItemState.Removed)
            .Where(wi => showClosedItem || wi.State != WorkItemState.Closed)
            .Where(wi => from == null || to == null || (wi.CreatedDate >= from && wi.CreatedDate < to))
            .ToListAsync();
        }
    }
}