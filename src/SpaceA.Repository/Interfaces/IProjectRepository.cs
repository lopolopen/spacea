using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpaceA.Model.Entities;

namespace SpaceA.Repository.Interfaces
{
    public interface IProjectRepository : IRepository<Project, uint>
    {
        Task<List<WorkItem>> GetWorkItems(uint id);

        Task<Project> GetProjectDetail(uint id);

        void AddDefaultTeam(Project project, Team defaultTeam);

        Task<List<Project>> GetProjectDetails();

        void LinkReposToProject(Project project, params Repo[] repos);

        Task<List<Repo>> GetRepos(uint id);

        Task<Team> GetDefaultTeamAsync(uint id);

        Task<List<Team>> GetTeamsAsync(uint id);

        void UpdateDefaultTeamId(uint id, uint defaultTeamId);

        void MarkAsDeleted(uint id);

        void UpdateProject(Project project);

        void RemoveRepo(uint id, uint repoId);

        Task<List<Folder>> GetFoldersAsync(uint id);

        Task<List<Config>> GetAccessTokensAsync(uint id);

        Task<List<TeamIteration>> GetActiveTeamIterationsAsync(uint projectId);
    }
}