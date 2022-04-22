using System;
using System.Threading.Tasks;
using System.Linq;
using SpaceA.Model.Entities;
using Microsoft.AspNetCore.Mvc;
using SpaceA.Repository.Interfaces;
using System.Collections.Generic;
using SpaceA.WebApi.Services.Interfaces;

namespace SpaceA.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodosController : ControllerBase
    {
        private readonly IWorkItemRepository _workItemRepository;
        private readonly Lazy<Member> _me;

        public TodosController(
            ITokenService tokenService,
            IWorkItemRepository workItemRepository)
        {
            _workItemRepository = workItemRepository;
            _me = new Lazy<Member>(() => tokenService.GetMember(User));
        }

        [HttpGet("workitems/inprogress/tome")]
        public async Task<IActionResult> GetInProgressWorkItemsToMe()
        {
            return await GetInProgressWorkItems(AboutMe.ToMe);
        }

        [HttpGet("workitems/inprogress/byme")]
        public async Task<IActionResult> GetInProgressWorkItemsByMe()
        {
            return await GetInProgressWorkItems(AboutMe.ByMe);
        }

        private async Task<IActionResult> GetInProgressWorkItems(AboutMe abountMe)
        {
            if (_me.Value == null)
            {
                return Forbid();
            }
            IList<WorkItem> wis;
            if (abountMe == AboutMe.ToMe)
            {
                wis = await _workItemRepository.GetInProgressWorkItemsToMe(_me.Value.Id);
            }
            else if (abountMe == AboutMe.ByMe)
            {
                wis = await _workItemRepository.GetInProgressWorkItemsByMe(_me.Value.Id);
            }
            else
            {
                wis = new List<WorkItem>();
            }
            // var allItemIds = wis.Select(wi => wi.Id).ToArray();
            // var tagsMap = await _workItemRepository.GetTagsAsync(allItemIds);
            var xs = wis.Select(wi =>
            {
                // tagsMap.TryGetValue(wi.Id, out var tags);
                return new
                {
                    Id = wi.Id,
                    Title = wi.Title,
                    Type = wi.Type,
                    State = wi.State,
                    Priority = wi.Priority,
                    ChangedDate = wi.ChangedDate,
                    Project = new
                    {
                        Id = wi.Project.Id,
                        Name = wi.Project.Name
                    },
                    // Tags = tags?.Select(tg =>
                    // new DTO.Tag
                    // {
                    //     Id = tg.Id,
                    //     Text = tg.Text,
                    //     Color = tg.Color
                    // })
                    // .ToList()
                };
            })
            .ToList();
            return Ok(xs);
        }
    }

    enum AboutMe
    {
        ToMe,
        ByMe
    }
}