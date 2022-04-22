using System;


namespace SpaceA.Model.GitLab
{
    public class ProtectedBranch
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public AccessLevel PushAccessLevel { get; set; }
        public AccessLevel MergeAccessLevel { get; set; }
        public AccessLevel UnprotectAccessLevel { get; set; }
    }
}