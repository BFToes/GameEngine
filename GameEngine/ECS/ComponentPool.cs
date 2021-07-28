using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.ECS
{
    /// <summary>
    /// interface for un-typed component creator
    /// </summary>
    interface IComponentPoolCreator
    {
        IComponentPool InstantiatePool();
        IComponent CreateComponent();
    }
    /// <summary>
    /// creates Component and Component pool
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ComponentPoolCreator<T> : IComponentPoolCreator where T : class, IComponent, new()
    {
        public IComponentPool InstantiatePool() => new ComponentPool<T>();
        public IComponent CreateComponent() => new T();
    }

    /// <summary>
    /// interface for un-typed component pool
    /// </summary>
    internal interface IComponentPool
    {
        void Add(int index, IComponent comp);
        void Replace(int freeIndex, int current);
        void Remove(int index);
        IComponent Get(int index);
        IComponent[] ToArray();
    }
    /// <summary>
    /// array for <see cref="IComponent"/> T
    /// </summary>
    /// <typeparam name="T">the <see cref="IComponent"/> stored in this pool</typeparam>
    internal class ComponentPool<T> : IComponentPool where T : class, IComponent, new()
    {
        private T[] Components = new T[1];

        public void Add(int index, IComponent comp)
        {
            if (index >= Components.Length)
                Array.Resize(ref Components, 2 * Components.Length);

            Components[index] = (T)comp;
        }
        public void Replace(int freeIndex, int current)
        {
            T comp = Components[current];
            Components[freeIndex] = comp;
            Components[current] = default;
        }
        public void Remove(int index) => Components[index] = default; // this isnt a very good solution
        public IComponent Get(int index) => Components[index];
        public T GetTyped(int index) => Components[index];
        public IComponent[] ToArray() => Components;
    }
}
