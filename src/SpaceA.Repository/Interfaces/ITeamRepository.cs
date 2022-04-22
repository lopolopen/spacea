using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpaceA.Model.Entities;

namespace SpaceA.Repository.Interfaces
{

    public interface ITeamRepository : IRepository<Team, uint>
    {
        Task<List<WorkItem>> GetWorkItemsAsync(uint id, uint? iterationId, bool showClosedItem, bool showClosedChild, bool showInProgressItem);

        Task<List<Member>> GetMembersAsync(uint id);

        Task<Dictionary<uint, List<Member>>> GetMemberMap(params uint[] ids);

        void AddMemberToTeam(Team team, Member member);

        void AddMembersToTeam(Team team, params Member[] members);


        void AddFolderToTeam(Team team, Folder folder);

        Task AddFolderToTeamAsync(uint id, uint folderId);

        void AddFolderToTeamAsync(Team team, Folder folder);

        Task RemoveFolderFromTeamAsync(uint id, uint folderIdGuid);

        void UpdateTeam(Team team);

        void RemoveAllMembers(Team team);

        Task AddIterationToTeamAsync(uint id, uint iterationId);

        Task RemoveIterationFromTeamAsync(uint id, uint iterationId);

        void AddIterationToTeam(Team team, Iteration iteration);

        void AddIterationsToTeam(Team team, params Iteration[] iterations);

        void UpdateDefaultIterationOfTeamAsync(uint id, uint iterationId);

        Task RemoveNotDescendantsAsync(uint id, uint ancestorIterationId);

        void UpdateDefaultFolderOfTeamAsync(uint id, uint defFolderId);
        void UpdateDefaultFolderOfTeamAsync(Team team);
        Task<List<WorkItem>> GetWorkItemsForStatisticsAsync(uint id, uint iterationId);
    }
}