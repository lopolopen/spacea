using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceA.Model.Entities
{
    public class Group : IEntity<uint>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; }
        [Required]
        [StringLength(32)]
        public string AccountName { get; set; }
        [StringLength(32)]
        public string Name { get; set; }
        [StringLength(4)]
        public string Acronym { get; set; }
        [StringLength(256)]
        public string Desc { get; set; }
        public bool Disabled { get; set; }
        public uint? LeaderId { get; set; }
        public virtual Member Leader { get; set; }
        public virtual ICollection<GroupMember> GroupMembers { get; set; }
    }
}
