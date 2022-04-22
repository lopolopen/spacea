using System;
using System.Collections.Generic;

namespace SpaceA.Model.Dto
{
    public class GitLabAccessToken
    {
        public uint OwnerId { get; set; }
        public string DesensitizedValue { get; set; }
        public string CipherValue { get; set; }
        public Member BelongTo { get; set; }
        public bool IsShared { get; set; }
    }

    public class CopAccessToken
    {
        public string DesensitizedValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public string Remarks { get; set; }
    }
}