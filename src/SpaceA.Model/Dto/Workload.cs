using System;
using System.Collections.Generic;

namespace SpaceA.Model.Dto
{
    public class Workload
    {
        public uint OwnerId { get; set; }
        public float AssignedCapacity { get; set; }
        public float Capacity { get; set; }
    }

    public class Workloads
    {
        public Workload TeamWorkload { get; set; }
        public List<Workload> MemberWorkloads { get; set; }
    }
}
