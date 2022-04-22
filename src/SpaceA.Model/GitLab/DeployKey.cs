using System;

namespace SpaceA.Model.GitLab
{
    public class DeployKey
    {
        public uint Id { get; set; }
        public string Title { get; set; }
        public string Key { get; set; }
        public bool CanPush { get; set; }
    }
}