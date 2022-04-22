using System;


namespace SpaceA.Model.GitLab
{
    public class Namespace
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string FullPath { get; set; }
        public string Kind { get; set; }
    }
}