using System;
using System.Collections.Generic;
using System.Linq;
using SpaceA.Model.Entities;
using DTO = SpaceA.Model.Dto;

namespace SpaceA.Model.Mapper
{
    public static class FolderMapper
    {
        public static DTO.Folder ToDto(this Folder entity)
        {
            return new DTO.Folder
            {
                Id = entity.Id,
                Name = entity.Name,
                Path = entity.Path
            };
        }

        public static Folder ToEntity(this DTO.Folder dto)
        {
            return new Folder
            {
                Id = dto.Id,
                Name = dto.Name,
                Path = dto.Path
            };
        }
    }
}