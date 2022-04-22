using System;
using System.Collections.Generic;

namespace SpaceA.Model.Dto
{
    public class Member
    {
        public uint Id { get; set; }
        public string AccountName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Xing { get; set; }
        public string Ming { get; set; }
        public bool Disabled { get; set; }
        public string Password { get; set; }
        public string JobNumber { get; set; }
        public string AvatarUrl => string.IsNullOrEmpty(AvatarUid) ? null : $"members/{Id}/avatar";
        public string AvatarUid { get; set; }


    }
}