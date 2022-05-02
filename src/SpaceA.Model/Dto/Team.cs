using System;
using System.Collections.Generic;

namespace SpaceA.Model.Dto
{
    public class Team
    {
        public uint Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Acronym { get; set; }

        public uint[] MemberIds { get; set; }

        public List<Member> Members { get; set; }
        
        public List<Folder> Folders { get; set; }

        public List<uint> IterationIds { get; set; }

        public Folder DefaultFolder { get; set; }

        public Iteration DefaultIteration { get; set; }
    }
}