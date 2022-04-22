using System;
using System.Collections.Generic;

namespace SpaceA.Common
{
    public sealed class Comparer<T> : IEqualityComparer<T>, IComparer<T>
    {
        private readonly Func<T, T, int> _compare;
        private readonly Func<T, int> _hash;

        private Comparer(Func<T, T, int> compare, Func<T, int> hash)
        {
            _compare = compare;
            _hash = hash;
        }

        // for IComparer<T>
        public static Comparer<T> Use(Func<T, T, int> compare)
        {
            return new Comparer<T>(compare, null);
        }

        // for IEqualityComparer<T>
        public static Comparer<T> Use<TBuiltIn>(Func<T, TBuiltIn> @as)
            where TBuiltIn : IEquatable<TBuiltIn>
        {
            return Use((x, y) => @as(x).Equals(@as(y)), h => @as(h).GetHashCode());
        }

        // for IEqualityComparer<T>
        public static Comparer<T> Use<TBuiltIn>(Func<T, TBuiltIn> @as, Func<T, int> hash)
            where TBuiltIn : IEquatable<TBuiltIn>
        {
            return Use((x, y) => @as(x).Equals(@as(y)), hash);
        }

        // for IEqualityComparer<T>
        public static Comparer<T> Use(Func<T, T, bool> compare, Func<T, int> hash)
        {
            return new Comparer<T>((x, y) => compare(x, y) ? 0 : 1, hash);
        }

        public int Compare(T x, T y)
        {
            return _compare(x, y);
        }

        public bool Equals(T x, T y)
        {
            return _compare(x, y) == 0;
        }

        public int GetHashCode(T obj)
        {
            return _hash?.Invoke(obj) ?? 0;
        }
    }
}
