using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Novell.Directory.Ldap;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace SpaceA.LdapSyncWorker
{
    public class SyncWorker : BackgroundService
    {
        private readonly ILogger<SyncWorker> _logger;
        private readonly LdapOptions _adOptions;
        private readonly SyncOptions _syncOptions;
        private readonly IConfiguration _config;
        private readonly string _connStr;
        private Dictionary<string, uint> _groupMap;
        private Dictionary<string, uint> _memberMap;
        private HashSet<(uint, uint)> _idPairs;


        public SyncWorker(
            ILogger<SyncWorker> logger,
            IOptions<LdapOptions> adOptions,
            IOptions<SyncOptions> syncOptions,
            IConfiguration config)
        {
            _logger = logger;
            _adOptions = adOptions.Value;
            _syncOptions = syncOptions.Value;
            _config = config;
            _connStr = _config["ConnectionStrings:DefaultConnection"];
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _groupMap = await GetAllGroups();
            _memberMap = await GetAllMembers();
            _idPairs = await GetAllIdPairs();

            using (var cn = new LdapConnection())
            {
                cn.Connect(_adOptions.Host, _adOptions.Port);
                cn.Bind(_adOptions.BindDn, _adOptions.BindPassword);
                for (; ; )
                {
                    //var lsc = cn.Search(
                    //    _adOptions.GroupsBindBase,
                    //    LdapConnection.SCOPE_SUB,
                    //    "objectClass=group",
                    //    new[] { "sAMAccountName" },
                    //    false);
                    LdapEntry currentEntry = null;
                    //while (!stoppingToken.IsCancellationRequested
                    //    && lsc.hasMore() && (currentEntry = lsc.next()) != null)
                    //{
                    //    var attributeSet = currentEntry.getAttributeSet();
                    //    foreach (LdapAttribute attr in attributeSet)
                    //    {
                    //        if (EqualsIgnoreCase(attr.Name, "sAMAccountName"))
                    //        {
                    //            var accountName = attr.StringValue;
                    //            if (!_groupMap.ContainsKey(accountName))
                    //            {
                    //                uint id = await AddGroup(accountName);
                    //                _groupMap.Add(accountName, id);
                    //            }
                    //        }
                    //    }
                    //}

                    var lsc = cn.Search(
                        _adOptions.BindBase,
                        LdapConnection.SCOPE_SUB,
                        "objectClass=*",
                        new[] { "objectClass", "cn", "memberOf" },
                        false);

                    while (!stoppingToken.IsCancellationRequested
                        && lsc.hasMore() && (currentEntry = lsc.next()) != null)
                    {
                        string accountName = null;
                        string commonName = null;
                        var groupNames = new List<string>();
                        var attributeSet = currentEntry.getAttributeSet();
                        foreach (LdapAttribute attr in attributeSet)
                        {
                            if (EqualsIgnoreCase(attr.Name, "sAMAccountName"))
                            {
                                accountName = attr.StringValue;
                            }
                            else if (EqualsIgnoreCase(attr.Name, "cn"))
                            {
                                commonName = attr.StringValue;
                            }
                            else if (EqualsIgnoreCase(attr.Name, "memberOf"))
                            {
                                foreach (var groupDn in attr.StringValueArray)
                                {
                                    var match = Regex.Match(groupDn, "CN=(?<group>.+?),");
                                    if (match == null) continue;
                                    var group = match.Groups["group"].Value;
                                    if (_groupMap.ContainsKey(group))
                                    {
                                        groupNames.Add(group);
                                    }
                                }
                            }
                        }
                        if (accountName != null && groupNames.Count > 0)
                        {
                            await AddMemberToGroups(accountName, commonName, groupNames);
                        }
                    }
                    _logger.LogDebug($"Synced from AD at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}.");
                    _logger.LogDebug($"Will wait for {_syncOptions.Interval/1000} seconds.");
                    await Task.Delay(_syncOptions.Interval);
                }
            }
        }

        private bool EqualsIgnoreCase(string x, string y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase) == 0;

        private async Task<Dictionary<string, uint>> GetAllGroups()
        {
            var accountNames = new Dictionary<string, uint>();
            using (var conn = new MySqlConnection(_connStr))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "select id, account_name from `groups`";
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var id = (uint)reader["id"];
                        var accountName = (string)reader["account_name"];
                        accountNames.Add(accountName, id);
                    }
                }
            }
            return accountNames;
        }

        private async Task<Dictionary<string, uint>> GetAllMembers()
        {
            var accountNames = new Dictionary<string, uint>();
            using (var conn = new MySqlConnection(_connStr))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "select id, account_name from members";
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var id = (uint)reader["id"];
                        var accountName = reader["account_name"] as string;
                        accountNames.Add(accountName, id);
                    }
                }
            }
            return accountNames;
        }

        private async Task<HashSet<(uint, uint)>> GetAllIdPairs()
        {
            var idPairs = new HashSet<(uint, uint)>();
            using (var conn = new MySqlConnection(_connStr))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "select GroupId, MemberId from group_members";
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var groupId = unchecked((uint)reader["GroupId"]);
                        var memberId = unchecked((uint)reader["MemberId"]);
                        idPairs.Add((groupId, memberId));
                    }
                }
            }
            return idPairs;
        }

        private async Task<uint> AddGroup(string accountName)
        {
            using (var conn = new MySqlConnection(_connStr))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
insert into `Group`(AccountName, Name) values(@accountName, @accountName);
select LAST_INSERT_ID();";
                cmd.Parameters.AddWithValue("@accountName", accountName);
                uint id = unchecked((uint)(UInt64)await cmd.ExecuteScalarAsync());
                _logger.LogInformation($"New group[{accountName}] is created.");
                return id;
            }
        }

        private async Task AddMemberToGroups(string accountName, string commonName, List<string> groupNames)
        {
            var nameArr = commonName.Split(" ");
            string firstName = nameArr[0];
            string lastName = null;
            if (nameArr.Length >= 2)
            {
                lastName = nameArr[1];
            }
            using (var conn = new MySqlConnection(_connStr))
            {
                await conn.OpenAsync();
                foreach (var gn in groupNames)
                {
                    uint groupId = _groupMap[gn];
                    uint memberId;
                    if (!_memberMap.ContainsKey(accountName))
                    {
                        var cmd1 = conn.CreateCommand();
                        cmd1.CommandText = @"
insert into Member(AccountName, FirstName, LastName) values(@accountName, @firstName, @lastName);
select LAST_INSERT_ID();";
                        cmd1.Parameters.AddWithValue("@accountName", accountName);
                        cmd1.Parameters.AddWithValue("@firstName", firstName);
                        cmd1.Parameters.AddWithValue("@lastName", lastName);
                        memberId = unchecked((uint)(UInt64)await cmd1.ExecuteScalarAsync());
                        _memberMap.Add(accountName, memberId);
                        _logger.LogInformation($"New user[{accountName}] is created.");
                    }
                    else
                    {
                        memberId = _memberMap[accountName];
                    }

                    if (!_idPairs.Contains((groupId, memberId)))
                    {
                        var cmd2 = conn.CreateCommand();
                        cmd2.CommandText = @"
insert into GroupMember(GroupId, MemberId) values(@groupId, @memberId);";
                        cmd2.Parameters.AddWithValue("@groupId", groupId);
                        cmd2.Parameters.AddWithValue("@memberId", memberId);
                        await cmd2.ExecuteNonQueryAsync();
                        _idPairs.Add((groupId, memberId));
                        _logger.LogInformation($"User[{accountName}] added to group[{gn}].");
                    }
                }
            }
        }
    }
}
