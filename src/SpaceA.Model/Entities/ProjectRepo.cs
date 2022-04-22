using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceA.Model.Entities
{
    public class ProjectRepo : IEntity<uint, uint>
    {
#if MYSQL
        [Column("project_id")]
#else
        [Column("ProjectId")]
#endif
        public uint Id1 { get; set; }

#if MYSQL
        [Column("repo_id")]
#else
        [Column("RepoId")]
#endif
        public uint Id2 { get; set; }

        public virtual Project Project { get; set; }
        public virtual Repo Repo { get; set; }
    }
}