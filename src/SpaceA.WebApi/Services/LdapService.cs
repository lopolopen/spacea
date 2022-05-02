using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SpaceA.Model.Entities;
using SpaceA.WebApi.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Novell.Directory.Ldap;
using SpaceA.WebApi.Options;

namespace SpaceA.WebApi.Services
{
    public class LdapService : ILdapService
    {
        // private readonly IConfiguration _configuration;
        private readonly LdapOptions _ldapOptions;

        public LdapService(IConfiguration configuration)
        {
            // _configuration = configuration;
            _ldapOptions = configuration.GetSection("Ldap").Get<LdapOptions>();
        }

        public Member GetMember(string accountName)
        {
            using (var cn = new LdapConnection())
            {
                cn.Connect(_ldapOptions.Host, _ldapOptions.Port);
                cn.Bind(_ldapOptions.BindDn, _ldapOptions.BindPassword);
                var lsc = cn.Search(
                    _ldapOptions.BindBase,
                    LdapConnection.SCOPE_SUB,
                    $"sAMAccountName={accountName}",
                    new[] { "cn" },
                    false
                );
                LdapEntry currentEntry = null;
                string commonName = null;
                while (lsc.hasMore() && (currentEntry = lsc.next()) != null)
                {
                    var attributeSet = currentEntry.getAttributeSet();
                    foreach (LdapAttribute attr in attributeSet)
                    {
                        if (EqualsIgnoreCase(attr.Name, "cn"))
                        {
                            commonName = attr.StringValue;
                        }
                    }
                }
                if (commonName == null)
                {
                    throw new Exception($"Can not find {accountName} in {_ldapOptions.BindBase}");
                }
                var nameArr = commonName.Split(" ");
                string firstName = nameArr[0];
                string lastName = null;
                if (nameArr.Length >= 2)
                {
                    lastName = nameArr[1];
                }
                return new Member
                {
                    AccountName = accountName,
                    FirstName = firstName,
                    LastName = lastName,
                    Disabled = false
                };
            }
        }

        public bool Verify(string accountName, string password)
        {
            using (var cn = new LdapConnection())
            {
                try
                {
                    cn.Connect(_ldapOptions.Host, _ldapOptions.Port);
                    cn.Bind($@"CORP\{accountName}", password);
                    return true;
                }
                catch (LdapException ex)
                {
                    //验证失败，用户名或密码错误
                    if (ex.ResultCode == 49)
                    {
                        return false;
                    }
                    //其它错误，比如网络不可用
                    else
                    {
                        throw ex;
                    }
                }
            }
        }

        private static bool EqualsIgnoreCase(string x, string y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase) == 0;
    }


}