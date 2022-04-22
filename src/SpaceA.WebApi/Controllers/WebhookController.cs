using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpaceA.WebApi.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;
using SpaceA.Model.Webhook;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SpaceA.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IHubContext<WebhookHub> _hubContext;

        public WebhookController(IHubContext<WebhookHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost("onhook")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        //public async Task<IActionResult> OnHook(Dictionary<string,dynamic> request)
        public async Task<IActionResult> OnHookAsync(WebhookRequest request)
        {
            try
            {
                switch (request.ObjectKind)
                {
                    case "push":
                        await OnPushAsync(
                            new
                            {
                                Id = request.Project.Id,
                                UserName = request.UserName
                            });
                        break;
                    case "merge_request":
                        var branch = request.ObjectAttributes.SourceBranch;
                        var match = Regex.Match(branch, @"#(?<id>\d+)");
                        if (match.Success)
                        {
                            int id = int.Parse(match.Groups["id"].Value);
                            string state = request.ObjectAttributes.State;
                            if (state == "merged")
                            {
                                await OnMergeRequestMergedAsync(
                                    new
                                    {
                                        Id = request.Project.Id,
                                        UserName = request.User.UserName,
                                        Assignees = request.Assignees.UserName,
                                        WorkItemId = id
                                    });
                            }
                            else if (state == "opened")
                            {
                                await OnMergeRequestOpenedAsync(
                                    new
                                    {
                                        Id = request.Project.Id,
                                        UserName = request.User.UserName,
                                        Assignees = request.Assignees.UserName,
                                        Url = request.ObjectAttributes.Url
                                    }
                                );
                            }
                        }
                        break;
                }
                return Ok();
            }
            catch (Exception err)
            {
                return BadRequest(err);
            }
        }

        private async Task OnPushAsync(object args = null)
        {
            await _hubContext.Clients.Client(GetConnectionId(args)).SendAsync("on-push");
            //await _hubContext.Clients.All.SendAsync("on-push", args);
        }

        private async Task OnMergeRequestOpenedAsync(object args = null)
        {
            await _hubContext.Clients.Client(GetConnectionId(args)).SendAsync("on-mr-opened");
            //await _hubContext.Clients.All.SendAsync("on-mr-opened", args);
        }

        private async Task OnMergeRequestMergedAsync(object args = null)
        {
            await _hubContext.Clients.Client(GetConnectionId(args)).SendAsync("on-mr-merged");
            //await _hubContext.Clients.All.SendAsync("on-mr-merged", args);
        }

        private string GetConnectionId(object args)
        {
            var jsonList = JsonConvert.SerializeObject(args);
            var userName = JObject.Parse(jsonList)["UserName"].ToString();
            var user = WebhookHub.Users.FirstOrDefault(p => p.Name == userName);
            return user.ConnectionID;
        }
    }
}