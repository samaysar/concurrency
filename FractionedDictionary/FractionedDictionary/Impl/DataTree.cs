using System.Collections.Generic;
using FractionedDictionary.Abs;

namespace FractionedDictionary.Impl
{
    internal class DataTree<TKey, TValue> : BaseDictionary<TKey, TValue>
    {
        private static readonly DataTree<TKey,TValue> Default = new DefNode<TKey, TValue>();
        private readonly TKey _key;
        private readonly TValue _value;
        private DataTree<TKey, TValue> _left;
        private readonly DataTree<TKey, TValue> _right;

        protected DataTree()
        {
            _key = default(TKey);
            _value = default(TValue);
            _left = this;
            _right = this;
        }

        internal DataTree(TKey key, TValue value)
        {
            _key = key;
            _value = value;
            _left = Default;
            _right = Default;
        }

        private DataTree(DataTree<TKey, TValue> iden)
        {
            _key = iden._key;
            _value = iden._value;
            _left = iden._left;
            _right = iden._right;
        }

        private DataTree(TKey key, TValue value, DataTree<TKey, TValue> left, DataTree<TKey, TValue> right)
        {
            _key = key;
            _value = value;
            _left = left;
            _right = right;
        }

        protected override BaseDictionary<TKey, TValue> GetElement()
        {
            throw new System.NotImplementedException();
        }

        internal override BaseDictionary<TKey, TValue> TryAdd(TKey key, TValue value, IComparer<TKey> keyComparer)
        {
            return TryAddIntern(key, value, keyComparer);
        }

        private DataTree<TKey, TValue> TryAddIntern(TKey key, TValue value, IComparer<TKey> keyComparer)
        {
            if (Empty) return new DataTree<TKey, TValue>(key, value);
            var compVal = keyComparer.Compare(key, _key);
            if (compVal == 0) return this;
            if (compVal < 0)
            {
                var newLeft = _left.TryAddIntern(key, value, keyComparer);
                return ReferenceEquals(newLeft, _left) ? this : new DataTree<TKey, TValue>(_key, _value, newLeft, _right);
            }
            var newRight = _right.TryAddIntern(key, value, keyComparer);
            return ReferenceEquals(newRight, _right) ? this : new DataTree<TKey, TValue>(_key, _value, _left, newRight);
        }

        internal override BaseDictionary<TKey, TValue> TryRemove(TKey key, IComparer<TKey> keyComparer)
        {
            return TryRemoveIntern(key, keyComparer);
        }

        private DataTree<TKey, TValue> TryRemoveIntern(TKey key, IComparer<TKey> keyComparer)
        {
            if (Empty) return this;
            var compVal = keyComparer.Compare(key, _key);
            if (compVal == 0)
            {
                if (_left.Empty) return _right;
                if (_right.Empty) return _left;
                var leftRoot = new DataTree<TKey, TValue>(_right);
                var movingLeftRoot = leftRoot;
                while (!movingLeftRoot._left.Empty)
                {
                    movingLeftRoot._left = new DataTree<TKey, TValue>(movingLeftRoot._left);
                    movingLeftRoot = movingLeftRoot._left;
                }
                movingLeftRoot._left = _left;
                return leftRoot;
            }
            if (compVal < 0)
            {
                var newLeft = _left.TryRemoveIntern(key, keyComparer);
                return ReferenceEquals(newLeft, _left) ? this : new DataTree<TKey, TValue>(_key, _value, newLeft, _right);
            }
            var newRight = _right.TryRemoveIntern(key, keyComparer);
            return ReferenceEquals(newRight, _right) ? this : new DataTree<TKey, TValue>(_key, _value, _left, newRight);
        }

        protected virtual bool Empty => false;
    }

    internal sealed class DefNode<TKey, TValue> : DataTree<TKey, TValue>
    {
        protected override bool Empty => true;
    }
}