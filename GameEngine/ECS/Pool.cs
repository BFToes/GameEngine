using System;
using System.Collections;
using System.Collections.Generic;

namespace ECS
{
    /// <summary>
    /// <see cref="IPoolable"/> is an object thats stored in an <see cref="Pool{T}"/>.
    /// </summary>
    public interface IPoolable { }

    internal partial class Archetype
    {
        /// <summary>
        /// A resizable contiguous collection used in <see cref="Archetype"/>. 
        /// </summary>
        internal interface IPool
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

            public override string ToString()
            {
                return $"Pool<{typeof(T).Name}>[{_array.Length}]";
            }
        }
    }
}