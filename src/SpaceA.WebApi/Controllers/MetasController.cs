using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using SpaceA.Model;
using SpaceA.Model.Entities;
using SpaceA.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DTO = SpaceA.Model.Dto;

namespace SpaceA.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetasController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                Enums = new
                {
                    Severity = GetEnumMap<Severity>(),
                    CapacityType = GetEnumMap<CapacityType>()
                }
            });
        }

        private List<dynamic> GetEnumMap<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
            .Cast<T>()
            .Select(val => (dynamic)new { Value = val, DisplayName = GetDisplayName(val) })
            .ToList();
        }

        private string GetDisplayName<T>(T value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var descAttributes = fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
            return descAttributes?.FirstOrDefault()?.Name ?? value.ToString();
        }
    }
}