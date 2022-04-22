using System;
using System.Collections.Generic;

namespace SpaceA.Model.Dto
{
    public class Capacity
    {
        public uint OwnerId { get; set; }
        public CapacityType Type { get; set; }
        public float HoursPerDay { get; set; }
    }
}
