using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaceA.Client.Try.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TryController : ControllerBase
    {
        private readonly IProjectClient _projectClient;
        public TryController(IProjectClient projectClient)
        {
            _projectClient = projectClient;
        }

        [HttpGet("try1")]
        public async Task<IActionResult> Try1(uint id)
        {
            try
            {
                var project = await _projectClient.GetProjectAsync(id);
                return Ok(project);
            }
            catch (ApiException ex)
            {
                return Ok(ex.StatusCode);
            }
        }
    }
}
