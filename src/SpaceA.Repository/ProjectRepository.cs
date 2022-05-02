using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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

    public class ProjectRepository : RepositoryBase<Project, uint>, IProjectRepository
    {
        public ProjectRepository(SpaceAContext context) :
            base(context, ctx => ctx.Projects)
        {
        }

        public override async Task<IList<Project>> GetAllAsync()
        {
            return await _entitySet
            .Where(p => p.DeletedFlag == Guid.Empty)
            .ToListAsync();
        }

        public override async Task<bool> ExistsAsync(uint id)
        {
            return await _entitySet
                .AnyAsync(p => p.Id.Equals(id) && p.DeletedFlag == Guid.Empty);
        }

        public async Task<Project> GetProjectDetail(uint id)
        {
            return await _entitySet
            .Include(p => p.Teams)
            .ThenInclude(t => t.DefaultFolder)
            .Include(p => p.Teams)
            .ThenInclude(t => t.DefaultIteration)
            .Include(p => p.Owner)
            .Where(p => p.Id == id)
            .FirstOrDefaultAsync();
        }

        public async Task<List<Project>> GetProjectDetails()
        {
            return await _entitySet
            .Include(p => p.Teams)
            .ToListAsync();
        }

        public async Task<List<WorkItem>> GetWorkItems(uint id)
        {
            return await this._entitySet
            .Include(p => p.WorkItems)
            .Where(p => p.Id == id)
            .SelectMany(p => p.WorkItems)
            .ToListAsync();
        }

        public void AddDefaultTeam(Project project, Team defaultTeam)
        {
            project.DefaultTeam = defaultTeam;
        }

        public void LinkReposToProject(Project project, params Repo[] repos)
        {
            TryAttach(project);
            TryAttachRange(repos);
            var projectRepos = repos.Select(r =>
            new ProjectRepo
            {
                Project = project,
                Repo = r
            })
            .ToList();
            _context.ProjectRepos.AddRange(projectRepos);
        }

        public async Task<List<Repo>> GetRepos(uint id)
        {
            var xs = await
            (
                from p in _entitySet
                join pr in _context.ProjectRepos on p.Id equals pr.Id1
                join r in _context.Repos on pr.Id2 equals r.Id
                where p.Id == id
                select r
            )
            .ToListAsync();
            return xs;
        }

        public async Task<Team> GetDefaultTeamAsync(uint id)
        {
            return await _entitySet.Include(p => p.DefaultTeam)
            .Where(p => p.Id == id)
            .Select(p => p.DefaultTeam)
            .FirstOrDefaultAsync();
        }

        public async Task<List<Team>> GetTeamsAsync(uint id)
        {
            return await _entitySet.Include(p => p.Teams)
            .Where(p => p.Id == id)
            .SelectMany(p => p.Teams)
            .ToListAsync();
        }

        public void MarkAsDeleted(uint id)
        {
            var project = new Project
            {
                Id = id,
                DeletedFlag = Guid.NewGuid()
            };
            _context.Attach(project);
            _context.Entry(project)
            .Property(p => p.DeletedFlag)
            .IsModified = true;
        }

        public void UpdateProject(Project project)
        {
            _context.Attach(project);
            _context.Entry(project).Property(p => p.Name).IsModified = true;
            _context.Entry(project).Property(p => p.Description).IsModified = true;
            _context.Entry(project).Property(p => p.AvatarUid).IsModified = true;
        }

        public void UpdateDefaultTeamId(uint id, uint defaultTeamId)
        {
            var project = new Project
            {
                Id = id,
                DefaultTeamId = defaultTeamId
            };
            _context.Attach(project);
            _context.Entry(project)
            .Property(p => p.DefaultTeamId)
            .IsModified = true;
        }

        public void RemoveRepo(uint id, uint repoId)
        {
            var project = new Project { Id = id };
            var projectRepo = new ProjectRepo { Id1 = id, Id2 = repoId };
            project.ProjectRepos = new List<ProjectRepo> { projectRepo };
            _context.Projects.Attach(project);
            project.ProjectRepos.Remove(projectRepo);
        }

        public async Task<List<Folder>> GetFoldersAsync(uint id)
        {
            return await _entitySet.Include(p => p.Folders)
            .Where(p => p.Id == id)
            .SelectMany(p => p.Folders)
            .ToListAsync();
        }

        public async Task<List<Config>> GetAccessTokensAsync(uint id)
        {
            var cs = await
            (
                from p in _entitySet
                join t in _context.Teams on p.Id equals t.ProjectId
                join tm in _context.TeamMembers on t.Id equals tm.Id1
                join m in _context.Members on tm.Id2 equals m.Id
                join c in _context.Configs on m.Id equals c.Id1
                where p.Id == id
                where c.Id2 == ConfigKeys.ACCESS_TOKEN
                select new Config
                {
                    Id1 = c.Id1,
                    Value = c.Value,
                    Member = m,
                    IsShared = c.IsShared
                }
            )
            .ToListAsync();
            return cs.Distinct(Comparer<Config>.Use(c => c.Id1)).ToList();
        }

        public async Task<List<TeamIteration>> GetActiveTeamIterationsAsync(uint projectId)
        {
            var today = DateTime.Now.Date;
            var theDayBeforce = today.AddDays(-1);
            var teamIterations = await
            (
                from ti in _context.TeamIterations
                join t in _context.Teams on ti.Id1 equals t.Id
                join i in _context.Iterations on ti.Id2 equals i.Id
                where t.ProjectId == projectId
                where i.StartDate != null && i.EndDate != null
                where i.StartDate <= today && i.EndDate >= theDayBeforce
                select ti
            )
            .ToListAsync();
            return teamIterations;
        }
    }
}