using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SpaceA.Repository.Interfaces;
using SpaceA.WebApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace SpaceA.WebApi.Services
{
    public class OrderService : IOrderService
    {
        private const char ORDER_SEPARATOR = ':';
        private const string DEF_ORDER = "k";
        private readonly IWorkItemRepository _workItemRepository;

        public OrderService(IWorkItemRepository workItemRepository)
        {
            _workItemRepository = workItemRepository;
        }

        public async Task<string> GetTopOrderAsync(uint projectId, uint? parentId)
        {
            string min = await _workItemRepository.DbSet
            .Where(wi => wi.ProjectId == projectId && wi.ParentId == parentId)
            .MinAsync(wi => wi.Order);
            return Decrease(min, null);
        }

        public async Task<string> GetBottomOrderAsync(uint projectId, uint? parentId)
        {
            string max = await _workItemRepository.DbSet
            .Where(wi => wi.ProjectId == projectId && wi.ParentId == parentId)
            .MaxAsync(wi => wi.Order);
            return Increase(max, null);
        }

        public async Task<string> GetMaxPreviousOrderAsync(uint projectId, uint? parentId, string order)
        {
            return await _workItemRepository.DbSet
            .Where(wi => wi.ProjectId == projectId && wi.ParentId == parentId && wi.Order.CompareTo(order) < 0)
            .MaxAsync(wi => wi.Order);
        }

        public async Task<string> GetMinNextOrderAsync(uint projectId, uint? parentId, string order)
        {
            return await _workItemRepository.DbSet
            .Where(wi => wi.ProjectId == projectId && wi.ParentId == parentId && wi.Order.CompareTo(order) > 0)
            .MinAsync(wi => wi.Order);
        }

        public async Task<string> GetOrder(uint? parentId, uint workItemId)
        {
            return await _workItemRepository.DbSet
            .Where(wi => wi.Id == workItemId && wi.ParentId == parentId)
            .Select(wi => wi.Order)
            .FirstOrDefaultAsync();
        }

        public static string Increase(string order, string guard)
        {
            if (string.IsNullOrEmpty(order)) return DEF_ORDER;
            if (string.IsNullOrEmpty(guard))
            {
                char last = order[order.Length - 1];
                if (last == 'z')
                {
                    return $"{order}{ORDER_SEPARATOR}{DEF_ORDER}";
                }
                last = (char)(last + (char)1);
                return $"{order.Substring(0, order.Length - 1)}{last}";
            }
            else if (string.Compare(order, guard) < 0)
            {
                if (order.Length == guard.Length)
                {
                    return $"{order}{ORDER_SEPARATOR}{DEF_ORDER}";
                }
                else if (guard.StartsWith(order))
                {
                    return Decrease(guard, null);
                }
                else
                {
                    return Increase(order, null);
                }
            }
            throw new Exception($"{nameof(order)}必须小于{nameof(guard)}");
        }

        public static string Decrease(string order, string guard)
        {
            if (string.IsNullOrEmpty(order)) return DEF_ORDER;


            if (string.IsNullOrEmpty(guard))
            {
                char last = order[order.Length - 1];
                if (last == 'b')
                {
                    return $"{order.Substring(0, order.Length - 1)}a{ORDER_SEPARATOR}{DEF_ORDER}";
                }
                last = (char)(last - (char)1);
                return $"{order.Substring(0, order.Length - 1)}{last}";
            }
            else if (string.Compare(order, guard) > 0)
            {
                if (order.Length == guard.Length)
                {
                    return $"{guard}{ORDER_SEPARATOR}{DEF_ORDER}";
                }
                else if (order.StartsWith(guard))
                {
                    return Decrease(order, null);
                }
                else
                {
                    return Increase(guard, null);
                }
            }
            throw new Exception($"{nameof(order)}必须大于{nameof(guard)}");
        }
    }
}