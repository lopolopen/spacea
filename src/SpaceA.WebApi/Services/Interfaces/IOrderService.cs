using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using SpaceA.Model.Entities;

namespace SpaceA.WebApi.Services.Interfaces
{
    public interface IOrderService
    {
        Task<string> GetOrder(uint? parentId, uint workItemId);
        Task<string> GetBottomOrderAsync(uint projectId, uint? parentId);
        Task<string> GetTopOrderAsync(uint projectId, uint? parentId);
        Task<string> GetMaxPreviousOrderAsync(uint projectId, uint? parentId, string order);
        Task<string> GetMinNextOrderAsync(uint projectId, uint? parentId, string order);
    }
}