using System;
using System.Collections.Generic;
using System.Text;

namespace ECS
{
    /// <summary>
    /// a simple collection of <see cref="IComponent"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal interface IComponentPool
    {
        /// <summary>
        /// The <see cref="Entity"/>'s <see cref="IComponent"/> at the Index
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        IComponent this[int Index] { get; }
        /// <summary>
        /// adds an item to the index specified
        /// </summary>
        /// <param name="item"></param>
        void Set(IComponent item, int Index);
        /// <summary>
        /// swaps the end of the array with the given index. then removes the end. 
        /// </summary>
        /// <param name="FreeIndex"></param>
        void Replace(int Index, int Length);
        /// <summary>
        /// Resizes the array to the specified size
        /// </summary>
        /// <param name="Size"></param>
        void Resize(int Size);
        /// <summary>
        /// Clears the last item in the pool
        /// </summary>
        void Clear(int Index);

    }
    /// <summary>
    /// Implements <see cref="IComponentPool"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ComponentPool<T> : IComponentPool where T : IComponent, new()
    {
        private T[] _array = new T[1];
        public IComponent this[int Index] { get => _array[Index]; }
        public void Set(IComponent item, int Index) => _array[Index] = (T)item;
        public void Replace(int FreeIndex, int Length) => _array[FreeIndex] = _array[Length];
        public void Resize(int Size) => Array.Resize(ref _array, Size);
        public void Clear(int Index) => _array[Index] = default;
    }
}
