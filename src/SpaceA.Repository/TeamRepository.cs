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
    using SpaceA.Model;
    using SpaceA.Common;

    public class TeamRepository : RepositoryBase<Team, uint>, ITeamRepository
    {
        public TeamRepository(SpaceAContext context) :
            base(context, ctx => ctx.Teams)
        {
        }

        public async Task<List<WorkItem>> GetWorkItemsForStatisticsAsync(uint id, uint iterationId)
        {
            var workItems = await
            (
                from t in _entitySet
                join tf in _context.TeamFolders
                on t.Id equals tf.Id1
                join f in _context.Folders
                on tf.Id2 equals f.Id
                join wi in _context.WorkItems
                on f.Id equals wi.FolderId
                where t.Id == id
                where wi.IterationId == iterationId
                where wi.Type == WorkItemType.Story || wi.Type == WorkItemType.Task || wi.Type == WorkItemType.Bug
                where wi.State != WorkItemState.Removed
                select new WorkItem
                {
                    Id = wi.Id,
                    Type = wi.Type,
                    State = wi.State,
                    AssigneeId = wi.AssigneeId,
                    EstimatedHours = wi.EstimatedHours,
                    RemainingHours = wi.RemainingHours,
                    CompletedHours = wi.CompletedHours
                }
            )
            .ToListAsync();
            return workItems;
        }

        public async Task<List<WorkItem>> GetWorkItemsAsync(uint id, uint? iterationId, bool showClosedItem, bool showClosedChild, bool showInProgressItem)
        {
            var workItems = await
            (
                from t in _entitySet
                join tf in _context.TeamFolders
                on t.Id equals tf.Id1
                join f in _context.Folders
                on tf.Id2 equals f.Id
                join wi in _context.WorkItems
                on f.Id equals wi.FolderId
                join m1 in _context.Members
                on wi.ChangerId equals m1.Id
                join m2 in _context.Members
                on wi.CreatorId equals m2.Id
                where t.Id == id
                where iterationId == null || wi.IterationId == iterationId
                where wi.State != WorkItemState.Removed
                where (showClosedItem && showClosedChild && showInProgressItem)

                    // || (showClosedItem && showClosedChild && (...))
                    || (showClosedChild && showInProgressItem && (wi.ParentId != null || wi.State != WorkItemState.Closed))
                    || (showClosedItem && showInProgressItem && (wi.ParentId == null || wi.State != WorkItemState.Closed))

                    // || (!showClosedChild && !showInProgressItem && (...))
                    || (showClosedChild && !showClosedItem && !showInProgressItem && (wi.ParentId != null || wi.State == WorkItemState.New))
                    || (showInProgressItem && !showClosedItem && !showClosedChild && (wi.State != WorkItemState.Closed))

                    || (wi.State == WorkItemState.New)
                select new WorkItem
                {
                    Id = wi.Id,
                    Rev = wi.Rev,
                    ProjectId = wi.ProjectId,
                    Title = wi.Title,
                    Type = wi.Type,
                    State = wi.State,
                    Reason = wi.Reason,
                    Priority = wi.Priority,
                    AssigneeId = wi.AssigneeId,
                    CreatedDate = wi.CreatedDate,
                    ChangedDate = wi.ChangedDate,
                    Environment = wi.Environment,
                    Severity = wi.Severity,
                    EstimatedHours = wi.EstimatedHours,
                    RemainingHours = wi.RemainingHours,
                    CompletedHours = wi.CompletedHours,
                    IterationId = wi.IterationId,
                    Order = wi.Order,
                    ParentId = wi.ParentId,
                    Changer = m1,
                    Creator = m2,
                    Folder = f,
                    FolderId = wi.FolderId
                }
            )
            .ToListAsync();
            return workItems;
        }

        public async Task<List<Member>> GetMembersAsync(uint id)
        {
            var members = await
            (
                from t in _entitySet
                join tm in _context.TeamMembers on t.Id equals tm.Id1
                join m in _context.Members on tm.Id2 equals m.Id into ms
                from m in ms.DefaultIfEmpty()
                where t.Id == id
                select m
            )
            .ToListAsync();
            return members;
        }

        public async Task<Dictionary<uint, List<Member>>> GetMemberMap(params uint[] ids)
        {
            var xs1 = await
            (
               from t in _entitySet
               join tm in _context.TeamMembers on t.Id equals tm.Id1
               join m in _context.Members on tm.Id2 equals m.Id into ms
               from m in ms.DefaultIfEmpty()
               where ids.Contains(t.Id)
               select new { t.Id, Member = m }
            )
            .ToListAsync();
            return xs1.Where(x => x.Member != null)
            .GroupBy(x => x.Id)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.Member).Distinct(Comparer<Member>.Use(m => m.Id)).ToList()
            );
        }

        public void RemoveAllMembers(Team team)
        {
            var teamMembers = _context.TeamMembers.Where(tp => tp.Id1 == team.Id).ToArray();
            _context.TeamMembers.RemoveRange(teamMembers);
        }

        public void AddMemberToTeam(Team team, Member member)
        {
            if (team == null || member == null) return;
            var teamMember = new TeamMember
            {
                Team = team,
                Member = member
            };
            TryAttachRange(team, member);
            _context.TeamMembers.Add(teamMember);
        }

        public void AddMembersToTeam(Team team, params Member[] members)
        {
            if (team == null || members == null) return;
            var teamMembers = members.Select(m => new TeamMember
            {
                Team = team,
                Member = m
            });
            TryAttach(team);
            TryAttachRange(members);
            _context.TeamMembers.AddRange(teamMembers);
        }

        public void AddFolderToTeam(Team team, Folder folder)
        {
            TryAttachRange(team, folder);
            var teamFolder = new TeamFolder
            {
                Team = team,
                Folder = folder
            };
            _context.TeamFolders.Add(teamFolder);
        }

        public async Task AddFolderToTeamAsync(uint id, uint folderId)
        {
            var record = await _context.TeamFolders
            .Where(ti => ti.Id1 == id && ti.Id2 == folderId)
            .FirstOrDefaultAsync();
            if (record != null) return;
            record = new TeamFolder
            {
                Team = new Team { Id = id },
                Folder = new Folder { Id = folderId }
            };
            TryAttach(record);
        }

        public void AddFolderToTeamAsync(Team team, Folder folder)
        {
            // var record = await _context.TeamFolders
            // .Where(ti => ti.Id1 == team.Id && ti.Id2 == folder.Id)
            // .FirstOrDefaultAsync();
            // if (record != null) return;
            var record = new TeamFolder
            {
                Team = team,
                Folder = folder
            };
            TryAttach(record);
        }

        public async Task RemoveFolderFromTeamAsync(uint id, uint folderId)
        {
            var record = await _context.TeamFolders
            .Where(ti => ti.Id1 == id && ti.Id2 == folderId)
            .FirstOrDefaultAsync();
            if (record == null) return;
            _context.TeamFolders.Remove(record);
        }

        public void UpdateTeam(Team team)
        {
            TryAttach(team);
            _context.Entry(team)
            .Property(t => t.Name)
            .IsModified = true;
            _context.Entry(team)
            .Property(t => t.Acronym)
            .IsModified = true;
            _context.Entry(team)
            .Property(t => t.Desc)
            .IsModified = true;
        }

        public void AddIterationToTeam(Team team, Iteration iteration)
        {
            TryAttachRange(team, iteration);
            var teamIteration = new TeamIteration
            {
                Team = team,
                Iteration = iteration
            };
            _context.TeamIterations.Add(teamIteration);
        }

        public async Task AddIterationToTeamAsync(uint id, uint iterationId)
        {
            var record = await _context.TeamIterations
            .Where(ti => ti.Id1 == id && ti.Id2 == iterationId)
            .FirstOrDefaultAsync();
            if (record != null) return;
            record = new TeamIteration
            {
                Team = new Team { Id = id },
                Iteration = new Iteration { Id = iterationId }
            };
            TryAttach(record);
        }

        public void AddIterationsToTeam(Team team, params Iteration[] iterations)
        {
            TryAttach(team);
            TryAttachRange(iterations);
            var teamIterations = iterations
            .Select(i => new TeamIteration
            {
                Team = team,
                Iteration = i
            })
            .ToArray();
            _context.TeamIterations.AddRange(teamIterations);
        }

        public async Task RemoveIterationFromTeamAsync(uint id, uint iterationId)
        {
            var record = await _context.TeamIterations
            .Where(ti => ti.Id1 == id && ti.Id2 == iterationId)
            .FirstOrDefaultAsync();
            if (record == null) return;
            _context.TeamIterations.Remove(record);
        }

        public void UpdateDefaultIterationOfTeamAsync(uint id, uint defIterationId)
        {
            var team = new Team
            {
                Id = id,
                DefaultIterationId = defIterationId
            };
            TryAttach(team);
            _context.Entry(team)
            .Property(t => t.DefaultIterationId)
            .IsModified = true;
        }

        public void UpdateDefaultFolderOfTeamAsync(uint id, uint defFolderId)
        {
            var team = new Team
            {
                Id = id,
                DefaultFolderId = defFolderId
            };
            TryAttach(team);
            _context.Entry(team)
            .Property(t => t.DefaultFolderId)
            .IsModified = true;
        }

        public void UpdateDefaultFolderOfTeamAsync(Team team)
        {
            TryAttach(team);
            _context.Entry(team)
            .Property(t => t.DefaultFolderId)
            .IsModified = true;
        }

        public async Task RemoveNotDescendantsAsync(uint id, uint ancestorIterationId)
        {
            var ancestorPath = await _context.Iterations
            .Where(i => i.Id == ancestorIterationId)
            .Select(i => i.Path).FirstOrDefaultAsync();
            var tis = await
            (
                from ti in _context.TeamIterations
                join i in _context.Iterations on ti.Id2 equals i.Id
                where ti.Id1 == id && !i.Path.StartsWith(ancestorPath)
                select ti
            )
            .ToArrayAsync();
            _context.TeamIterations.RemoveRange(tis);
        }
    }
}