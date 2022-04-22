using System.Collections.Generic;
using System.Threading.Tasks;
using SpaceA.Model;
using GitLab = SpaceA.Model.GitLab;

namespace SpaceA.WebApi.Services.Interfaces
{
    public interface IGitLabService
    {
        Task<List<GitLab.Repo>> GetReposAsync(bool owned, string accessToken);

        Task ConfigMergeRequestRuleAsync(uint repoId, string accessToken);

        Task<List<GitLab.Branch>> GetBranchesAsync(uint repoId, string accessToken);

        Task<GitLab.Branch> CreateBranchAsync(uint repoId, string branch, string @ref, string accessToken);

        Task<GitLab.Hook> AddWebHookAsync(uint repoId, string hookUrl, string accessToken);

        Task<List<GitLab.Hook>> GetAllWebHooksAsync(uint repoId, string accessToken);

        Task<GitLab.Hook> DeleteWebHookAsync(uint repoId, uint hookId, string accessToken);

        Task<GitLab.Hook> UpdateWebHookAsync(uint repoId, uint hookId, string hookUrl, string accessToken);

        Task<List<GitLab.DeployKey>> GetAllDeployKeysAsync(string accessToken);

        Task EnableDeployKeyAsync(uint repoId, uint keyId, string accessToken);

        Task<List<GitLab.ProtectedBranch>> ListProtectedBranchesAsync(uint repoId, string accessToken);

        Task ProtectBranchAsync(
            uint repoId,
            string name,
            AccessLevel push_access_level,
            AccessLevel merge_access_level,
            AccessLevel unprotect_access_level,
            string accessToken);

        Task<GitLab.File> GetFileAsync(uint repoId, string path, string @ref, string accessToken);

        Task AddOneFileAsync(uint repoId, string path, GitLab.File file, string accessToken);

        Task CommitFilesAsync(uint repoId, GitLab.Commit commit, string accessToken);
    }
}