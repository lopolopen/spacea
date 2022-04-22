using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpaceA.Model.Entities;
using SpaceA.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DTO = SpaceA.Model.Dto;

namespace SpaceA.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        ITagRepository _repository;

        public TagsController(ITagRepository repository)
        {
            _repository = repository;
        }

        // [HttpPost]
        // public async Task<IActionResult> CreateTagAsync(DTO.Tag[] tagDTO)
        // {
        //     var tags = new List<Tag>();
        //     foreach (var item in tagDTO)
        //     {
        //         tags.Add(
        //             new Tag
        //             {
        //                 ProjectId = item.ProjectId,
        //                 Text = item.Text,
        //                 Color = item.Color
        //             });
        //     };
        //     await _repository.AddRangeAsync(tags, true);
        //     return Ok();
        // }

    }
}