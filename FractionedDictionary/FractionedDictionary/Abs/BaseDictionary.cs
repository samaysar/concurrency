using System.Collections.Generic;
using System.Threading;

namespace FractionedDictionary.Abs
{
    internal abstract class BaseDictionary<TKey, TValue>
    {
        protected readonly BaseDictionary<TKey, TValue>[] Collection = new BaseDictionary<TKey, TValue>[256];

        internal virtual bool TryAdd(TKey key, TValue value, int hashCode, IComparer<TKey> keyComparer)
        {
            var idx = hashCode&0x000000FF;
            if (Collection[idx] == null)
            {
                Interlocked.CompareExchange(ref Collection[idx], GetElement(), null);
            }
            return Collection[idx].TryAdd(key, value, hashCode>>8, keyComparer);
        }

        internal virtual bool TryRemove(TKey key, int hashCode, IComparer<TKey> keyComparer)
        {
            var idx = hashCode & 0x000000FF;
            return Collection[idx] != null && Collection[idx].TryRemove(key, hashCode>>8, keyComparer);
        }

        internal virtual BaseDictionary<TKey, TValue> TryAdd(TKey key, TValue value, IComparer<TKey> keyComparer)
        {
            throw new System.NotImplementedException();
        }

        internal virtual BaseDictionary<TKey, TValue> TryRemove(TKey key, IComparer<TKey> keyComparer)
        {
            throw new System.NotImplementedException();
        }

        protected abstract BaseDictionary<TKey, TValue> GetElement();
    }
}