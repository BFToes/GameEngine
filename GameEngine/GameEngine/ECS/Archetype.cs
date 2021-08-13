using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ECS
{
    /// <summary>
    /// A collection of <see cref="Entity"/> which all share the same types of <see cref="IComponent"/>.
    /// </summary>
    public sealed class Archetype : IEnumerable<Entity>
    {
        private readonly Archetype[] Next = new Archetype[byte.MaxValue];
        private readonly Archetype[] Prior = new Archetype[byte.MaxValue];

        private readonly EntityContext _manager;
        internal readonly byte[] ComponentIDs;
        private readonly IComponentPool[] _componentPools;
        private Entity[] _entities = new Entity[1];


        public Entity this[int Index] { get => Index < EntityCount ? _entities[Index] : throw new IndexOutOfRangeException(); }
        public int EntityCount { private set; get; }

        internal Archetype(EntityContext Manager, params byte[] ComponentIDs)
        {
            _manager = Manager;

            this.ComponentIDs = ComponentIDs;
            Array.Sort(ComponentIDs); // Components must be sorted

            _componentPools = new IComponentPool[ComponentIDs.Length];
            for (int i = 0; i < ComponentIDs.Length; i++)
                _componentPools[i] = ComponentManager.CreatePool(ComponentIDs[i]);
        }

        /// <summary>
        /// Moves <see cref="Entity"/> to a new <see cref="Archetype"/>. 
        /// And copies <see cref="IComponent"/>s to new <see cref="Archetype"/>.
        /// </summary>
        /// <param name="Entity">the <see cref="Entity"/> to be moved in this <see cref="Archetype"/>.</param>
        /// <param name="Archetype">the <see cref="Archetype"/> the <see cref="Entity"/> will be moved to.</param>
        internal void MoveEntityTo(Entity Entity, Archetype Archetype)
        {
            int NewEntityIndex = Archetype.AddEntity(Entity);

            // copy components to new Archetype
            for (int i = 0; i < ComponentIDs.Length; i++)
            {
                IComponent Component = _componentPools[i][Entity.ArchetypeIndex];
                int ComponentIndex = FindComponent(ComponentIDs[i]);
                if (ComponentIndex != -1)
                    Archetype._componentPools[ComponentIndex][NewEntityIndex] = Component;
            }

            RemoveEntity(Entity);
            Entity.Archetype = Archetype;
            Entity.ArchetypeIndex = NewEntityIndex;
        }
        /// <summary>
        /// Adds the <see cref="Entity"/> to the archetype and generates <see cref="IComponentPool"/> 
        /// for the relevant <see cref="IComponent"/>.
        /// Does not change Entity's "ArchetypeIndex" and "Archetype" object pointer.
        /// </summary>
        /// <param name="Entity"></param>
        /// <returns>The index in the array of <see cref="Entity"/></returns>
        internal int AddEntity(Entity Entity)
        {
            // if entity array too small
            if (EntityCount == _entities.Length)
            {
                int newSize = _entities.Length << 1; // make bigger
                foreach (IComponentPool Pool in _componentPools)
                    Pool.Resize(newSize);

                Array.Resize(ref _entities, newSize);
            }
            // create component pools
            _entities[EntityCount] = Entity;
            for (int i = 0; i < ComponentIDs.Length; i++)
                _componentPools[i][EntityCount] = ComponentManager.CreateComponent(ComponentIDs[i]);

            return EntityCount++;
        }
        /// <summary>
        /// removes the <see cref="Entity"/> and <see cref="IComponentPool"/>s objects from this array.
        /// Does not change Entity's "ArchetypeIndex" and "Archetype" object pointer.
        /// </summary>
        internal void RemoveEntity(Entity Entity)
        {
            // if entity not at the end move it to the end
            if (Entity.ArchetypeIndex != EntityCount - 1)
            {
                foreach (IComponentPool Pool in _componentPools)
                    Pool.Replace(Entity.ArchetypeIndex, EntityCount);
                _entities[Entity.ArchetypeIndex] = _entities[EntityCount];
            }

            // remove entity at the end
            foreach (IComponentPool Pool in _componentPools)
                Pool.Clear(Entity.ArchetypeIndex);
            _entities[Entity.ArchetypeIndex] = null;

            // if entity array larger than needed resize array
            if (EntityCount < _entities.Length >> 1)
            {
                int newSize = _entities.Length >> 1; // make smaller
                foreach (IComponentPool Pool in _componentPools)
                    Pool.Resize(newSize);

                Array.Resize(ref _entities, newSize);
            }
            --EntityCount;                
        }
        
        /// <summary>
        /// Finds or creates the <see cref="Archetype"/> with the added component <paramref name="ComponentType"/>
        /// </summary>
        internal Archetype FindNext(byte ComponentType)
        {
            if (Next[ComponentType] != null)
                return Next[ComponentType];

            byte[] Components = ComponentIDs; // copy components
            Array.Resize(ref Components, Components.Length + 1); // resize

            int i = Components.Length - 2; // starting at one before the end
            while (i >= 0 && ComponentType < Components[i]) // shift elements along unit correct position found
                Components[i + 1] = Components[i--];
            Components[++i] = ComponentType; // add element in position found

            return Next[ComponentType] = _manager.FindOrCreateArchetype(Components);
        }
        /// <summary>
        /// Finds or creates the <see cref="Archetype"/> with out the removed component <paramref name="ComponentType"/>
        /// </summary>
        internal Archetype FindPrior(byte ComponentType)
        {
            if (Prior[ComponentType] != null)
                return Prior[ComponentType];

            byte[] Components = ComponentIDs; // copy components
            Array.Resize(ref Components, Components.Length - 1); // resize

            int i = ComponentIDs.Length - 1; // start at end
            while (i >= 0 && ComponentIDs[i] != ComponentType) // if keeping value
                Components[i - 1] = ComponentIDs[i--]; // shuffle value down 1 index, overwriting previous

            return Prior[ComponentType] = _manager.FindOrCreateArchetype(Components);
        }
        
        /// <summary>
        /// Gets the <see cref="IComponent"/> with ID <paramref name="ComponentID"/> on the <see cref="Entity"/> at <paramref name="Index"></paramref>.
        /// </summary>
        internal IComponent GetComponent(byte ComponentID, int Index)
        {
            int CompIndex = FindComponent(ComponentID);
            if (CompIndex == -1) throw new ComponentNotFound();
            return _componentPools[CompIndex][Index];
        }
        /// <summary>
        /// Gets the <see cref="IComponent"/>s related to <see cref="Entity"/> at <paramref name="Index">Archetype Index</paramref>.
        /// </summary>
        internal IEnumerable<IComponent> GetAllComponents(int Index)
        {
            for (int i = 0; i< _componentPools.Length; i++)
                yield return _componentPools[i][Index];
        }
        /// <summary>
        /// Gets the <see cref="ComponentPool{T}"/> for Component <paramref name="ComponentID"/>
        /// </summary>
        internal IComponentPool GetComponentPool(byte ComponentID)
        {
            int CompIndex = FindComponent(ComponentID);
            if (CompIndex == -1) throw new ComponentNotFound();
            return _componentPools[CompIndex];
        }
        /// <summary>
        /// Gets the <see cref="ComponentPool{T}"/> for Component <paramref name="ComponentID"/>
        /// </summary>
        public ComponentPool<TComponent> GetComponentPool<TComponent>() where TComponent : IComponent, new() => (ComponentPool<TComponent>)GetComponentPool(ComponentManager.ID<TComponent>());
        
        /// <summary>
        /// returns true if Archetype contains Component of type <typeparamref name="TComponent"/>.
        /// </summary>
        public bool Has<TComponent>() where TComponent : IComponent, new() => FindComponent(ComponentManager.ID<TComponent>()) != -1;
        /// <summary>
        /// returns true if Archetype contains Component of type <typeparamref name="TComponent"/>.
        /// </summary>
        /// <param name="Index">the entity index for <paramref name="Component"/></param>
        /// <param name="Component">the Component</param>
        /// <returns></returns>
        public bool Has<TComponent>(int Index, out TComponent Component) where TComponent : IComponent, new()
        {
            int CompIndex = FindComponent(ComponentManager.ID<TComponent>());
            if (CompIndex == -1) 
            {
                Component = default;
                return false;
            }
            else
            {
                Component = (TComponent)_componentPools[CompIndex][Index];
                return true;
            }
        }

        /// <summary>
        /// returns the index of the <paramref name="CompID"/>
        /// </summary>
        /// <param name="CompID"></param>
        /// <returns></returns>
        private int FindComponent(byte CompID) => Array.BinarySearch(ComponentIDs, 0, ComponentIDs.Length, CompID);

        internal bool Equals(byte[] ComponentIDs) => Equals(this.ComponentIDs, ComponentIDs);
        private static bool Equals(byte[] Left, byte[] Right)
        {
            if (Left.Length == Right.Length)
            {
                for (int i = 0; i < Left.Length; i++)
                    if (Left[i] != Right[i])
                        return false;
                return true;
            }
            return false;
        }

        public IEnumerator<Entity> GetEnumerator()
        {
            for (int i = 0; i < EntityCount; i++)
                yield return _entities[i];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

}
