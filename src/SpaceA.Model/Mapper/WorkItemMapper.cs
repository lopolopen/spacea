using System;
using System.Collections.Generic;
using System.Linq;
using SpaceA.Model.Entities;
using DTO = SpaceA.Model.Dto;

namespace SpaceA.Model.Mapper
{
    public static class WorkItemMapper
    {
        public static WorkItem ToEntity(this DTO.WorkItem dto)
        {
            return new WorkItem
            {
                ProjectId = dto.ProjectId,
                // Rev = dto.Rev,
                Type = dto.Type,
                Title = dto.Title,
                AssigneeId = dto.AssigneeId,
                FolderId = dto.FolderId,
                IterationId = dto.IterationId,
                Description = dto.Description,
                AcceptCriteria = dto.AcceptCriteria,
                ReproSteps = dto.ReproSteps,
                Priority = dto.Priority,
                State = dto.State,
                Reason = dto.Reason,
                UploadFiles = dto.UploadFiles,
                EstimatedHours = dto.EstimatedHours,
                RemainingHours = dto.RemainingHours,
                CompletedHours = dto.CompletedHours,
                Environment = dto.Environment,
                Severity = dto.Severity,
                ParentId = dto.ParentId,
                CreatedDate = dto.CreatedDate,
                ChangedDate = dto.ChangedDate,
                CreatorId = dto.CreatorId,
                ChangerId = dto.ChangerId
            };
        }

        public static DTO.WorkItem ToDto(this WorkItem entity)
        {
            return new DTO.WorkItem
            {
                Id = entity.Id,
                Rev = entity.Rev,
                Type = entity.Type,
                Title = entity.Title,
                AssigneeId = entity.AssigneeId,
                AssignedTo = entity.AssignedTo?.ToDto(),
                Description = entity.Description,
                AcceptCriteria = entity.AcceptCriteria,
                ReproSteps = entity.ReproSteps,
                Priority = entity.Priority,
                State = entity.State,
                Reason = entity.Reason,
                UploadFiles = entity.UploadFiles,
                EstimatedHours = entity.EstimatedHours,
                RemainingHours = entity.RemainingHours,
                CompletedHours = entity.CompletedHours,
                Environment = entity.Environment,
                Severity = entity.Severity,
                ParentId = entity.ParentId,
                Order = entity.Order,
                ChangerId = entity.ChangerId,
                Changer = entity.Changer?.ToDto(),
                Creator = entity.Creator?.ToDto(),
                CreatedDate = entity.CreatedDate,
                ChangedDate = entity.ChangedDate,
                CreatorId = entity.CreatorId,
                Iteration = entity.Iteration?.ToDto(),
                ProjectId = entity.ProjectId,
                Project = entity.Project?.ToDto(),
                Tags = entity.Tags?.Select(tg =>
                new DTO.Tag
                {
                    Id = tg.Id,
                    Text = tg.Text,
                    Color = tg.Color
                })
                .ToList(),
                Attachments = entity.Attachments?.Select(a =>
                new DTO.Attachment
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    Size = a.Size,
                    UploadedTime = a.UploadedTime,
                    Creator = a.Creator?.ToDto(),
                    Comments = a.Comments
                })
                .ToList(),
                FolderId = entity.FolderId,
                IterationId = entity.IterationId ?? 0
            };
        }
    }
}