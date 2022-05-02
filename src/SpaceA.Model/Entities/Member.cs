using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceA.Model.Entities
{
    public class Member : IEntity<uint>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; }
        [Required]
        [StringLength(32)]
        public string AccountName { get; set; }
        [StringLength(64)]
        public string Password { get; set; }
        [StringLength(6)]
        public string Salt { get; set; }
        [StringLength(16)]
        public string FirstName { get; set; }
        [StringLength(16)]
        public string LastName { get; set; }
        [StringLength(2)]
        public string Xing { get; set; }
        [StringLength(2)]
        public string Ming { get; set; }
        [StringLength(32)]
        public string AvatarUid { get; set; }
        public bool Disabled { get; set; }
        public string RefreshToken { get; set; }
        public virtual Group Group { get; set; }
        public virtual ICollection<Project> OwnedProjects { get; set; }
        public virtual ICollection<WorkItem> WorkItems { get; set; }
        public virtual ICollection<Attachment> Attachments { get; set; }
        public virtual ICollection<WorkItem> ChangedWorkItems { get; set; }
        public virtual ICollection<WorkItem> ProposedStories { get; set; }
        public virtual ICollection<GroupMember> GroupMembers { get; set; }
        public virtual ICollection<TeamMember> TeamMembers { get; set; }
        public virtual ICollection<Config> Configs { get; set; }
        public virtual ICollection<PersonalAccessToken> PersonalAccessTokens { get; set; }
    }
}
