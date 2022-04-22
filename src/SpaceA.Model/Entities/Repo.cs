using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceA.Model.Entities
{
    public class Repo : IEntity<uint>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public uint Id { get; set; }
        public string Name { get; set; }
        public string NameWithNamespace { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string WebUrl { get; set; }
        public string HttpUrlToRepo { get; set; }
        public string SshUrlToRepo { get; set; }
        public string Description { get; set; }
        public string NamespaceName { get; set; }
        public string NamespacePath { get; set; }
        public string NamespaceFullPath { get; set; }
        public string NamespaceKind { get; set; }

        public virtual ICollection<ProjectRepo> ProjectRepos { get; set; }
    }
}