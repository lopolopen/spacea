using System;
using System.Collections.Generic;
using System.Linq;
using SpaceA.Model.Entities;
using DTO = SpaceA.Model.Dto;

namespace SpaceA.Model.Mapper
{
    public static class ProjectMapper
    {
        public static DTO.Project ToDto(this Project entity)
        {
            return new DTO.Project
            {
                Id = entity.Id,
                Name = entity.Name,
                Owner = entity.Owner?.ToDto()
            };
        }
    }
}