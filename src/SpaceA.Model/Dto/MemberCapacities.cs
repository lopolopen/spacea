using System;
using System.Collections.Generic;

namespace SpaceA.Model.Dto
{
    public class MemberCapacities
    {
        public uint MemberId { get; set; }
        public List<Capacity> Capacities { get; set; }
    }
}
