using System;

namespace SpaceA.Model.Entities
{
    public class MemberCapacity
    {
        public uint TeamId { get; set; }
        public uint IterationId { get; set; }
        public uint MemberId { get; set; }
        public CapacityType Type { get; set; }
        public float HoursPerDay { get; set; }
    }
}