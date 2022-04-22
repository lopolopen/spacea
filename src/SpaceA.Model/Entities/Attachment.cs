using System;
using System.ComponentModel.DataAnnotations;

namespace SpaceA.Model.Entities
{
    public class Attachment : IEntity<Guid>
    {
        public Guid Id { get; set; }
        [Required]
        [StringLength(256)]
        public string FileName { get; set; }
        public long Size { get; set; }
        public DateTime? UploadedTime { get; set; }
        public uint? WorkItemId { get; set; }
        public uint? CreatorId { get; set; }
        public string Comments { get; set; }

        public virtual WorkItem AttachedTo { get; set; }
        public virtual Member Creator { get; set; }
    }
}