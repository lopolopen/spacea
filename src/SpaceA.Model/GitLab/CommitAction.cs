using System;

namespace SpaceA.Model.GitLab
{
    public class CommitAction
    {
        public string Action { get; set; }
        public string FilePath { get; set; }

        public string Content { get; set; }
        public string Encoding { get; set; }
    }
}