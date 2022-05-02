using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceA.Model.Entities
{
    public class Team : IEntity<uint>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; }
        [Required]
        [StringLength(32)]
        public string Name { get; set; }
        [StringLength(4)]
        public string Acronym { get; set; }
        [StringLength(256)]
        public string Description { get; set; }
        public uint ProjectId { get; set; }
        public uint? DefaultFolderId { get; set; }
        public uint? DefaultIterationId { get; set; }
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }
        public virtual Folder DefaultFolder { get; set; }
        [ForeignKey("DefaultIterationId")]
        public virtual Iteration DefaultIteration { get; set; }
        public virtual ICollection<TeamFolder> TeamFolders { get; set; }
        public virtual ICollection<TeamIteration> TeamIterations { get; set; }
        public virtual ICollection<TeamMember> TeamMembers { get; set; }
    }
}
