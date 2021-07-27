using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace GameEngine.ECS
{
    public interface IEntity
    {
        uint ID { get; }
        int ComponentsCount { get; }
        T GetComponent<T>() where T : class, IComponent, new();
        bool HasComponent<T>() where T : class, IComponent, new();
        void AddComponent<T>() where T : class, IComponent, new();
        void RemoveComponent<T>() where T : class, IComponent, new();
        void Destroy();
    }


    public abstract class Entity : IEntity
    {
        public int ArchetypeIndex;
        public uint ID { get; private set; }
        public int ComponentsCount => throw new NotImplementedException();

        private Archetype _archetype;
        private readonly ArchetypeManager _archetypemanager;

        protected Entity(ArchetypeManager manager)
        {
            _archetypemanager = manager;
        }
        public void Initialize(uint ID)
        {
            this.ID = ID;
            _archetype = _archetypemanager.Empty;
            _archetype.AddEntity(this);
        }

        public void Initialize<T1>(uint ID, T1 Component1) 
            where T1 : class, IComponent, new()
        {
            this.ID = ID;

            byte i1 = ComponentType<T1>.Index;

            _archetype = _archetypemanager.FindOrCreateArchetype(i1);
            _archetype.AddComponent(i1, Component1);
            _archetype.AddEntity(this);
        }
        public void Initialize<T1, T2>(uint ID, T1 Component1, T2 Component2) 
            where T1 : class, IComponent, new()
            where T2 : class, IComponent, new()
        {
            this.ID = ID;

            byte i1 = ComponentType<T1>.Index;
            byte i2 = ComponentType<T2>.Index;

            _archetype = _archetypemanager.FindOrCreateArchetype(i1, i2);
            _archetype.AddComponent(i1, Component1);
            _archetype.AddComponent(i2, Component2);
            _archetype.AddEntity(this);
        }
        public void Initialize<T1, T2, T3>(uint ID, T1 Component1, T2 Component2, T3 Component3) 
            where T1 : class, IComponent, new()
            where T2 : class, IComponent, new()
            where T3 : class, IComponent, new()
        {
            this.ID = ID;

            byte i1 = ComponentType<T1>.Index;
            byte i2 = ComponentType<T2>.Index;
            byte i3 = ComponentType<T3>.Index;

            _archetype = _archetypemanager.FindOrCreateArchetype(i1, i2, i3);
            _archetype.AddComponent(i1, Component1);
            _archetype.AddComponent(i2, Component2);
            _archetype.AddComponent(i3, Component3);
            _archetype.AddEntity(this);
        }
        public void Initialize<T1, T2, T3, T4>(uint ID, T1 Component1, T2 Component2, T3 Component3, T4 Component4) 
            where T1 : class, IComponent, new()
            where T2 : class, IComponent, new()
            where T3 : class, IComponent, new()
            where T4 : class, IComponent, new()
        {
            this.ID = ID;

            byte i1 = ComponentType<T1>.Index;
            byte i2 = ComponentType<T2>.Index;
            byte i3 = ComponentType<T3>.Index;
            byte i4 = ComponentType<T4>.Index;

            _archetype = _archetypemanager.FindOrCreateArchetype(i1, i2, i3, i4);
            _archetype.AddComponent(i1, Component1);
            _archetype.AddComponent(i2, Component2);
            _archetype.AddComponent(i3, Component3);
            _archetype.AddComponent(i4, Component4);
            _archetype.AddEntity(this);
        }

        public void AddComponent<T>() where T : class, IComponent, new()
        {
            byte index = ComponentType<T>.Index;
            T component = new T();
            if (HasComponent(index) || component == null) throw new ArgumentException();
            AddComponent(index, component);
        }
        internal void AddComponent(byte index, IComponent component)
        {
            Archetype newArchetype = _archetypemanager.FindOrCreateNextArchetype(_archetype, index);
            foreach (byte curIndex in _archetype.Indices)
            {
                IComponentPool componentPool = _archetype.GetComponentPool(curIndex);
                newArchetype.AddComponent(curIndex, componentPool.Get(ArchetypeIndex));
            }

            newArchetype.AddComponent(index, component);

            _archetype.RemoveEntity(this);
            _archetype = newArchetype;
            _archetype.AddEntity(this);
        }
        public void RemoveComponent<TC>() where TC : class, IComponent, new()
        {
            byte index = ComponentType<TC>.Index;
            if (!HasComponent(index)) throw new InvalidOperationException();
            RemoveComponent(index);
        }
        internal void RemoveComponent(byte index)
        {
            Archetype newArchetype = _archetypemanager.FindOrCreatePriorArchetype(_archetype, index);
            foreach (byte curIndex in _archetype.Indices)
            {
                if (curIndex == index) continue;

                IComponentPool componentPool = _archetype.GetComponentPool(curIndex);
                newArchetype.AddComponent(curIndex, componentPool.Get(ArchetypeIndex));
            }

            _archetype.RemoveEntity(this);
            _archetype = newArchetype;
            _archetype.AddEntity(this);
        }

        public T GetComponent<T>() where T : class, IComponent, new()
        {
            if (!HasComponent<T>()) throw new InvalidOperationException();

            return _archetype.GetComponentPool<T>().GetTyped(ArchetypeIndex);
        }
        internal IComponent GetComponent(byte index) => ((IArchetype)_archetype).GetComponentPool(index).Get(ArchetypeIndex);

        public bool HasComponent<T>() where T : class, IComponent, new() => HasComponent(ComponentType<T>.Index);
        internal bool HasComponent(byte index) => _archetype.SetIndices.Contains(index);


        public void Destroy()
        {
            _archetype.RemoveEntity(this);
            _archetype = null;

            OnDestroy();
        }
        protected virtual void OnDestroy()
        {
        }
    }
}