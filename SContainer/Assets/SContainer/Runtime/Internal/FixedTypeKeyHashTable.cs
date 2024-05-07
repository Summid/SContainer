using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SContainer.Runtime.Internal
{
    // 实现类似 Dictionary<Type, TValue> 的功能
    public class FixedTypeKeyHashTable<TValue>
    {
        private readonly struct HashEntry
        {
            public readonly Type Type;
            public readonly TValue Value;

            public HashEntry(Type type, TValue value)
            {
                this.Type = type;
                this.Value = value;
            }
        }

        private readonly HashEntry[][] table;  // [hash][HashEntry]
        private readonly int indexFor;

        public FixedTypeKeyHashTable(KeyValuePair<Type, TValue>[] values, float loadFactor = 0.75f)
        {
            int initialCapacity = (int)(values.Length / loadFactor);

            // make power of 2 (and use mask)
            // see: Hashing https://en.wikipedia.org/wiki/Hash_table
            int capacity = 1;
            while (capacity < initialCapacity)  // 大于 initialCapacity 的最小 2 的幂
            {
                capacity <<= 1;
            }

            this.table = new HashEntry[capacity][];
            this.indexFor = this.table.Length - 1;  // mask
            
            foreach (var item in values)
            {
                var hash = RuntimeHelpers.GetHashCode(item.Key);
                var array = this.table[hash & this.indexFor];  // & indexFor 防止越界
                if (array == null)
                {
                    // 该 hash 下的 entry 数组未初始化
                    array = new HashEntry[1];
                    array[0] = new HashEntry(item.Key, item.Value);
                }
                else
                {
                    // 遇到 hash 冲突，扩容
                    var newArray = new HashEntry[array.Length + 1];
                    Array.Copy(array, newArray, array.Length);
                    array = newArray;
                    array[array.Length - 1] = new HashEntry(item.Key, item.Value);  // 把新 kvp 放到扩容出来的位置
                }

                this.table[hash & this.indexFor] = array;
            }
        }

        public TValue Get(Type type)
        {
            var hashCode = RuntimeHelpers.GetHashCode(type);
            var buckets = this.table[hashCode & this.indexFor];

            if (buckets == null) goto ERROR;

            if (buckets[0].Type == type)
            {
                return buckets[0].Value;
            }

            for (var i = 0; i < buckets.Length; i++)
            {
                if (buckets[i].Type == type)
                {
                    return buckets[i].Value;
                }
            }

        ERROR:
            throw new KeyNotFoundException("Type was not found, Type: " + type.FullName);
        }

        public bool TryGet(Type type, out TValue value)
        {
            var hashCode = RuntimeHelpers.GetHashCode(type);
            var buckets = this.table[hashCode & this.indexFor];

            if (buckets == null) goto END;

            if (buckets[0].Type == type)
            {
                value = buckets[0].Value;
                return true;
            }

            for (var i = 0; i < buckets.Length; i++)
            {
                if (buckets[i].Type == type)
                {
                    value = buckets[i].Value;
                    return true;
                }
            }

        END:
            value = default;
            return false;
        }
    }
}