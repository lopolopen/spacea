using System;
using System.Collections.Generic;
using System.Linq;
using SpaceA.Model.Entities;
using DTO = SpaceA.Model.Dto;

namespace SpaceA.Model.Mapper
{
    public static class MemberMapper
    {
        public static DTO.Member ToDto(this Member entity)
        {
            return new DTO.Member
            {
                Id = entity.Id,
                AccountName = entity.AccountName,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                Xing = entity.Xing,
                Ming = entity.Ming,
                AvatarUid = entity.AvatarUid,
                Disabled = entity.Disabled
            };
        }

        public static Member ToEntity(this DTO.Member dto)
        {
            return new Member
            {
                Id = dto.Id,
                AccountName = dto.AccountName,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Xing = dto.Xing,
                Ming = dto.Ming,
                AvatarUid = dto.AvatarUid,
                Disabled = dto.Disabled
            };
        }
    }
}