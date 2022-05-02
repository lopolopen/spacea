namespace SpaceA.WebApi.Options
{
    public class LdapOptions
    {
        public const string PREFIX = "Ldap";

        public string Host { get; set; }
        public int Port { get; set; }

        public string BindDn { get; set; }

        public string BindPassword { get; set; }

        public string GroupsBindBase { get; set; }

        public string BindBase { get; set; }
    }
}
