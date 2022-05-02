using System;
using System.Collections.Generic;

namespace SpaceA.Model.Dto
{
    public class Project
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AvatarUrl => string.IsNullOrEmpty(AvatarUid) ? null : $"projects/{Id}/avatar";
        public string AvatarUid { get; set; }
        public DateTime CreatedDate { get; set; }
        public uint? DefaultTeamId { get; set; }
        public uint RootFolderId { get; set; }
        public uint RootIterationId { get; set; }
        public List<Team> Teams { get; set; }
        public List<Member> Members { get; set; }
        public uint OwnerId { get; set; }
        public Member Owner { get; set; }
        public List<Folder> Folders { get; set; }
        public List<Iteration> Iterations { get; set; }
    }
}