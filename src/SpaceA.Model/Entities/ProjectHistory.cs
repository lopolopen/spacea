using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace SpaceA.Model.Entities
{
    [SuppressMessage("ReSharper", "All")]
    public class ProjectHistory : IEntity<uint>, IEntityHistory<uint?>
    {
        public uint Id { get; set; }

        public int Rev { get; set; }

        public uint OwnerId { get; set; }

        [Required]
        [StringLength(32)]
        public string Name { get; set; }

        [StringLength(256)]
        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public uint? DefaultTeamId { get; set; }

        public uint? RootFolderId { get; set; }

        public Guid? RootIterationId { get; set; }

        public Guid DeletedFlag { get; set; }

        public DateTime ChangedDate { get; set; }

        public uint? ChangerId { get; set; }

        [NotMapped]
        public bool IsDeleted => DeletedFlag != Guid.Empty;
    }
}