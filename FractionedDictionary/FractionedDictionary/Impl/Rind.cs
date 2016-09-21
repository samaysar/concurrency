using System.Collections.Generic;
using System.Threading;
using FractionedDictionary.Abs;

namespace FractionedDictionary.Impl
{
    internal sealed class Rind<TKey, TValue> : BaseDictionary<TKey, TValue>
    {
        protected override BaseDictionary<TKey, TValue> GetElement()
        {
            return new Sind<TKey, TValue>();
        }
    }

    internal sealed class Sind<TKey, TValue> : BaseDictionary<TKey, TValue>
    {
        protected override BaseDictionary<TKey, TValue> GetElement()
        {
            return new Tind<TKey, TValue>();
        }
    }

    internal sealed class Tind<TKey, TValue> : BaseDictionary<TKey, TValue>
    {
        protected override BaseDictionary<TKey, TValue> GetElement()
        {
            return new Find<TKey, TValue>();
        }
    }

    internal sealed class Find<TKey, TValue> : BaseDictionary<TKey, TValue>
    {
        internal override bool TryAdd(TKey key, TValue value, int hashCode, IComparer<TKey> keyComparer)
        {
            bool retValue;
            try {}
            finally
            {
                var idx = hashCode&0x000000FF;
                if (Collection[idx] == null &&
                    Interlocked.CompareExchange(ref Collection[idx], new DataTree<TKey, TValue>(key, value), null) ==
                    null)
                {
                    retValue = true;
                }
                else
                {
                    BaseDictionary<TKey, TValue> curr, newCurr;
                    do
                    {
                        curr = Collection[idx];
                        newCurr = curr.TryAdd(key, value, keyComparer);
                        retValue = !ReferenceEquals(curr, newCurr);
                        if (!retValue) break;
                    } while (Interlocked.CompareExchange(ref Collection[idx], newCurr, curr) != curr);
                }
            }
            return retValue;
        }

        internal override bool TryRemove(TKey key, int hashCode, IComparer<TKey> keyComparer)
        {
            var retValue = false;
            try { }
            finally
            {
                var idx = hashCode & 0x000000FF;
                if (Collection[idx] != null)
                {
                    BaseDictionary<TKey, TValue> curr, newCurr;
                    do
                    {
                        curr = Collection[idx];
                        newCurr = curr.TryRemove(key, keyComparer);
                        retValue = !ReferenceEquals(curr, newCurr);
                        if (!retValue) break;
                    } while (Interlocked.CompareExchange(ref Collection[idx], newCurr, curr) != curr);
                }
            }
            return retValue;
        }

        protected override BaseDictionary<TKey, TValue> GetElement()
        {
            throw new System.NotImplementedException();
        }
    }
}