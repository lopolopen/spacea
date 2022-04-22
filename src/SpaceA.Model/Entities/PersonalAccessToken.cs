using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SpaceA.Model.Entities
{
    public class PersonalAccessToken : IEntity<string>
    {
        [Column("Token")]
        [StringLength(28)]
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public uint OwnerId { get; set; }
        [StringLength(64)]
        public string Remarks { get; set; }

        public virtual Member Owner { get; set; }
    }
}
