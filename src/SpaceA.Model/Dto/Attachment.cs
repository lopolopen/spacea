using System;
using System.Collections.Generic;

namespace SpaceA.Model.Dto
{
    public class Attachment
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public long Size { get; set; }
        public DateTime? UploadedTime { get; set; }
        public uint? WorkItemId { get; set; }
        public Member Creator { get; set; }
        public string Comments { get; set; }
    }
}