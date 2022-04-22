using System;


namespace SpaceA.Model.GitLab
{
    public class Repo
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string SshUrlToRepo { get; set; }
        public string WebUrl { get; set; }
        public string HttpUrlToRepo { get; set; }
        public string DefaultBranch { get; set; }
        public string NameWithNamespace { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public string PathWithNamespace { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Namespace Namespace { get; set; }
    }
}