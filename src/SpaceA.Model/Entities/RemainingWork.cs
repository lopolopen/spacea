using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SpaceA.Model.Entities
{
    public class RemainingWork
    {
        public uint TeamId { get; set; }
        public uint IterationId { get; set; }
        public WorkItemType WorkItemType { get; set; }
        public DateTime AccountingDate { get; set; }

        public float EstimatedHours { get; set; }
        public float RemainingHours { get; set; }
        public float CompletedHours { get; set; }
        public int RemainingCount { get; set; }
        public float CompletedCount { get; set; }
    }
}
