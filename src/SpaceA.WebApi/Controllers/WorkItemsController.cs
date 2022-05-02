using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpaceA.Repository.Context;
using SpaceA.Model.Entities;
using SpaceA.Model.Mapper;
using SpaceA.Repository.Interfaces;
using SpaceA.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using DTO = SpaceA.Model.Dto;

namespace SpaceA.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkItemsController : ControllerBase
    {
        private readonly SpaceAContext _context;
        private readonly IWorkItemRepository _workItemRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly ITagRepository _tagRepository;
        private readonly Lazy<Member> _me;

        public WorkItemsController(
            SpaceAContext context,
            ITokenService tokenService,
            IWorkItemRepository workItemRepository,
            IMemberRepository memberRepository,
            ITagRepository tagRepository
            )
        {
            _context = context;
            _workItemRepository = workItemRepository;
            _memberRepository = memberRepository;
            _tagRepository = tagRepository;
            _me = new Lazy<Member>(() => tokenService.GetMember(User));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkItemAsync(uint id)
        {
            var workItem = await _workItemRepository.GetWorkItemAsync(id);
            if (workItem == null)
            {
                return NoContent();
            }
            return Ok(workItem.ToDto());
        }

        [HttpGet("{id}/details")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkItemDetailAsync(uint id)
        {
            var detail = await _workItemRepository.GetWorkItemDetailAsync(id);
            if (detail == null)
            {
                return NoContent();
            }
            return Ok(detail.ToDto());
        }

        [HttpPut("{id}/parents/{parentId?}")]
        public async Task<IActionResult> SetAsChild(uint id, uint? parentId = null)
        {
            if (parentId == 0) parentId = null;
            _workItemRepository.UpdateParentId(id, parentId);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkItemAsync(uint id, DTO.WorkItem itemDto)
        {
            bool existing = await _workItemRepository.ExistsAsync(id);
            if (!existing)
            {
                return NoContent();
            }
            var now = DateTime.Now;
            var member = _me.Value;
            await using (var trans = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _workItemRepository.ArchiveAsync<WorkItemHistory>(id);
                    var changedNum = await _workItemRepository.DbSet
                        .Where(wi => wi.Id == id && wi.Rev == itemDto.Rev)
                        .UpdateFromQueryAsync(
                            wi => new WorkItem
                            {
                                Rev = wi.Rev + 1,
                                Title = itemDto.Title,
                                AssigneeId = itemDto.AssigneeId,
                                FolderId = itemDto.FolderId,
                                IterationId = itemDto.IterationId,
                                Description = itemDto.Description,
                                AcceptCriteria = itemDto.AcceptCriteria,
                                ReproSteps = itemDto.ReproSteps,
                                Priority = itemDto.Priority,
                                State = itemDto.State,
                                Reason = itemDto.Reason,
                                UploadFiles = itemDto.UploadFiles,
                                EstimatedHours = itemDto.EstimatedHours,
                                RemainingHours = itemDto.RemainingHours,
                                CompletedHours = itemDto.CompletedHours,
                                Environment = itemDto.Environment,
                                Severity = itemDto.Severity,
                                ChangedDate = now,
                                ChangerId = member.Id,
                            });
                    if (changedNum == 0)
                    {
                        await trans.RollbackAsync();
                        return StatusCode(409, new DTO.Error("工作事项已被其他人改动，请刷新后重试。"));
                    }

                    //TODO:
                    // await _tagRepository.DbSet
                    // .Where(tg => tg.WorkItemId == id)
                    // .DeleteFromQueryAsync();
                    // var tags = itemDTO.Tags
                    // ?.Select(tgDTO =>
                    // new Tag
                    // {
                    //     Text = tgvm.Text,
                    //     Color = tgvm.Color,
                    //     WorkItemId = id
                    // })
                    // ?.ToList()
                    // ?? new List<Tag>(0);
                    // await _tagRepository.AddRangeAsync(tags, true);
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

        [HttpPatch("{id}")]
        public async Task<IActionResult> PartialUpdateWorkItemAsync(uint id, DTO.WorkItem itemDto)
        {
            bool existing = await _workItemRepository.ExistsAsync(id);
            if (!existing)
            {
                return NoContent();
            }
            var member = _me.Value;
            await using (var trans = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _workItemRepository.ArchiveAsync<WorkItemHistory>(id);
                    var changedNum = await _workItemRepository.DbSet
                        .Where(wi => wi.Id == id && wi.Rev == itemDto.Rev)
                        .UpdateFromQueryAsync(
                            wi => new WorkItem
                            {
                                Rev = wi.Rev + 1,
                                ChangedDate = DateTime.Now,
                                ChangerId = member.Id,
                            });
                    if (changedNum == 0)
                    {
                        await trans.RollbackAsync();
                        return StatusCode(409, new DTO.Error("工作事项已被其他人改动，请刷新后重试。"));
                    }

                    var workItem = itemDto.ToEntity();
                    workItem.Id = id;
                    _workItemRepository.PartialUpdate(workItem, key => itemDto.Allow(key));
                    await _context.SaveChangesAsync();
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveAsync(uint id)
        {
            _workItemRepository.Remove(id);
            await _context.SaveChangesAsync();
            return Ok();
        }


        [HttpGet("{id}/summary")]
        public async Task<IActionResult> GetWorkItemSummaryAsync(uint id)
        {
            var workItem = await _workItemRepository.GetWorkItemSummaryAsync(id);
            var itemDto = workItem.ToDto();
            return Ok(itemDto);
        }

        [HttpGet("{id}/childrensummary")]
        public async Task<IActionResult> GetChildrenSummaryAsync(uint id)
        {
            var children = await _workItemRepository.GetChildrenSummaryAsync(id);
            var itemDtos = children
            .Select(c => c.ToDto())
            .ToList();
            return Ok(itemDtos);
        }

        [HttpGet("prodbugs")]
        public async Task<IActionResult> GetProdBugsAsync(
            DateTime? from,
            DateTime? to,
            bool showClosedItem)
        {
            if (from == null && to != null) return BadRequest();
            if (from != null && to == null) return BadRequest();
            var bugs = await _workItemRepository.GetProdBugsAsync(from, to, showClosedItem);
            var bugDtos = bugs.Select(bug => bug.ToDto()).ToList();
            return Ok(bugDtos);
        }

        [HttpGet("codereviews")]
        public async Task<IActionResult> GetCodeReviewAsync(
                    DateTime? from,
                    DateTime? to,
                    bool showClosedItem)
        {
            if (from == null && to != null) return BadRequest();
            if (from != null && to == null) return BadRequest();
            var crs = await _workItemRepository.GetCodeReviewsAsync(from, to, showClosedItem);
            var crDtos = crs.Select(cr => cr.ToDto()).ToList();
            return Ok(crDtos);
        }

        [HttpPost("{id}/tags")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateTagsAsync(uint id, DTO.Tag[] tagDTOs)
        {
            try
            {
                //TODO:
                var tags = new List<Tag>();
                foreach (var item in tagDTOs)
                {
                    tags.Add(
                        new Tag
                        {
                            WorkItemId = id,
                            Text = item.Text,
                            Color = item.Color
                        }
                    );
                };
                _tagRepository.AddRange(tags);
                await _context.SaveChangesAsync();
                return Created(new Uri($"/api/workitems/{id}/tags", UriKind.Relative), tagDTOs);
            }
            catch (DbUpdateException)
            {
                return Conflict();
            }
        }

        [HttpGet("{id}/tags")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTagsAsync(uint id)
        {
            bool existing = await _workItemRepository.ExistsAsync(id);
            if (!existing)
            {
                return NotFound();
            }
            var tags = await _workItemRepository.GetTagsAsync(id);
            var tagDTOs = tags.Select(tg =>
            new DTO.Tag
            {
                Id = tg.Id,
                Text = tg.Text,
                Color = tg.Color
            })
            .ToList();
            return Ok(tagDTOs);
        }

        [HttpGet("{id}/histories")]
        public async Task<IActionResult> GetWorkItemHistories(uint id)
        {
            var latest = await _workItemRepository.GetAsync(id);
            var histories = await _workItemRepository.GetHistoriesAsync(id);
            //TODO:
            histories.Add(new WorkItemHistory
            {
                Id = latest.Id,
                Rev = latest.Rev,
                Order = latest.Order,
                ProjectId = latest.ProjectId,
                Type = latest.Type,
                Title = latest.Title,
                AssigneeId = latest.AssigneeId,
                FolderId = latest.FolderId,
                IterationId = latest.IterationId,
                Description = latest.Description,
                AcceptCriteria = latest.AcceptCriteria,
                ReproSteps = latest.ReproSteps,
                Priority = latest.Priority,
                State = latest.State,
                Reason = latest.Reason,
                UploadFiles = latest.UploadFiles,
                EstimatedHours = latest.EstimatedHours,
                RemainingHours = latest.RemainingHours,
                CompletedHours = latest.CompletedHours,
                Environment = latest.Environment,
                Severity = latest.Severity,
                ParentId = latest.ParentId,
                CreatedDate = latest.CreatedDate,
                ChangedDate = latest.ChangedDate,
                CreatorId = latest.CreatorId,
                ChangerId = latest.ChangerId
            });
            return Ok(histories);
        }
    }
}