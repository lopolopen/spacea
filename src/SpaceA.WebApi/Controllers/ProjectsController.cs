using SpaceA.Model;
using SpaceA.Model.Entities;
using SpaceA.Model.Mapper;
using SpaceA.Repository.Interfaces;
using SpaceA.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTO = SpaceA.Model.Dto;
using GitLab = SpaceA.Model.GitLab;

namespace SpaceA.WebApi.Controllers
{
    using SpaceA.Repository.Context;
    using SpaceA.Common;
    using SpaceA.WebApi.Options;
    using SpaceA.WebApi.Services;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Minio;
    using Minio.Exceptions;

    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private IConfiguration Configuration { get; }
        private readonly SpaceAContext _context;
        private readonly ILogger _logger;
        private readonly IProjectRepository _projectRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IWorkItemRepository _workItemRepository;
        private readonly IFolderRepository _folderRepository;
        private readonly IIterationRepository _iterationRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IRepoRepository _repoRepository;
        private readonly IOrderService _orderService;
        private readonly ICipherService _cipherService;
        private readonly Lazy<Member> _me;
        private readonly MinioOptions _minioOptions;

        public ProjectsController(
            IConfiguration configuration,
            ILogger<ProjectsController> logger,
            ITokenService tokenService,
            SpaceAContext context,
            IProjectRepository projectRepository,
            ITeamRepository teamRepository,
            IWorkItemRepository workItemRepository,
            IMemberRepository memberRepository,
            IFolderRepository folderRepository,
            IIterationRepository iterationRepository,
            IGroupRepository groupRepository,
            IRepoRepository repoRepository,
            IOrderService orderService,
            ICipherService cipherService,
            IOptions<MinioOptions> minioOptions)
        {
            Configuration = configuration;
            _context = context;
            _logger = logger;
            _projectRepository = projectRepository;
            _teamRepository = teamRepository;
            _workItemRepository = workItemRepository;
            _folderRepository = folderRepository;
            _iterationRepository = iterationRepository;
            _memberRepository = memberRepository;
            _groupRepository = groupRepository;
            _repoRepository = repoRepository;
            _orderService = orderService;
            _cipherService = cipherService;
            _me = new Lazy<Member>(() => tokenService.GetMember(User));
            _minioOptions = minioOptions.Value;
        }

        [HttpGet("names")]
        public async Task<IActionResult> GetProjectNamesAsync([FromQuery] uint[] ids)
        {
            if (ids == null) return BadRequest();
            if (ids.Length == 0) return Ok(new object[] { });
            var names = await _projectRepository.DbSet
            .Where(p => ids.Contains(p.Id))
            .Select(p => new { Id = p.Id, Name = p.Name })
            .ToListAsync();
            return Ok(names);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateProjectAsync(DTO.Project projectDTO)
        {
            var member = _me.Value;
            var now = DateTime.Now;
            var project = new Project
            {
                OwnerId = member.Id,
                Name = projectDTO.Name,
                Desc = projectDTO.Desc,
                AvatarUid = projectDTO.AvatarUid,
                CreatedDate = now,
                ChangedDate = now,
                ChangerId = member.Id,
            };
            using (var trans = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _projectRepository.Add(project);
                    await _context.SaveChangesAsync();
                    var folder = new Folder
                    {
                        ProjectId = project.Id,
                        Name = project.Name,
                        Path = $"/{project.Name}",
                    };
                    _folderRepository.Add(folder);
                    await _context.SaveChangesAsync();
                    project.RootFolderId = folder.Id;
                    var iteration = new Iteration
                    {
                        ProjectId = project.Id,
                        Name = project.Name,
                        Path = $"/{project.Name}"
                    };
                    var subIterations = new List<Iteration>{
                        new Iteration
                        {
                            ProjectId = project.Id,
                            Name = "迭代1",
                            Path = $"/{project.Name}/迭代1"
                        },
                        new Iteration
                        {
                            ProjectId = project.Id,
                            Name = "迭代2",
                            Path = $"/{project.Name}/迭代2"
                        },
                        new Iteration
                        {
                            ProjectId = project.Id,
                            Name = "迭代3",
                            Path = $"/{project.Name}/迭代3"
                        }
                    };
                    _iterationRepository.Add(iteration);
                    await _context.SaveChangesAsync();
                    project.RootIterationId = iteration.Id;
                    _iterationRepository.AddRange(subIterations);
                    var team = new Team
                    {
                        ProjectId = project.Id,
                        Name = project.Name,
                        Desc = $"项目{project.Name}的默认团队",
                        DefaultFolderId = folder.Id,
                        DefaultIterationId = iteration.Id
                    };
                    _teamRepository.Add(team);
                    _teamRepository.AddIterationsToTeam(team, subIterations.ToArray());
                    _teamRepository.AddFolderToTeam(team, folder);
                    var me = await _memberRepository.GetAsync(_me.Value.Id);
                    _teamRepository.AddMemberToTeam(team, me);
                    _projectRepository.AddDefaultTeam(project, team);
                    await _context.SaveChangesAsync();
                    await trans.CommitAsync();
                    projectDTO.Teams = new List<DTO.Team>
                    {
                        new DTO.Team
                        {
                            Id = team.Id,
                            Name = team.Name,
                            Desc = team.Desc
                        }
                    };
                    projectDTO.Owner = me.ToDto();
                    projectDTO.Members = new List<DTO.Member> { projectDTO.Owner };
                    projectDTO.Id = project.Id;
                    projectDTO.CreatedDate = now;
                }
                catch (DbUpdateException)
                {
                    await trans.RollbackAsync();
                    return Conflict();
                }
            }
            return Created(new Uri($"/api/projects/{project.Id}", UriKind.Relative), projectDTO);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjectsAsync()
        {
            var projects = await _projectRepository.DbSet
            .Include(p => p.Owner)
            .Include(p => p.Teams)
            .ThenInclude(t => t.TeamMembers)
            .ThenInclude(tm => tm.Member)
            .Where(p => p.DeletedFlag == Guid.Empty)
            .ToListAsync();
            var projectDTOs = projects
            .Select(p => new DTO.Project
            {
                Id = p.Id,
                Name = p.Name,
                Desc = p.Desc,
                AvatarUid = p.AvatarUid,
                CreatedDate = p.CreatedDate,
                Owner = p.Owner.ToDto(),
                Members = p.Teams
                .SelectMany(t => t.TeamMembers)
                .Select(tm => tm.Member.ToDto())
                .Distinct(Comparer<DTO.Member>.Use(m => m.Id))
                .ToList(),
                DefaultTeamId = p.DefaultTeamId
            })
            .ToList();
            return Ok(projectDTOs);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DTO.Project), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProjectAsync(uint id)
        {
            bool existing = await _projectRepository.ExistsAsync(id);
            if (!existing)
            {
                return NotFound();
            }
            var project = await _projectRepository.GetProjectDetail(id);
            var teamIds = project.Teams.Select(t => t.Id).ToArray();
            var memberMap = await _teamRepository.GetMemberMap(teamIds);
            var allFolders = await _folderRepository.GetFoldersAsync(id);
            var folderMap = await _folderRepository.GetFolderMapAsync(teamIds);
            var allIterations = await _iterationRepository.GetIterationsAsync(id);
            var iterationIdMap = await _iterationRepository.GetIterationIdMapAsync(teamIds);
            var projectDTO = new DTO.Project
            {
                Id = project.Id,
                Name = project.Name,
                Desc = project.Desc,
                AvatarUid = project.AvatarUid,
                CreatedDate = project.CreatedDate,
                Owner = project.Owner.ToDto(),
                DefaultTeamId = project.DefaultTeamId,
                RootFolderId = project.RootFolderId.Value,
                RootIterationId = project.RootIterationId.Value,
                Teams = project.Teams.Select(t =>
                {
                    memberMap.TryGetValue(t.Id, out var members);
                    folderMap.TryGetValue(t.Id, out var folders);
                    iterationIdMap.TryGetValue(t.Id, out var iterationIds);
                    // iterationMap.TryGetValue(t.Id, out var iterations);
                    return new DTO.Team
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Acronym = t.Acronym,
                        Desc = t.Desc,
                        DefaultFolder = t.DefaultFolder?.ToDto(),
                        Folders = folders?.Select(f =>
                        new DTO.Folder
                        {
                            Id = f.Id,
                            Name = f.Name,
                            Path = f.Path
                        })
                        .ToList(),
                        DefaultIteration = t.DefaultIteration?.ToDto(),
                        IterationIds = iterationIds,
                        Members = members?.Select(m => m.ToDto())
                        .ToList() ?? new List<DTO.Member>()
                    };
                })
                .ToList(),
                Folders = allFolders.Select(f =>
                new DTO.Folder
                {
                    Id = f.Id,
                    Name = f.Name,
                    Path = f.Path
                })
                .ToList(),
                Iterations = allIterations?.Select(i => i.ToDto())
                .ToList() ?? new List<DTO.Iteration>()
            };
            return Ok(projectDTO);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveAsync(uint id)
        {
            bool existing = await _projectRepository.ExistsAsync(id);
            if (!existing)
            {
                return NoContent();
            }
            _projectRepository.MarkAsDeleted(id);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateProjectAsync(uint id, DTO.Project projectDTO)
        {
            bool existing = await _projectRepository.ExistsAsync(id);
            if (!existing)
            {
                return NoContent();
            }
            var now = DateTime.Now;
            var member = _me.Value;
            //?
            await using (var trans = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _projectRepository.DbSet
                        .Where(p => p.Id == id)
                        .UpdateFromQueryAsync(p =>
                            new Project
                            {
                                Rev = p.Rev + 1,
                                Name = projectDTO.Name,
                                Desc = projectDTO.Desc,
                                ChangedDate = now,
                                ChangerId = member.Id,
                            }
                        );
                    await trans.CommitAsync();
                }
                catch (Exception)
                {
                    await trans.RollbackAsync();
                    throw;
                }
            }
            return Ok();
        }

        [HttpPost("{id}/avatar")]
        public async Task<IActionResult> UploadAvatarAsync(uint id, IFormFile file, string uid)
        {
            if (file == null) return BadRequest();
            bool existing = await _projectRepository.ExistsAsync(id);
            if (!existing)
            {
                return NoContent();
            }
            uid = uid ?? Guid.NewGuid().ToString().Replace("-", "").ToLower();
            await using (var trans = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var project = new Project
                    {
                        Id = id,
                        AvatarUid = uid
                    };
                    _context.Entry(project)
                        .Property(p => p.AvatarUid)
                        .IsModified = true;
                    await _context.SaveChangesAsync();
                    var opt = _minioOptions;
                    var bucket = opt.AccessKey;
                    var filePath = $"public/projects/{id}/avatar";
                    var minio = new MinioClient(opt.Endpoint, opt.AccessKey, opt.SecretKey);
                    using (var stream = file.OpenReadStream())
                    {
                        await minio.PutObjectAsync(bucket, filePath, stream, file.Length, file.ContentType);
                    }
                    await trans.CommitAsync();
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex.Message);
                    throw ex;
                }
            }
            var projectDto = new DTO.Project()
            {
                Id = id,
                AvatarUid = uid
            };
            return Ok(projectDto.AvatarUrl);
        }

        [HttpDelete("{id}/avatar")]
        public async Task<IActionResult> RemoveAvatarAsync(uint id)
        {
            bool existing = await _projectRepository.ExistsAsync(id);
            if (!existing)
            {
                return NoContent();
            }
            var project = new Project
            {
                Id = id,
                AvatarUid = null
            };
            _context.Entry(project)
                .Property(p => p.AvatarUid)
                .IsModified = true;
            await _context.SaveChangesAsync();
            var opt = _minioOptions;
            var bucket = opt.AccessKey;
            var filePath = $"public/projects/{id}/avatar";
            var minio = new MinioClient(opt.Endpoint, opt.AccessKey, opt.SecretKey);
            try
            {
                await minio.RemoveObjectAsync(bucket, filePath);
            }
            catch (MinioException ex)
            {
                _logger.LogError(ex.Message);
            }
            return Ok();
        }

        [HttpGet("{id}/workitems/{workItemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkItemAsync(uint id, uint workItemId)
        {
            bool existing = await _projectRepository.ExistsAsync(id);
            if (!existing)
            {
                return NoContent();
            }
            var workItem = await _workItemRepository.GetWorkItemAsync(workItemId);
            if (workItem == null)
            {
                return NoContent();
            }
            return Ok(workItem.ToDto());
        }

        [HttpGet("{id}/teams/{teamId}/workitems")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkItemsAsync(
            uint id,
            uint teamId,
            [FromQuery] uint? iterationId,
            bool showClosedItem,
            bool showClosedChild,
            bool showInProgressItem)
        {
            // 隐藏业务逻辑：当showInProgressItem为false，showClosedItem必为false
            if (!showInProgressItem && showClosedItem)
            {
                return BadRequest();
            }
            //TODO: 性能需要优化
            bool existing = await _projectRepository.ExistsAsync(id);
            if (!existing)
            {
                return NotFound();
            }
            bool teamExisting = await _teamRepository.ExistsAsync(teamId);
            if (!teamExisting)
            {
                return NotFound();
            }

            var workItems = await _teamRepository.GetWorkItemsAsync(
                teamId,
                iterationId,
                showClosedItem,
                showClosedChild,
                showInProgressItem);
            workItems = FilterOutOrphans(workItems);
            var allItemIds = workItems.Select(wi => wi.Id).ToArray();
            // var tagsMap = await _workItemRepository.GetTagsAsync(allItemIds);
            var workItemDTOs = workItems
            .Select(wi =>
            {
                // tagsMap.TryGetValue(wi.Id, out var tags);
                // wi.Tags = tags;
                return wi.ToDto();
            })
            .ToList();

            return Ok(workItemDTOs);
        }

        private bool Filter(WorkItem wi)
        {
            if (wi.State == WorkItemState.Removed) return false;
            return wi.State != WorkItemState.Closed || wi.ChangedDate > DateTime.Now.AddDays(-7);
        }

        private List<WorkItem> FilterOutOrphans(IEnumerable<WorkItem> workItems)
        {
            var map = workItems.ToDictionary(wi => wi.Id);
            foreach (var wi in workItems)
            {
                FlagOrphan(wi, map);
            }
            return workItems.Where(wi => !wi.IsOrphan).ToList();
        }

        //ParentId为null的节点为根父节点
        //在全集范围内，不存在可到达根父节点的路径的所有子节点为孤儿节点
        //有Ref的节点例外
        private bool FlagOrphan(WorkItem workItem, Dictionary<uint, WorkItem> map)
        {
            if (workItem.ParentId == null) return false;
            if (!map.ContainsKey(workItem.ParentId.Value))
            {
                workItem.IsOrphan = true;
                return true;
            }
            var parent = map[workItem.ParentId.Value];
            workItem.IsOrphan = FlagOrphan(parent, map);
            return workItem.IsOrphan;
        }

        private IEnumerable<WorkItem> TempFilter(IEnumerable<WorkItem> items)
        {
            var xs = items.GroupBy(i => i.ParentId).ToDictionary(g => g.Key ?? 0, g => g.ToList());
            return items.Where(i =>
            {
                if (Filter(i)) return true;
                if (!xs.ContainsKey(i.Id)) return false;
                var children = xs[i.Id];
                return children.Any(c => Filter(c));
            });
        }

        [HttpPost("{id}/folders/{folderId}/workitems")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateWorkItemAsync(uint id, uint folderId, DTO.WorkItem workItemDTO, Location location)
        {
            //TODO:
            var assigneeId = workItemDTO.AssigneeId;
            var assigneeTo = assigneeId == null ? null : await _memberRepository.GetAsync(assigneeId.Value);
            var changerId = _me.Value.Id;
            var changer = await _memberRepository.GetAsync(changerId);
            var folder = await _folderRepository.GetAsync(folderId);
            var now = DateTime.Now;
            string order;
            switch (location)
            {
                case Location.Bottom:
                    order = await _orderService.GetBottomOrderAsync(id, workItemDTO.ParentId);
                    break;
                case Location.Top:
                default:
                    order = await _orderService.GetTopOrderAsync(id, workItemDTO.ParentId);
                    break;
            }
            var workItem = workItemDTO.ToEntity();
            if (workItem.Type == WorkItemType.TestSuite)
            {
                bool existing = await _workItemRepository.ExistsTestSuiteAsync(workItem.ParentId);
                if (existing)
                {
                    return Conflict();
                }
            }
            workItem.ProjectId = id;
            workItem.CreatedDate = now;
            workItem.ChangedDate = now;
            workItem.Order = order;
            workItem.FolderId = folderId;
            workItem.ChangedDate = now;
            workItem.ChangerId = changerId;
            try
            {
                _workItemRepository.Add(workItem);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict();
            }
            workItemDTO.Id = workItem.Id;
            workItemDTO.CreatedDate = workItem.CreatedDate;
            workItemDTO.ChangedDate = workItem.ChangedDate;
            workItemDTO.ChangerId = workItem.ChangerId;
            workItemDTO.Order = order;
            workItemDTO.AssignedTo = assigneeTo?.ToDto();
            workItemDTO.Changer = changer.ToDto();
            return Created(new Uri($"/api/workitems/{workItem.Id}", UriKind.Relative), workItemDTO);
        }

        [HttpGet("{id}/repos")]
        public async Task<IActionResult> GetRepos(uint id)
        {
            bool existing = await _projectRepository.ExistsAsync(id);
            if (!existing)
            {
                return NotFound();
            }
            var repos = await _projectRepository.GetRepos(id);
            var repoDTOs = repos.Select(r => new GitLab.Repo
            {
                Id = r.Id,
                Name = r.Name,
                NameWithNamespace = r.NameWithNamespace,
                CreatedAt = r.CreatedAt,
                WebUrl = r.WebUrl,
                HttpUrlToRepo = r.HttpUrlToRepo,
                SshUrlToRepo = r.SshUrlToRepo,
            })
            .ToList();
            return Ok(repoDTOs);
        }

        [HttpPost("{id}/repos/bulk")]
        public async Task<IActionResult> LinkRepos(uint id, List<GitLab.Repo> repoDTOs)
        {
            var repos = repoDTOs.Select(r =>
            new Repo
            {
                Id = r.Id,
                Name = r.Name,
                NameWithNamespace = r.NameWithNamespace,
                CreatedAt = r.CreatedAt,
                WebUrl = r.WebUrl,
                HttpUrlToRepo = r.HttpUrlToRepo,
                SshUrlToRepo = r.SshUrlToRepo,
                NamespaceName = r.Namespace.Name,
                NamespacePath = r.Namespace.Path,
                NamespaceFullPath = r.Namespace.FullPath,
                NamespaceKind = r.Namespace.Kind,
            })
            .ToArray();
            using (var trans = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _repoRepository.DbSet
                    .UpsertRange(repos)
                    .On(r => new { r.Id })
                    .NoUpdate()
                    .RunAsync();
                    var project = new Project { Id = id };
                    _projectRepository.LinkReposToProject(project, repos);
                    await _context.SaveChangesAsync();
                    await trans.CommitAsync();
                }
                catch (DbUpdateException)
                {
                    await trans.RollbackAsync();
                    return StatusCode(409, new DTO.Error("仓库不能在同一项目中被重复关联"));
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    throw ex;
                }
            }
            return Ok(repoDTOs);
        }

        [HttpDelete("{id}/repos/{repoId}")]
        public async Task<IActionResult> DeleteRepo(uint id, uint repoId)
        {
            _projectRepository.RemoveRepo(id, repoId);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NoContent();
            }
            return Ok();
        }

        [HttpPost("{id}/teams")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateTeamAsync(uint id, DTO.Team teamDTO)
        {
            var project = await _projectRepository.GetAsync(id);
            if (project == null) return NoContent();
            var rootFolder = await _folderRepository.GetAsync(project.RootFolderId.Value);
            using (var trans = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var folder = new Folder
                    {
                        ProjectId = id,
                        Name = teamDTO.Name,
                        Path = $"{rootFolder.Path}/{teamDTO.Name}",
                    };
                    _folderRepository.Add(folder);
                    await _context.SaveChangesAsync();
                    var team = new Team
                    {
                        ProjectId = id,
                        Name = teamDTO.Name,
                        Acronym = teamDTO.Acronym,
                        Desc = teamDTO.Desc,
                        DefaultFolderId = folder.Id,
                        DefaultIterationId = project.RootIterationId,
                    };
                    _teamRepository.Add(team);
                    await _context.SaveChangesAsync();
                    teamDTO.Id = team.Id;
                    _teamRepository.AddFolderToTeam(team, folder);
                    var members = teamDTO.MemberIds?.Select(mid => new Member { Id = mid }).ToArray();
                    _teamRepository.AddMembersToTeam(team, members);
                    await _context.SaveChangesAsync();
                    await trans.CommitAsync();
                    teamDTO.DefaultFolder = folder.ToDto();
                }
                catch (DbUpdateException)
                {
                    await trans.RollbackAsync();
                    return Conflict();
                }
            }

            //TODO: uri
            return Created(new Uri($"", UriKind.Relative), teamDTO);
        }

        // [HttpPost("{id}/teams/bulk")]
        // [ProducesResponseType(StatusCodes.Status201Created)]
        // public async Task<IActionResult> CreateTeamsAsync(uint id, List<DTO.Team> teamDTOs)
        // {
        //     var project = await _projectRepository.GetAsync(id);
        //     if (project == null) return NoContent();
        //     using (var trans = await _projectRepository.BeginTransactionAsync())
        //     {
        //         try
        //         {
        //             foreach (var tvm in teamDTOs)
        //             {
        //                 var folder = new Folder
        //                 {
        //                     ProjectId = id,
        //                     Name = tvm.DefaultFolder.Name,
        //                     //TODO:
        //                     Path = $"/{project.Name}/{tvm.DefaultFolder.Name}",
        //                 };
        //                 await _folderRepository.AddAsync(folder, true);
        //                 tvm.DefaultFolder.Id = folder.Id;
        //                 tvm.DefaultFolder.Path = folder.Path;
        //                 var team = new Team
        //                 {
        //                     ProjectId = id,
        //                     Name = tvm.Name,
        //                     Acronym = tvm.Acronym,
        //                     Desc = tvm.Desc,
        //                     DefaultFolderId = folder.Id,
        //                     DefaultIterationId = project.RootIterationId,
        //                 };
        //                 await _teamRepository.AddAsync(team, true);
        //                 tvm.Id = team.Id;
        //                 _teamRepository.AddFolderToTeam(team, folder);
        //                 if (tvm.GroupId != null)
        //                 {
        //                     var group = new Group { Id = tvm.GroupId.Value };
        //                     _teamRepository.AddGroupToTeam(team, group);
        //                 }
        //                 else
        //                 {
        //                     //目前Free模式不会批量创建团队
        //                     throw new Exception("Don't use this action in Free mode.");
        //                 }
        //             }
        //             await _projectRepository.SaveAsync();
        //             await trans.CommitAsync();
        //         }
        //         catch (DbUpdateException)
        //         {
        //             await trans.RollbackAsync();
        //             return Conflict();
        //         }
        //     }
        //     //TODO: uri
        //     return Created(new Uri($"", UriKind.Relative), teamDTOs);
        // }

        [HttpPut("{id}/teams/{defaultTeamId}")]
        public async Task<IActionResult> UpdateDefaultTeamId(uint id, uint defaultTeamId)
        {
            _projectRepository.UpdateDefaultTeamId(id, defaultTeamId);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPatch("{id}/workitems/{workItemId}/reorder")]
        public async Task<IActionResult> ReOrderAsync(uint id, uint workItemId, DTO.ReOrder reOrder)
        {
            bool existing = await _workItemRepository.DbSet
            .AnyAsync(wi => wi.Id == workItemId && wi.ProjectId == id);
            if (!existing)
            {
                return NoContent();
            }
            var parentId = reOrder.ParentId;
            string order = null;
            if (reOrder.PreviousId != null)
            {
                // location === 'top'
                var previous = await _orderService.GetOrder(parentId, reOrder.PreviousId.Value);
                if (previous == null)
                {
                    return BadRequest();
                }
                var minGuard = await _orderService.GetMinNextOrderAsync(id, parentId, previous);
                order = OrderService.Increase(previous, minGuard);
            }
            else if (reOrder.NextId != null)
            {
                // location === 'bottom'
                var next = await _orderService.GetOrder(parentId, reOrder.NextId.Value);
                if (next == null)
                {
                    return BadRequest();
                }
                var maxGuard = await _orderService.GetMaxPreviousOrderAsync(id, parentId, next);
                order = OrderService.Decrease(next, maxGuard);
            }
            else
            {
                // location === 'center'
                order = await _orderService.GetTopOrderAsync(id, parentId);
            }
            await _workItemRepository.DbSet
            .Where(wi => wi.Id == workItemId)
            .UpdateFromQueryAsync(
                wi => new WorkItem
                {
                    ParentId = parentId,
                    Order = order
                });
            return Ok(order);
        }

        [HttpPost("{id}/folders")]
        public async Task<IActionResult> CreateFolderAsync(uint id, DTO.Folder folderDto)
        {
            if (string.IsNullOrEmpty(folderDto.Name)
            || folderDto.Name.Contains('/')
            || folderDto.Name.Contains('\\'))
            {
                return BadRequest();
            }
            var folder = folderDto.ToEntity();
            folder.ProjectId = id;
            try
            {
                _folderRepository.Add(folder);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict();
            }

            return Ok(folder.ToDto());
        }

        [HttpPost("{id}/iterations")]
        public async Task<IActionResult> CreateIterationAsync(uint id, DTO.Iteration iterationDto)
        {
            if (string.IsNullOrEmpty(iterationDto.Name)
            || iterationDto.Name.Contains('/')
            || iterationDto.Name.Contains('\\'))
            {
                return BadRequest();
            }
            var iteration = iterationDto.ToEntity();
            iteration.ProjectId = id;
            try
            {
                _iterationRepository.Add(iteration);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict();
            }

            return Ok(iteration.ToDto());
        }

        [HttpPost("{id}/teams/{teamId}/folders")]
        public async Task<IActionResult> CreateFolderForTeamAsync(uint id, uint teamId, DTO.Folder folderDto)
        {
            if (string.IsNullOrEmpty(folderDto.Name)
            || folderDto.Name.Contains('/')
            || folderDto.Name.Contains('\\'))
            {
                return BadRequest();
            }
            var folder = folderDto.ToEntity();
            folder.ProjectId = id;
            var team = new Team { Id = teamId };
            _folderRepository.Add(folder);
            _teamRepository.AddFolderToTeam(team, folder);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict();
            }
            folderDto.Id = folder.Id;
            return Created(new Uri($"", UriKind.Relative), folderDto);
        }

        [HttpPost("{id}/teams/{teamId}/iterations")]
        public async Task<IActionResult> CreateIterationForTeamAsync(uint id, uint teamId, DTO.Iteration iterationDto)
        {
            if (string.IsNullOrEmpty(iterationDto.Name)
            || iterationDto.Name.Contains('/')
            || iterationDto.Name.Contains('\\'))
            {
                return BadRequest();
            }
            var iteration = iterationDto.ToEntity();
            iteration.ProjectId = id;
            var team = new Team { Id = teamId };
            _iterationRepository.Add(iteration);
            _teamRepository.AddIterationToTeam(team, iteration);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict();
            }
            iterationDto.Id = iteration.Id;
            return Created(new Uri($"", UriKind.Relative), iterationDto);
        }

        [HttpGet("{id}/accesstokens")]
        public async Task<IActionResult> GetAccessTokensAsync(uint id)
        {
            bool existing = await _projectRepository.ExistsAsync(id);
            if (!existing)
            {
                return NotFound();
            }
            var configs = await _projectRepository.GetAccessTokensAsync(id);
            var tokens = configs.Select(c => new DTO.GitLabAccessToken
            {
                OwnerId = c.Id1,
                DesensitizedValue = Desensitize(c.Value),
                CipherValue = _cipherService.Encrypt(c.Value),
                BelongTo = c.Member.ToDto(),
                IsShared = c.IsShared
            });
            return Ok(tokens);
        }

        private string Desensitize(string token)
        {
            if (string.IsNullOrEmpty(token)) return token;
            if (token.Length < 8)
            {
                int half = token.Length >> 1;
                return $"{token.Substring(0, half)}{new string('*', token.Length - half)}";
            }
            return $"{token.Substring(0, 4)}{new string('*', token.Length - 8)}{token.Substring(token.Length - 4, 4)}";
        }
    }
}