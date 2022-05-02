using System;
using System.Collections.Generic;

namespace SpaceA.Model.Dto
{
    public class WorkItem : WithMask<WorkItem>
    {
        public uint? Id { get; set; }
        public int Rev { get; set; }
        public WorkItemType Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string AcceptCriteria { get; set; }
        public string ReproSteps { get; set; }
        public WorkItemPriority Priority { get; set; }
        public WorkItemState State { get; set; }
        public string Reason { get; set; }
        public uint FolderId { get; set; }
        public uint IterationId { get; set; }
        public uint ProjectId { get; set; }
        public uint? AssigneeId { get; set; }
        public string UploadFiles { get; set; }
        public float? EstimatedHours { get; set; }
        public float? RemainingHours { get; set; }
        public float? CompletedHours { get; set; }
        public string Environment { get; set; }
        public Severity? Severity { get; set; }
        public uint? ParentId { get; set; }
        public string Order { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ChangedDate { get; set; }
        public uint CreatorId { get; set; }
        public uint? ChangerId { get; set; }
        public Member AssignedTo { get; set; }
        public Member Creator { get; set; }
        public Member Changer { get; set; }
        public Project Project { get; set; }
        public Iteration Iteration { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Attachment> Attachments { get; set; }
    }
}