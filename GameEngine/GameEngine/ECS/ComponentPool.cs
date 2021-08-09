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
        internal IComponent this[int Index] { get; set; }
        /// <summary>
        /// swaps the end of the array with the given index. then removes the end. 
        /// </summary>
        /// <param name="FreeIndex"></param>
        internal void Replace(int Index, int Length);
        /// <summary>
        /// Resizes the array to the specified size
        /// </summary>
        /// <param name="Size"></param>
        internal void Resize(int Size);
        /// <summary>
        /// Clears the last item in the pool
        /// </summary>
        internal void Clear(int Index);

    }
    /// <summary>
    /// Implements <see cref="IComponentPool"/>, a simple collection of <typeparamref name="TComponent"/>
    /// </summary>
    /// <typeparam name="TComponent"></typeparam>
    public sealed class ComponentPool<TComponent> : IComponentPool where TComponent : IComponent, new()
    {
        private TComponent[] _array = new TComponent[1];
        public TComponent this[int Index]
        {
            get => _array[Index];
            set => _array[Index] = value;
        }

        IComponent IComponentPool.this[int Index] 
        {
            get => _array[Index];
            set => _array[Index] = (TComponent)value;
        }
        void IComponentPool.Replace(int Index, int Length) => _array[Index] = _array[Length];
        void IComponentPool.Resize(int Size) => Array.Resize(ref _array, Size);
        void IComponentPool.Clear(int Index) => _array[Index] = default;
    }
}
