using System;
using System.Collections.Generic;

namespace SpaceA.Model.GitLab
{
    public class Commit
    {
        public string Branch { get; set; }
        public string CommitMessage { get; set; }
        public List<CommitAction> Actions { get; set; }
    }
}