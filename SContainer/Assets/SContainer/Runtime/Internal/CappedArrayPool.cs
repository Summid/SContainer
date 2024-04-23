using System;
using UnityEngine.UIElements;

namespace SContainer.Runtime.Internal
{
    /// <summary>
    /// 有上限的数组池，默认上限为8；当获取超过上限长度的数组时将直接向内存申请空间
    /// </summary>
    internal sealed class CappedArrayPool<T>
    {
        internal const int InitialBucketSize = 4;

        public static readonly CappedArrayPool<T> Shared8Limit = new CappedArrayPool<T>(8);
        
        private readonly T[][][] buckets;
        private readonly object syncRoot = new object();
        private readonly int[] tails;

        //      buckets[index][][]: index 既表索引，也表示在该索引下 bucket 的 array 长度为 index + 1
        //      bucket[0][1][2][3]  bucket[0][1][2][3] bucket[0][1][2][3]
        // pool length 1  1  1  1   length 2  2  2  2  length 3  3  3  3
        
        internal CappedArrayPool(int maxLength)
        {
            this.buckets = new T[maxLength][][];// maxLength (8) 个 bucket （二维数组）
            this.tails = new int[maxLength];
            for (var i = 0; i < maxLength; i++)
            {
                var arrayLength = i + 1;
                this.buckets[i] = new T[InitialBucketSize][];  // bucket 的初始容量为 InitialBucketSize，即存放 4 个数组，这 4 个数组长度相等，都为 arrayLength
                for (var j = 0; j < InitialBucketSize; j++)
                {
                    this.buckets[i][j] = new T[arrayLength];  // bucket 里存放的是数组（因为这是对象池类）
                }
                this.tails[i] = this.buckets[i].Length - 1;
                // this.tails[i] = 0;
            }
        }

        public T[] Rent(int length)
        {
            if (length <= 0)
                return Array.Empty<T>();

            if (length > this.buckets.Length)
                return new T[length];  // Not supported

            var i = length - 1;  // 长度为 length 的 bucket 索引

            lock (this.syncRoot)
            {
                var bucket = this.buckets[i];
                var tail = this.tails[i];
                if (tail >= bucket.Length)
                {
                    Array.Resize(ref bucket, bucket.Length * 2);
                    this.buckets[i] = bucket;
                }

                var result = bucket[tail] ?? new T[length];  // 扩容后需初始化
                this.tails[i] += 1;
                return result;
            }
        }

        public void Return(T[] array)
        {
            if (array.Length <= 0 || array.Length > this.buckets.Length)
                return;

            var i = array.Length - 1;
            lock (this.syncRoot)
            {
                Array.Clear(array, 0, array.Length);
                if (this.tails[i] > 0)
                    this.tails[i] -= 1;
            }
        }
    }
}