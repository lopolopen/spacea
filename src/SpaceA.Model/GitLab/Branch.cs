using System;

namespace SpaceA.Model.GitLab
{
    public class Branch
    {
        public string Name { get; set; }
        public bool Default { get; set; }

        public string Ref { get; set; }
    }
}