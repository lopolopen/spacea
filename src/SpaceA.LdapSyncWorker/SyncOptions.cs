using System;

namespace SpaceA.LdapSyncWorker
{
    public class SyncOptions
    {
        public int Interval { get; set; } = 30_000;
    }
}