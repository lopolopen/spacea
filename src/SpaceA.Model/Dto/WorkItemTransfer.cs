using System;
using System.Collections.Generic;

namespace SpaceA.Model.Dto
{
    public class WorkItemTransfer
    {
        public uint FromFolderId { get; set; }
        public string FromFolderPath { get; set; }
        public uint ToFolderId { get; set; }
        public string ToFolderPath { get; set; }
    }
}