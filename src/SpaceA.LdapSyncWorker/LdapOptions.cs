using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceA.LdapSyncWorker
{
    public class LdapOptions
    {
        public const string CONFIG_PREFIX = "Ldap";

        public string Host { get; set; }

        public int Port { get; set; }

        public string BindDn { get; set; }

        public string BindPassword { get; set; }

        public string BindBase { get; set; }
    }
}
