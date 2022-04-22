using System;
using System.Collections.Generic;

namespace SpaceA.Model.Dto
{
    public class Iteration : WithMask<Iteration>
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public uint ProjectId { get; set; }
    }
}