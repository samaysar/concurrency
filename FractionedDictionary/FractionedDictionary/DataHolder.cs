using System.Collections.Generic;
using System.Threading;
#pragma warning disable 420

namespace FractionedDictionary
{
    internal sealed class DataHolder<TKey, TValue> : BaseDictionary<TKey, TValue>
    {
        private volatile BinaryDataTree _root = null;

        public int TryAdd(KeyValuePair<TKey, TValue> data, Comparer<TKey> keyComparer)
        {
            var node = new BinaryDataTree(data);
            var rootCopy = _root;
            if (rootCopy == null)
            {
                rootCopy = Interlocked.CompareExchange(ref _root, node, null);
                if (rootCopy == null) return 1;
            }
            BinaryDataTree newRoot;
            while (rootCopy.TryAdd(node, keyComparer, out newRoot))
            {
                var newRootCopy = Interlocked.CompareExchange(ref _root, newRoot, rootCopy);
                if (newRootCopy == rootCopy) return 1;
                if (newRootCopy == null)
                {
                    newRootCopy = Interlocked.CompareExchange(ref _root, node, null);
                    if (newRootCopy == null) return 1;
                }
                rootCopy = newRootCopy;
            }
            return 0;
        }

        public int TryRemove(TKey key, Comparer<TKey> keyComparer)
        {
            var rootCopy = _root;
            if (rootCopy == null) return 0;
            BinaryDataTree newRoot;
            while (rootCopy != null && rootCopy.TryRemove(key, keyComparer, out newRoot))
            {
                var newRootCopy = Interlocked.CompareExchange(ref _root, newRoot, rootCopy);
                if (newRootCopy == rootCopy) return -1;
                rootCopy = newRootCopy;
            }
            return 0;
        }

        private sealed class BinaryDataTree
        {
            private readonly KeyValuePair<TKey, TValue> _pair;
            private volatile BinaryDataTree _left;
            private volatile BinaryDataTree _right;

            internal BinaryDataTree(KeyValuePair<TKey, TValue> pair) : this(pair, null, null)
            {
            }

            private BinaryDataTree(KeyValuePair<TKey, TValue> pair, BinaryDataTree left, BinaryDataTree right)
            {
                _pair = pair;
                _left = left;
                _right = right;
            }

            internal bool TryAdd(BinaryDataTree node, IComparer<TKey> keyComparer, out BinaryDataTree root)
            {
                var compareResult = keyComparer.Compare(_pair.Key, node._pair.Key);
                if (compareResult<0)
                {
                    if (_left == null)
                    {
                        root = new BinaryDataTree(_pair, node, _right);
                        return true;
                    }
                    BinaryDataTree subRoot;
                    if (_left.TryAdd(node, keyComparer, out subRoot))
                    {
                        root = new BinaryDataTree(_pair, subRoot, _right);
                        return true;
                    }
                }
                else if (compareResult>0)
                {
                    if (_right == null)
                    {
                        root = new BinaryDataTree(_pair, _left, node);
                        return true;
                    }
                    BinaryDataTree subRoot;
                    if (_right.TryAdd(node, keyComparer, out subRoot))
                    {
                        root = new BinaryDataTree(_pair, _left, subRoot);
                        return true;
                    }
                }
                root = this;
                return false;
            }

            internal bool TryRemove(TKey key, IComparer<TKey> keyComparer, out BinaryDataTree root)
            {
                var compareResult = keyComparer.Compare(_pair.Key, key);
                if (compareResult == 0)
                {
                    if (_left != null && _right != null)
                    {
                        var rightParent = _left;
                        while (rightParent._right != null)
                        {
                            rightParent = rightParent._right;
                        }
                        rightParent._right = _right;
                        root = _left;
                        return true;
                    }
                    if (_left == null && _right == null)
                    {
                        root = null;
                    }
                    else
                    {
                        root = _left??_right;
                    }
                    return true;
                }
                if (compareResult < 0)
                {
                    if (_left == null)
                    {
                        root = this;
                        return false;
                    }
                    BinaryDataTree subRoot;
                    if (_left.TryRemove(key, keyComparer, out subRoot))
                    {
                        root = new BinaryDataTree(_pair, subRoot, _right);
                        return true;
                    }
                }
                else if (compareResult > 0)
                {
                    if (_right == null)
                    {
                        root = this;
                        return false;
                    }
                    BinaryDataTree subRoot;
                    if (_right.TryRemove(key, keyComparer, out subRoot))
                    {
                        root = new BinaryDataTree(_pair, _left, subRoot);
                        return true;
                    }
                }
                root = this;
                return false;
            }

            internal int AddOrUpdate(BinaryDataTree node, IComparer<TKey> keyComparer, out BinaryDataTree root)
            {
                var compareResult = keyComparer.Compare(_pair.Key, node._pair.Key);
                if (compareResult<0)
                {
                    if (_left == null)
                    {
                        root = new BinaryDataTree(_pair, node, _right);
                        return 1;
                    }
                    BinaryDataTree subRoot;
                    var changeCount = _left.AddOrUpdate(node, keyComparer, out subRoot);
                    root = new BinaryDataTree(_pair, subRoot, _right);
                    return changeCount;
                }
                if (compareResult>0)
                {
                    if (_right == null)
                    {
                        root = new BinaryDataTree(_pair, _left, node);
                        return 1;
                    }
                    BinaryDataTree subRoot;
                    var changeCount = _right.AddOrUpdate(node, keyComparer, out subRoot);
                    root = new BinaryDataTree(_pair, _left, subRoot);
                    return changeCount;
                }
                node._right = _right;
                node._left = _left;
                root = node;
                return 0;
            }
        }
    }
}