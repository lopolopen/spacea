using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceA.Model.Entities
{
    public class Folder : IEntity<uint>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; }
        [Required]
        [StringLength(128)]
        public string Name { get; set; }
        [StringLength(512)]
        public string Path { get; set; }
        public uint ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }
        public virtual ICollection<Team> Teams { get; set; }
        public virtual ICollection<TeamFolder> TeamFolders { get; set; }
        public virtual ICollection<WorkItem> WorkItems { get; set; }
    }
}
