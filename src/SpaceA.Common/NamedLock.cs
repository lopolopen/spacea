using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace SpaceA.Common
{
    public class NamedLock
    {
        private readonly ConcurrentDictionary<string, object> _lockerPool = new ConcurrentDictionary<string, object>();

        public object this[string name] => _lockerPool.GetOrAdd(name, _ => new object());
    }

    public class NamedLock2
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _lockerPool = new ConcurrentDictionary<string, SemaphoreSlim>();

        public SemaphoreSlim this[string name] => _lockerPool.GetOrAdd(name, _ => new SemaphoreSlim(1, 1));
    }
}
