using System;

namespace SpaceA.Model.GitLab
{
    public class File
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Encoding { get; set; }
        public string Content { get; set; }
        public string ContentSha256 { get; set; }
        public string Ref { get; set; }

        public string Branch { get; set; }
        public string CommitMessage { get; set; }
    }
}