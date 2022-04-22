using System;
using System.Collections.Generic;

namespace SpaceA.Model.Dto
{
    public class Folder
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public uint ProjectId { get; set; }
    }
}