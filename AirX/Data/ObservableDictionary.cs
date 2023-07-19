using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace AirX.Data
{
    /// <summary>
    /// Map数据结构，又称字典，并且支持每当数据变化时通知外部。
    /// Map就是键值对的集合，一个键对应一个值。
    /// 继承自C#自带的Dictionary（也就直接从Dictionary继承了一个字典的实现）。
    /// </summary>
    public class ObservableDictionary<K, V> : Dictionary<K, V>, IObservableMap<K, V>
    {
        /// <summary>
        /// event非常类似于订阅者模式，理解为需要通知多人的回调函数。
        /// 具体可参见mac端的 `AirXApp.swift`
        /// </summary>
        public event MapChangedEventHandler<K, V> MapChanged;

        /// 覆盖Dictionary原先的索引器，使得每当索引器被调用时，都会触发MapChanged事件。
        new public V this[K key]
        {
            get => base[key];
            set
            {
                base[key] = value;

                // 做完实际行为（base[key]=value）后，调用MapChanged事件。
                MapChanged?.Invoke(this, new MapChangedEventArgs<K>(
                    CollectionChange.ItemChanged, key));
            }
        }

        /// 覆盖Dictionary原先的Add方法，使得每当Add方法被调用时，都会触发MapChanged事件。
        new public void Add(K key, V value)
        {
            base.Add(key, value);
            MapChanged?.Invoke(this, new MapChangedEventArgs<K>(
                CollectionChange.ItemInserted, key));
        }

        /// 覆盖Dictionary原先的Remove方法，使得每当Remove方法被调用时，都会触发MapChanged事件。
        new public bool Remove(K key)
        {
            bool removed = base.Remove(key);
            if (removed)
            {
                MapChanged?.Invoke(this, new MapChangedEventArgs<K>(
                    CollectionChange.ItemRemoved, key));
            }
            return removed;
        }

        /// 向外部开放主动无条件触发一次MapChanged事件的办法。
        public void TriggerNotifyChanged()
        {
            MapChanged?.Invoke(this, new MapChangedEventArgs<K>(
                CollectionChange.Reset, default));
        }
    }

    /// 为了让map改动能够通知到外部，需要自定义MapChangedEventArgs
    /// 属于是C#的固定搭配
    public class MapChangedEventArgs<K> : IMapChangedEventArgs<K>
    {
        public CollectionChange _changeType;
        public CollectionChange CollectionChange => _changeType;

        public K _changedKey;
        public K Key => _changedKey;

        public MapChangedEventArgs(CollectionChange changeType, K key)
        {
            _changeType = changeType;
            _changedKey = key;
        }
    }
}
