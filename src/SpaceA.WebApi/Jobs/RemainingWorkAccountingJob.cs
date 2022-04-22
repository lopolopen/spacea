using System;
using System.Linq;
using System.Threading.Tasks;
using SpaceA.Repository.Context;
using SpaceA.Model;
using SpaceA.Model.Entities;
using SpaceA.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace SpaceA.WebApi.Jobs
{

    [DisallowConcurrentExecution]
    public class RemainingWorkAccountingJob : IJob
    {
        private readonly IServiceProvider _provider;

        public RemainingWorkAccountingJob(IServiceProvider provider)
        {
            _provider = provider;
        }


        public async Task Execute(IJobExecutionContext context)
        {
            var theDayBefore = DateTime.Now.Date.AddDays(-1);
            using (var scope = _provider.CreateScope())
            {
                var copContext = scope.ServiceProvider.GetService<SpaceAContext>();
                var productRepository = scope.ServiceProvider.GetService<IProjectRepository>();
                var teamRepository = scope.ServiceProvider.GetService<ITeamRepository>();
                var projects = await productRepository.GetAllAsync();
                var pids = projects.Select(p => p.Id).ToList();
                var normalization = new[]
                {
                    // new WorkItem { Type = WorkItemType.Story },
                    new WorkItem { Type = WorkItemType.Task },
                    new WorkItem { Type = WorkItemType.Bug }
                };
                foreach (var pid in pids)
                {
                    var teamIterations = await productRepository.GetActiveTeamIterationsAsync(pid);
                    foreach (var ti in teamIterations)
                    {
                        var workItems = await teamRepository.GetWorkItemsForStatisticsAsync(ti.Id1, ti.Id2);
                        var remainingWorks = normalization
                        .Concat(workItems)
                        .GroupBy(wi => wi.Type)
                        .Select(g => new RemainingWork
                        {
                            TeamId = ti.Id1,
                            IterationId = ti.Id2,
                            WorkItemType = g.Key,
                            AccountingDate = theDayBefore,
                            EstimatedHours = g.Sum(wi => wi.EstimatedHours ?? 0),
                            RemainingHours = g.Sum(wi => wi.RemainingHours ?? 0),
                            CompletedHours = g.Sum(wi => wi.CompletedHours ?? 0),
                            RemainingCount = g.Count(wi => wi.State != WorkItemState.Closed) - 1,
                            CompletedCount = g.Count(wi => wi.State == WorkItemState.Closed)
                        })
                        .ToList();
                        await copContext.RemainingWorks
                        .UpsertRange(remainingWorks)
                        .On(r => new { r.TeamId, r.IterationId, r.WorkItemType, r.AccountingDate })
                        .RunAsync();
                    }
                }
            }
        }
    }
}