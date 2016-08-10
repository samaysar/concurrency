using System.Collections.Generic;
#pragma warning disable 420

namespace FractionedDictionary
{
    internal class BinaryDataTree<TKey, TValue>
    {
        internal static readonly BinaryDataTree<TKey, TValue> Empty = new DefaultBinaryDataTree();
        private readonly KeyValuePair<TKey, TValue> _pair;
        private volatile BinaryDataTree<TKey, TValue> _left;
        private volatile BinaryDataTree<TKey, TValue> _right;

        internal BinaryDataTree(KeyValuePair<TKey, TValue> pair) : this(pair, Empty, Empty) {}

        private BinaryDataTree(KeyValuePair<TKey, TValue> pair, BinaryDataTree<TKey, TValue> left,
            BinaryDataTree<TKey, TValue> right)
        {
            _pair = pair;
            _left = left;
            _right = right;
        }

        public virtual bool TryAdd(KeyValuePair<TKey, TValue> node, IComparer<TKey> keyComparer,
            out BinaryDataTree<TKey, TValue> root)
        {
            var compareResult = keyComparer.Compare(_pair.Key, node.Key);
            if (compareResult<0)
            {
                if (_left.TryAdd(node, keyComparer, out root))
                {
                    root = new BinaryDataTree<TKey, TValue>(_pair, root, _right);
                    return true;
                }
            }
            else if (compareResult>0)
            {
                if (_right.TryAdd(node, keyComparer, out root))
                {
                    root = new BinaryDataTree<TKey, TValue>(_pair, _left, root);
                    return true;
                }
            }
            root = this;
            return false;
        }

        public virtual bool TryRemove(TKey key, IComparer<TKey> keyComparer, out BinaryDataTree<TKey, TValue> root)
        {
            var compareResult = keyComparer.Compare(_pair.Key, key);
            if (compareResult == 0)
            {
                if (_left.IsEmpty)
                {
                    root = _right;
                }
                else if(_right.IsEmpty)
                {
                    root = _left;
                }
                else
                {
                    var rightParent = _left;
                    while (!rightParent._right.IsEmpty)
                    {
                        rightParent = rightParent._right;
                    }
                    rightParent._right = _right;
                    root = _left;
                    return true;
                }
                return true;
            }
            if (compareResult<0)
            {
                if (_left.TryRemove(key, keyComparer, out root))
                {
                    root = new BinaryDataTree<TKey, TValue>(_pair, root, _right);
                    return true;
                }
            }
            else if (compareResult>0 && _right.TryRemove(key, keyComparer, out root))
            {
                root = new BinaryDataTree<TKey, TValue>(_pair, _left, root);
                return true;
            }
            root = this;
            return false;
        }

        public virtual int AddOrUpdate(KeyValuePair<TKey, TValue> node, IComparer<TKey> keyComparer,
            out BinaryDataTree<TKey, TValue> root)
        {
            var compareResult = keyComparer.Compare(_pair.Key, node.Key);
            if (compareResult<0)
            {
                var changeCount = _left.AddOrUpdate(node, keyComparer, out root);
                root = new BinaryDataTree<TKey, TValue>(_pair, root, _right);
                return changeCount;
            }
            if (compareResult>0)
            {
                var changeCount = _right.AddOrUpdate(node, keyComparer, out root);
                root = new BinaryDataTree<TKey, TValue>(_pair, _left, root);
                return changeCount;
            }
            root = new BinaryDataTree<TKey, TValue>(node, _left, _right);
            return 0;
        }

        public virtual bool TryGetValue(TKey key, IComparer<TKey> keyComparer, out TValue value)
        {
            value = _pair.Value;
            var compareResult = keyComparer.Compare(_pair.Key, key);
            return compareResult == 0 ||
                   (compareResult>0
                       ?_right.TryGetValue(key, keyComparer, out value)
                       :_left.TryGetValue(key, keyComparer, out value));
        }

        protected virtual bool IsEmpty => false;

        private sealed class DefaultBinaryDataTree : BinaryDataTree<TKey, TValue>
        {
            internal DefaultBinaryDataTree() : base(default(KeyValuePair<TKey, TValue>), null, null) {}

            public override bool TryAdd(KeyValuePair<TKey, TValue> pair, IComparer<TKey> keyComparer,
                out BinaryDataTree<TKey, TValue> root)
            {
                root = new BinaryDataTree<TKey, TValue>(pair, this, this);
                return true;
            }

            public override bool TryRemove(TKey key, IComparer<TKey> keyComparer,
                out BinaryDataTree<TKey, TValue> root)
            {
                root = this;
                return false;
            }

            public override int AddOrUpdate(KeyValuePair<TKey, TValue> pair, IComparer<TKey> keyComparer,
                out BinaryDataTree<TKey, TValue> root)
            {
                root = new BinaryDataTree<TKey, TValue>(pair, this, this);
                return 1;
            }

            public override bool TryGetValue(TKey key, IComparer<TKey> keyComparer, out TValue value)
            {
                value = default(TValue);
                return false;
            }

            protected override bool IsEmpty => true;
        }
    }
}