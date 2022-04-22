using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceA.Model.Entities
{
    public class TeamMember : IEntity<uint, uint>
    {
#if MYSQL
        [Column("team_id")]
#else
        [Column("TeamId")]
#endif
        public uint Id1 { get; set; }

#if MYSQL
        [Column("member_id")]
#else
        [Column("MemberId")]
#endif
        public uint Id2 { get; set; }

        public virtual Member Member { get; set; }
        public virtual Team Team { get; set; }
    }
}
