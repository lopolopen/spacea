using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace SpaceA.Model.Entities
{
    [SuppressMessage("ReSharper", "All")]
    public class WorkItemHistory : IEntity<uint>, IEntityHistory<uint?>
    {
        public uint Id { get; set; }

        public int Rev { get; set; }

        [Required]
        public WorkItemType Type { get; set; }

        [Required]
        [StringLength(128)]
        public string Title { get; set; }

        public uint? AssigneeId { get; set; }

        public string Desc { get; set; }

        public string AcceptCriteria { get; set; }

        public string ReproSteps { get; set; }

        [Required]
        public WorkItemPriority Priority { get; set; }

        [Required]
        public WorkItemState State { get; set; }

        [StringLength(128)]
        public string Reason { get; set; }

        [Required]
        public uint FolderId { get; set; }

        public uint? IterationId { get; set; }

        public string UploadFiles { get; set; }

        public float? EstimatedTime { get; set; }

        public float? EstimatedHours { get; set; }

        public float? RemainingHours { get; set; }

        public float? CompletedHours { get; set; }

        public string Environment { get; set; }

        public Severity? Severity { get; set; }//Bug - 严重程度

        public uint? ParentId { get; set; }

        [StringLength(512)]
        public string Order { get; set; }

        public DateTime CreatedDate { get; set; }

        public uint CreatorId { get; set; }

        public uint ProjectId { get; set; }

        public DateTime ChangedDate { get; set; }

        public uint? ChangerId { get; set; }
    }
}