
using SpaceA.Repository;
using SpaceA.Repository.Interfaces;
using SpaceA.WebApi.Services;
using SpaceA.WebApi.Services.Interfaces;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExts
    {
        public static void AddCustomServices(this IServiceCollection services)
        {
            services.AddScoped<IProjectRepository, ProjectRepository>()
                .AddScoped<IFolderRepository, FolderRepository>()
                .AddScoped<IIterationRepository, IterationRepository>()
                .AddScoped<IGroupRepository, GroupRepository>()
                .AddScoped<ITeamRepository, TeamRepository>()
                .AddScoped<IWorkItemRepository, WorkItemRepository>()
                .AddScoped<ITagRepository, TagRepository>()
                .AddScoped<IAttachmentRepository, AttachmentRepository>()
                .AddScoped<IMemberRepository, MemberRepository>()
                .AddScoped<IRepoRepository, RepoRepository>()
                .AddScoped<IOrderService, OrderService>()
                .AddSingleton<ITokenService, TokenService>()
                .AddSingleton<ICipherService, CipherService>()
                .AddSingleton<ILdapService, LdapService>();
        }
    }
}