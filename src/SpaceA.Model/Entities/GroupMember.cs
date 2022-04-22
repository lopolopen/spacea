using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceA.Model.Entities
{
    public class GroupMember : IEntity<uint, uint>
    {
#if MYSQL
        [Column("group_id")]
#else
        [Column("GroupId")]
#endif
        public uint Id1 { get; set; }

#if MYSQL
        [Column("member_id")]
#else
        [Column("MemberId")]
#endif
        public uint Id2 { get; set; }

        public virtual Member Member { get; set; }
        public virtual Group Group { get; set; }
    }
}
