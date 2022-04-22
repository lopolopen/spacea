using System;
using System.Collections.Generic;

namespace SpaceA.Model.Dto
{
    public class ReOrder
    {
        public uint? ParentId { get; set; }
        public uint? PreviousId { get; set; }
        public uint? NextId { get; set; }
    }
}