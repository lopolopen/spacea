using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceA.Model.Entities
{
    public class WorkItem : IEntity<uint>, IEntityHistory<uint?>
    {
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; }
        [Column(Order = 1)]
        public int Rev { get; set; }
        [Column(Order = 2)]
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

        public string Environment { get; set; }//Bug - 环境
        public Severity? Severity { get; set; }//Bug - 严重程度
        public uint? ParentId { get; set; }
        [StringLength(512)]
        public string Order { get; set; }
        public DateTime CreatedDate { get; set; }
        public uint CreatorId { get; set; }
        public uint ProjectId { get; set; }
        public DateTime ChangedDate { get; set; }
        public uint? ChangerId { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }
        public virtual Folder Folder { get; set; }
        public virtual Iteration Iteration {get;set;}
        public virtual Member AssignedTo { get; set; }
        public virtual Member Changer { get; set; }
        public virtual Member Creator { get; set; }
        public virtual ICollection<WorkItem> Children { get; set; }
        public virtual WorkItem Parent { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }
        public virtual ICollection<Attachment> Attachments { get; set; }
        [NotMapped]
        public bool IsOrphan { get; set; }
    }
}