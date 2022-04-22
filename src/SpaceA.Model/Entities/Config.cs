using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceA.Model.Entities
{
    public class Config : IEntity<uint, string>
    {
        [Column("MemberId")]
        public uint Id1 { get; set; }
        [Column("Key")]
        [StringLength(64)]
        public string Id2 { get; set; }
        [NotMapped]
        public string Key
        {
            get { return Id2; }
            set { Id2 = value; }
        }
        public string Value { get; set; }

        public bool IsShared { get; set; }

        public virtual Member Member { get; set; }
    }
}