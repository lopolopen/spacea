using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceA.Model.Entities
{
    public class Tag : IEntity<uint>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; }
        public uint? ProjectId { get; set; }
        public uint? WorkItemId { get; set; }
        [StringLength(32)]
        public string Text { get; set; }
        [StringLength(16)]
        public string Color { get; set; }
        public virtual Project Project { get; set; }
        public virtual WorkItem WorkItem { get; set; }
    }
}