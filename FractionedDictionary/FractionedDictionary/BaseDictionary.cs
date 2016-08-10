using System.Collections.Generic;

namespace FractionedDictionary
{
    internal abstract class BaseDictionary<TKey, TValue>
    {
        public abstract bool TryGetValue(TKey key, IComparer<TKey> keyComparer, out TValue value);
    }
}