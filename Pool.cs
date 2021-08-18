using System;
using System.Collections;
using System.Collections.Generic;

namespace ECS
{
    public partial class Archetype
    {
        /// <summary>
        /// <see cref="IPoolable"/> is an object thats stored in an <see cref="Pool{T}"/>.
        /// </summary>
        public interface IPoolable { }

        /// <summary>
        /// A resizable contiguous collection used in <see cref="Archetype"/>. 
        /// </summary>
        /// <remark>
        /// Resizing arrays is still kinda a dumb idea
        /// </remark>
        public interface IPool
        {
            IPoolable this[int index] { get; set; }
            internal void Resize(int newSize);
            internal void Remove(int index);
        }

        /// <summary>
        /// implements <see cref="IPool"/> 
        /// </summary>
        public class Pool<T> : IPool where T : IPoolable
        {
            private T[] _array = new T[DEFAULT_ARRAY_SIZE];
            public ref T this[int index] => ref _array[index];
            IPoolable IPool.this[int index]
            {
                get => _array[index];
                set => _array[index] = (T)value;
            }

            void IPool.Remove(int index) => _array[index] = default;
            void IPool.Resize(int newSize) => Array.Resize(ref _array, newSize);
        }
    }
}