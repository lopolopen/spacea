using System;
using System.Collections.Generic;

namespace SpaceA.Model.Dto
{
    public class Group
    {
        public uint Id { get; set; }
        public string AccountName { get; set; }
        public string Name { get; set; }
        public string Acronym { get; set; }
        public string Description { get; set; }
        public bool Disabled { get; set; }
        public uint? LeaderId { get; set; }
        public Member Leader { get; set; }
        public List<Member> Members { get; set; }
    }
}