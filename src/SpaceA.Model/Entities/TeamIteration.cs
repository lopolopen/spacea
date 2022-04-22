using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceA.Model.Entities
{
    public class TeamIteration : IEntity<uint, uint>
    {
#if MYSQL
        [Column("team_id")]
#else
        [Column("TeamId")]
#endif
        public uint Id1 { get; set; }

#if MYSQL
        [Column("iteration_id")]
#else
        [Column("IterationId")]
#endif
        public uint Id2 { get; set; }

        public virtual Team Team { get; set; }

        public virtual Iteration Iteration { get; set; }

    }
}