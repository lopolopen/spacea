using System;
using System.Collections.Generic;

namespace SpaceA.Model.Dto
{
    public class ProjectMember
    {
        public uint Id { get; set; }
        public uint ProjectId { get; set; }
        public uint MemberId { get; set; }
        public virtual Member Member { get; set; }
    }
}