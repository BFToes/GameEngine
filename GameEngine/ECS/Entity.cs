using System;
using System.Collections.Generic;

namespace GameEngine.ECS
{

    public abstract class Entity
    {
        internal uint ID { get; private set; }
        
        internal int ArchetypeIndex;
        internal Archetype _archetype;

        private readonly World World;

        protected Entity(World World)
        {
            this.World = World;
        }
        internal void Initialize(uint ID)
        {
            this.ID = ID;
            _archetype = World.archetypeManager.Empty;
            _archetype.AddEntity(this);
        }

        internal void Initialize<T1>(uint ID, T1 Component1) 
            where T1 : class, IComponent, new()
        {
            this.ID = ID;

            byte i1 = ComponentType<T1>.ID;

            _archetype = World.archetypeManager.FindOrCreateArchetype(i1);
            _archetype.AddComponent(i1, Component1);
            _archetype.AddEntity(this);
        }
        internal void Initialize<T1, T2>(uint ID, T1 Component1, T2 Component2) 
            where T1 : class, IComponent, new()
            where T2 : class, IComponent, new()
        {
            this.ID = ID;

            byte i1 = ComponentType<T1>.ID;
            byte i2 = ComponentType<T2>.ID;

            _archetype = World.archetypeManager.FindOrCreateArchetype(i1, i2);
            _archetype.AddComponent(i1, Component1);
            _archetype.AddComponent(i2, Component2);
            _archetype.AddEntity(this);
        }
        internal void Initialize<T1, T2, T3>(uint ID, T1 Component1, T2 Component2, T3 Component3) 
            where T1 : class, IComponent, new()
            where T2 : class, IComponent, new()
            where T3 : class, IComponent, new()
        {
            this.ID = ID;

            byte i1 = ComponentType<T1>.ID;
            byte i2 = ComponentType<T2>.ID;
            byte i3 = ComponentType<T3>.ID;

            _archetype = World.archetypeManager.FindOrCreateArchetype(i1, i2, i3);
            _archetype.AddComponent(i1, Component1);
            _archetype.AddComponent(i2, Component2);
            _archetype.AddComponent(i3, Component3);
            _archetype.AddEntity(this);
        }
        internal void Initialize<T1, T2, T3, T4>(uint ID, T1 Component1, T2 Component2, T3 Component3, T4 Component4) 
            where T1 : class, IComponent, new()
            where T2 : class, IComponent, new()
            where T3 : class, IComponent, new()
            where T4 : class, IComponent, new()
        {
            this.ID = ID;

            byte i1 = ComponentType<T1>.ID;
            byte i2 = ComponentType<T2>.ID;
            byte i3 = ComponentType<T3>.ID;
            byte i4 = ComponentType<T4>.ID;

            _archetype = World.archetypeManager.FindOrCreateArchetype(i1, i2, i3, i4);
            _archetype.AddComponent(i1, Component1);
            _archetype.AddComponent(i2, Component2);
            _archetype.AddComponent(i3, Component3);
            _archetype.AddComponent(i4, Component4);
            _archetype.AddEntity(this);
        }

        public void AddComponent<T>() where T : class, IComponent, new()
        {
            byte index = ComponentType<T>.ID;
            T component = new T();
            if (HasComponent(index) || component == null) throw new ArgumentException();
            AddComponent(index, component);
        }
        internal void AddComponent(byte index, IComponent component)
        {
            Archetype newArchetype = World.archetypeManager.FindOrCreateNextArchetype(_archetype, index);
            foreach (byte curIndex in _archetype.ComponentIDs)
            {
                IComponent[] componentPool = _archetype.GetComponents(curIndex);
                newArchetype.AddComponent(curIndex, componentPool[ArchetypeIndex]);
            }

            newArchetype.AddComponent(index, component);

            _archetype.RemoveEntity(this);
            _archetype = newArchetype;
            _archetype.AddEntity(this);
        }
        
        public void RemoveComponent<T>() where T : class, IComponent, new()
        {
            byte index = ComponentType<T>.ID;
            if (!HasComponent(index)) throw new InvalidOperationException();
            RemoveComponent(index);
        }
        internal void RemoveComponent(byte index)
        {
            Archetype newArchetype = World.archetypeManager.FindOrCreatePriorArchetype(_archetype, index);
            foreach (byte curIndex in _archetype.ComponentIDs)
            {
                if (curIndex == index) continue;

                IComponent[] componentPool = _archetype.GetComponents(curIndex);
                newArchetype.AddComponent(curIndex, componentPool[ArchetypeIndex]);
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
        internal IComponent GetComponent(byte index) => _archetype.GetComponents(index)[ArchetypeIndex];

        public bool HasComponent<T>() where T : class, IComponent, new() => HasComponent(ComponentType<T>.ID);
        internal bool HasComponent(byte CompID) => Array.FindIndex(_archetype.ComponentIDs, B => B == CompID) != -1;

        public void Destroy()
        {
            World.RemoveEntity(this);
            _archetype = null;

            OnDestroy();
        }
        protected virtual void OnDestroy()
        {
        }

    }
}