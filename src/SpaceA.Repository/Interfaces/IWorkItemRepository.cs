using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpaceA.Model;
using SpaceA.Model.Entities;
using DTO = SpaceA.Model.Dto;

namespace SpaceA.Repository.Interfaces
{
    public interface IWorkItemRepository : IRepository<WorkItem, uint>
    {
        Task<int> GetRevAsync(uint id);

        Task<WorkItem> GetWorkItemAsync(uint id);

        Task<WorkItem> GetWorkItemDetailAsync(uint id);

        Task<IList<WorkItem>> GetInProgressWorkItemsToMe(uint memberId);

        Task<IList<WorkItem>> GetInProgressWorkItemsByMe(uint memberId);

        Task<List<Tag>> GetTagsAsync(uint id);

        Task<Dictionary<uint, List<Tag>>> GetTagsAsync(params uint[] ids);


        void UpdateParentId(uint id, uint? parentId);

        Task<WorkItem> GetWorkItemSummaryAsync(uint id);

        Task<List<WorkItem>> GetChildrenSummaryAsync(uint id);

        Task<bool> ExistsTestSuiteAsync(uint? parentId);

        Task<List<WorkItemHistory>> GetHistoriesAsync(uint id);

        Task<List<WorkItem>> GetProdBugsAsync(DateTime? from, DateTime? to, bool showClosedItem);

        Task<List<WorkItem>> GetCodeReviewsAsync(DateTime? from, DateTime? to, bool showClosedItem);
    }
}