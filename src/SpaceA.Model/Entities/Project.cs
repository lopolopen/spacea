using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceA.Model.Entities
{
    public class Project : IEntity<uint>, IEntityHistory<uint?>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; }

        public int Rev { get; set; }

        public uint OwnerId { get; set; }

        [Required]
        [StringLength(32)]
        public string Name { get; set; }

        [StringLength(256)]
        public string Description { get; set; }

        [StringLength(32)]
        public string AvatarUid { get; set; }

        public DateTime CreatedDate { get; set; }

        public uint? DefaultTeamId { get; set; }

        public uint? RootFolderId { get; set; }

        public uint? RootIterationId { get; set; }

        public Guid DeletedFlag { get; set; }

        public DateTime ChangedDate { get; set; }

        public uint? ChangerId { get; set; }

        [NotMapped]
        public bool IsDeleted => DeletedFlag != Guid.Empty;

        public virtual Team DefaultTeam { get; set; }

        public virtual Folder RootFolder { get; set; }

        public virtual Iteration RootIteration { get; set; }

        public virtual Member Owner { get; set; }

        public virtual ICollection<WorkItem> WorkItems { get; set; }

        public virtual ICollection<Team> Teams { get; set; }

        public virtual ICollection<Folder> Folders { get; set; }

        public virtual ICollection<Iteration> Iterations { get; set; }

        public virtual ICollection<Tag> Tags { get; set; }

        public virtual ICollection<ProjectRepo> ProjectRepos { get; set; }
    }
}