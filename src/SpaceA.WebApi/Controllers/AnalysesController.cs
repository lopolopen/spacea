
using System;
using System.Linq;
using System.Threading.Tasks;
using SpaceA.Repository.Context;
using SpaceA.Model;
using SpaceA.Model.Entities;
using SpaceA.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DTO = SpaceA.Model.Dto;

namespace SpaceA.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalysesController : ControllerBase
    {
        private SpaceAContext _context;
        private ITeamRepository _teamRepository;
        private IProjectRepository _projectRepository;
        private IWorkItemRepository _workItemRepository;

        public AnalysesController(
            SpaceAContext context,
            IProjectRepository projectRepository,
            ITeamRepository teamRepository,
            IWorkItemRepository workItemRepository)
        {
            _context = context;
            _projectRepository = projectRepository;
            _teamRepository = teamRepository;
            _workItemRepository = workItemRepository;
        }

        [HttpGet("burndown/teams/{teamId}/iterations/{iterationId}")]
        public async Task<IActionResult> GetBurndownTrendAsync(uint teamId, uint iterationId)
        {
            var trend = await _context.RemainingWorks
            .Where(r => r.TeamId == teamId && r.IterationId == iterationId)
            .ToListAsync();
            return Ok(trend);
        }

        [HttpGet("burndown/teams/{teamId}/iterations/{iterationId}/current")]
        public async Task<IActionResult> GetCurrentBurndownPointsAsync(uint teamId, uint iterationId)
        {
            //TODO: check
            var today = DateTime.Now.Date;
            var normalization = new[]
            {
                new WorkItem { Type = WorkItemType.Story },
                new WorkItem { Type = WorkItemType.Task },
                new WorkItem { Type = WorkItemType.Bug }
            };
            var workItems = await _teamRepository.GetWorkItemsForStatisticsAsync(teamId, iterationId);
            var records = normalization
            .Concat(workItems)
            .GroupBy(wi => wi.Type)
            .Select(g => new RemainingWork
            {
                WorkItemType = g.Key,
                AccountingDate = today,
                EstimatedHours = g.Sum(wi => wi.EstimatedHours ?? 0),
                RemainingHours = g.Sum(wi => wi.RemainingHours ?? 0),
                CompletedHours = g.Sum(wi => wi.CompletedHours ?? 0),
                RemainingCount = g.Count(wi => wi.State != WorkItemState.Closed) - 1,
                CompletedCount = g.Count(wi => wi.State == WorkItemState.Closed)
            })
            .ToList();
            return Ok(records);
        }

        [HttpPost("burndown/teams/{teamId}/iterations/{iterationId}/runjob")]
        public async Task<IActionResult> RunAccountingJobAsync(uint teamId, uint iterationId)
        {
            var theDayBefore = DateTime.Now.Date.AddDays(-1);
            var normalization = new[]
            {
                new WorkItem{Type=WorkItemType.Task},
                new WorkItem{Type=WorkItemType.Bug}
            };
            var workItems = await _teamRepository.GetWorkItemsForStatisticsAsync(teamId, iterationId);
            var remainingWorks = normalization
            .Concat(workItems)
            .GroupBy(wi => wi.Type)
            .Select(g => new RemainingWork
            {
                TeamId = teamId,
                IterationId = iterationId,
                WorkItemType = g.Key,
                AccountingDate = theDayBefore,
                EstimatedHours = g.Sum(wi => wi.EstimatedHours ?? 0),
                RemainingHours = g.Sum(wi => wi.RemainingHours ?? 0),
                CompletedHours = g.Sum(wi => wi.CompletedHours ?? 0),
                RemainingCount = g.Count(wi => wi.State != WorkItemState.Closed) - 1,
                CompletedCount = g.Count(wi => wi.State == WorkItemState.Closed)
            })
            .ToList();
            await _context.RemainingWorks
            .UpsertRange(remainingWorks)
            .On(r => new { r.TeamId, r.IterationId, r.WorkItemType, r.AccountingDate })
            .RunAsync();
            return Ok();
        }

        [HttpGet("workloads/teams/{teamId}/iterations/{iterationId}")]
        public async Task<IActionResult> GetWorkloadsAsync(uint teamId, uint iterationId, int remainingDays)
        {
            var capacities = await _context.MemberCapacities
            .Where(c => c.TeamId == teamId && c.IterationId == iterationId)
            .ToListAsync();
            var teamCapacityPerDay = capacities.Sum(c => c.HoursPerDay);

            var workItems = await _teamRepository.GetWorkItemsForStatisticsAsync(teamId, iterationId);
            var teamAssignedCapacity = workItems.Sum(wi => wi.RemainingHours ?? 0);
            var assignedCapacityMap = workItems
            .GroupBy(wi => wi.AssigneeId)
            .ToDictionary(g => g.Key ?? 0, g => g.Sum(wi => wi.RemainingHours ?? 0));

            var memberWorkloads = Enumerable.Repeat(
                new MemberCapacity
                {
                    MemberId = 0,
                    HoursPerDay = 0f
                }, 1)
                .Concat(capacities)
                .GroupBy(c => c.MemberId)
                .Select(g =>
                {
                    assignedCapacityMap.TryGetValue(g.Key, out float assignedCapacity);
                    return new DTO.Workload
                    {
                        OwnerId = g.Key,
                        Capacity = g.Sum(c => c.HoursPerDay * remainingDays),
                        AssignedCapacity = assignedCapacity
                    };
                })
                .ToList();

            return Ok(new DTO.Workloads
            {
                TeamWorkload = new DTO.Workload
                {
                    OwnerId = teamId,
                    Capacity = teamCapacityPerDay * remainingDays,
                    AssignedCapacity = teamAssignedCapacity
                },
                MemberWorkloads = memberWorkloads
            });
        }

        [HttpGet("codereviews/created/thismonth")]
        public async Task<IActionResult> GetHowManyCodeReviewsCreatedThisMonthAsync()
        {
            var now = DateTime.Now;
            var from = new DateTime(now.Year, now.Month, 1);
            var to = from.AddMonths(1);
            var count = await _workItemRepository.DbSet
                .Where(wi => wi.ProjectId == 68)
                .Where(wi => wi.State != WorkItemState.Removed)
                .Where(wi => wi.CreatedDate >= from && wi.CreatedDate < to)
                .CountAsync();
            return Ok(count);
        }

        [HttpGet("codereviews/solved/thismonth")]
        public async Task<IActionResult> GetHowManyCodeReviewsSolvedThisMonthAsync()
        {
            var now = DateTime.Now;
            var from = new DateTime(now.Year, now.Month, 1);
            var to = from.AddMonths(1);
            var count = await _workItemRepository.DbSet
                .Where(wi => wi.ProjectId == 68)
                .Where(wi => wi.State == WorkItemState.Closed)
                .Where(wi => wi.ChangedDate >= from && wi.CreatedDate < to)
                .CountAsync();
            return Ok(count);
        }

        [HttpGet("codereviews/left")]
        public async Task<IActionResult> GetHowManyCodeReviewsLeftAsync()
        {
            var count = await _workItemRepository.DbSet
                .Where(wi => wi.ProjectId == 68)
                .Where(wi => wi.State != WorkItemState.Closed && wi.State != WorkItemState.Removed)
                .CountAsync();
            return Ok(count);
        }
    }
}