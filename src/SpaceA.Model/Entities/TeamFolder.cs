using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceA.Model.Entities
{
    public class TeamFolder : IEntity<uint, uint>
    {
#if MYSQL
        [Column("team_id")]
#else
        [Column("TeamId")]
#endif
        public uint Id1 { get; set; }

#if MYSQL
        [Column("folder_id")]
#else
        [Column("FolderId")]
#endif
        public uint Id2 { get; set; }

        public virtual Team Team { get; set; }

        public virtual Folder Folder { get; set; }

    }
}