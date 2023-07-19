using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Business
{
    public class ObjectPool<T> where T : IPoolObject
    {
        public List<ObjectPoolContainer<T>> List => _list;
        private readonly List<ObjectPoolContainer<T>> _list;
        private readonly Dictionary<T, ObjectPoolContainer<T>> _lookup;
        private readonly Func<UniTask<T>> _factoryFunc;
        private int _lastIndex = 0;

        public ObjectPool(Func<UniTask<T>> factoryFunc, int initialSize)
        {
            _factoryFunc = factoryFunc;

            _list = new List<ObjectPoolContainer<T>>(initialSize);
            _lookup = new Dictionary<T, ObjectPoolContainer<T>>(initialSize);
        }

        public async UniTask Warm(int capacity)
        {
            for (int i = 0; i < capacity; i++)
            {
                await CreateContainer();
            }
        }

        private async UniTask<ObjectPoolContainer<T>> CreateContainer()
        {
            var container = new ObjectPoolContainer<T>
            {
                Item = await _factoryFunc()
            };
            _list.Add(container);
            return container;
        }

        public async UniTask<T> GetItem()
        {
            ObjectPoolContainer<T> container = null;
            for (int i = 0; i < _list.Count; i++)
            {
                _lastIndex++;
                if (_lastIndex > _list.Count - 1) _lastIndex = 0;

                if (_list[_lastIndex].Used)
                {
                    continue;
                }
                else
                {
                    container = _list[_lastIndex];
                    break;
                }
            }

            if (container == null)
            {
                container = await CreateContainer();
            }

            container.Consume();
            _lookup.Add(container.Item, container);
            return container.Item;
        }

        public void ReleaseItem(object item)
        {
            ReleaseItem((T)item);
        }

        public void ReleaseItem(T item)
        {
            if (_lookup.ContainsKey(item))
            {
                var container = _lookup[item];
                container.Release();
                _lookup.Remove(item);
            }
            else
            {
                Debug.LogWarning("This object pool does not contain the item provided: " + item);
            }
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public int CountUsedItems
        {
            get { return _lookup.Count; }
        }

        public void Clear()
        {
            foreach (var obj in _list)
                obj.Item.Destroy();
            _list.Clear();
        }

        public record ObjectPoolContainer<TRecord>(bool Used = false)
        {
            private TRecord _item;

            public bool Used { get; private set; } = Used;

            public void Consume()
            {
                Used = true;
            }

            public TRecord Item
            {
                get
                {
                    return _item;
                }
                set
                {
                    _item = value;
                }
            }

            public void Release()
            {
                Used = false;
            }
        }
    }
}
