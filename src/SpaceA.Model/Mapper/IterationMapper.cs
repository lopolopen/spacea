using System;
using System.Collections.Generic;
using System.Linq;
using SpaceA.Model.Entities;
using DTO = SpaceA.Model.Dto;

namespace SpaceA.Model.Mapper
{
    public static class IterationMapper
    {
        public static DTO.Iteration ToDto(this Iteration entity)
        {
            return new DTO.Iteration
            {
                Id = entity.Id,
                Name = entity.Name,
                Path = entity.Path,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                ProjectId = entity.ProjectId
            };
        }

        public static Iteration ToEntity(this DTO.Iteration dto)
        {
            return new Iteration
            {
                Id = dto.Id,
                Name = dto.Name,
                Path = dto.Path,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ProjectId = dto.ProjectId
            };
        }
    }
}