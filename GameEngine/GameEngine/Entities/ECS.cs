using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ECS
{
    /// <summary>
    /// A collection of <see cref="Entity"/> which all share the same types of <see cref="IComponent"/>.
    /// </summary>
    public class Archetype : IEnumerable<Entity>
    {
        private readonly EntityContext _manager;
        private readonly byte[] _componentIDs;
        private readonly IComponentPool[] _componentPools;
        private Entity[] _entities = new Entity[1];


        public Entity this[int Index] { get => Index < EntityCount ? _entities[Index] : throw new IndexOutOfRangeException(); }
        public int EntityCount { private set; get; }

        internal Archetype(EntityContext Manager, params byte[] ComponentIDs)
        {
            this._manager = Manager;

            this._componentIDs = ComponentIDs;
            Array.Sort(ComponentIDs); // Components must be sorted

            this._componentPools = new IComponentPool[ComponentIDs.Length];

            for (int i = 0; i < ComponentIDs.Length; i++)
                _componentPools[i] = ComponentManager.CreatePool(ComponentIDs[i]);
        }

        /// <summary>
        /// Moves <see cref="Entity"/> to a new <see cref="Archetype"/>. 
        /// Copies <see cref="IComponent"/>s to new <see cref="Archetype"/>.
        /// </summary>
        /// <param name="Entity">the <see cref="Entity"/> to be moved in this <see cref="Archetype"/>.</param>
        /// <param name="Archetype">the <see cref="Archetype"/> the <see cref="Entity"/> will be moved to.</param>
        internal void MoveEntityTo(Entity Entity, Archetype Archetype)
        {
            int NewEntityIndex = Archetype.Add(Entity);

            // copy components to new Archetype
            for (int i = 0; i < _componentIDs.Length; i++)
            {
                IComponent Component = _componentPools[i][Entity.ArchetypeIndex];
                int ComponentIndex = Array.FindIndex(Archetype._componentIDs, ID => ID == _componentIDs[i]);
                if (ComponentIndex != -1) 
                    Archetype._componentPools[ComponentIndex][NewEntityIndex] = Component;
            }
            
            Remove(Entity);
            Entity.Archetype = Archetype;
            Entity.ArchetypeIndex = NewEntityIndex;
        }
        /// <summary>
        /// Adds the <see cref="Entity"/> to the archetype and generates <see cref="IComponentPool"/> for the relevant <see cref="IComponent"/>.
        /// Does not change Entity's "ArchetypeIndex" and "Archetype" object pointer.
        /// </summary>
        /// <param name="Entity"></param>
        /// <returns>The index in the array of <see cref="Entity"/></returns>
        internal int Add(Entity Entity)
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
            for (int i = 0; i < _componentIDs.Length; i++)
                _componentPools[i][EntityCount] = ComponentManager.CreateComponent(_componentIDs[i]);
            return EntityCount++;
        }
        /// <summary>
        /// removes the <see cref="Entity"/> and <see cref="IComponentPool"/>s objects from this array.
        /// Does not change Entity's "ArchetypeIndex" and "Archetype" object pointer.
        internal void Remove(Entity Entity)
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
            
            // if no entities in list, cease existing
            if (--EntityCount == 0)
                _manager.Remove(this);
        }

        /// <summary>
        /// Finds or creates the <see cref="Archetype"/> with the added component <paramref name="ComponentType"/>
        /// </summary>
        internal Archetype FindNext(byte ComponentType)
        {
            byte[] Components = _componentIDs; // copy components
            Array.Resize(ref Components, Components.Length + 1); // resize

            int i = Components.Length - 2; // starting at one before the end
            while (i >= 0 && ComponentType < Components[i]) // shift elements along unit correct position found
                Components[i + 1] = Components[i--];
            Components[++i] = ComponentType; // add element in position found

            return _manager.FindOrCreateArchetype(Components);
        }
        /// <summary>
        /// Finds or creates the <see cref="Archetype"/> without <paramref name="ComponentType"/>
        /// </summary>
        internal Archetype FindPrior(byte ComponentType)
        {
            byte[] Components = _componentIDs; // copy components
            Array.Resize(ref Components, Components.Length - 1); // resize

            int i = _componentIDs.Length - 1; // start at end
            while (i >= 0 && _componentIDs[i] != ComponentType) // if keeping value
                Components[i - 1] = _componentIDs[i--]; // shuffle value down 1 index, overwriting previous

            return _manager.FindOrCreateArchetype(Components);
        }

        /// <summary>
        /// gets the component of type <typeparamref name="TComponent"/> for the Entity at <paramref name="Index"/>
        /// </summary>
        internal TComponent GetComponent<TComponent>(int Index) where TComponent : IComponent, new()
        {
            int CompIndex = Array.FindIndex(_componentIDs , ID => ID == ComponentManager.ID<TComponent>());
            return (TComponent)_componentPools[CompIndex][Index];
        }
        /// <summary>
        /// gets the <see cref="IComponentPool"/> of type <typeparamref name="TComponent"/>
        /// </summary>
        internal ComponentPool<TComponent> GetComponentPool<TComponent>() where TComponent : IComponent, new()
        {
            int CompIndex = Array.FindIndex(_componentIDs , ID => ID == ComponentManager.ID<TComponent>());
            return (ComponentPool<TComponent>)_componentPools[CompIndex];
        }

        /// <summary>
        /// returns true if Archetype has Component of type <typeparamref name="TComponent"/>
        /// </summary>
        public bool Has<TComponent>() where TComponent : IComponent, new() => Array.FindIndex(_componentIDs, ID => ID == ComponentType<TComponent>.ID) != -1;

        internal void WriteDebug()
        {
            Console.Write("Archetype : ");
            foreach (byte C in _componentIDs)
                Console.Write($"{C}, ");
            Console.Write("\n");
        }

        internal bool Equals(byte[] ComponentIDs) => Equals(this._componentIDs, ComponentIDs);
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

    /// <summary>
    /// a local context of entities with behaviour systems and archetypes. 
    /// </summary>
    public abstract class EntityContext 
    {
        private readonly List<Archetype> _archetypes = new List<Archetype>();
        private readonly List<BaseBehaviour> _behaviours = new List<BaseBehaviour>();

        internal Archetype EmptyArchetype;

        protected EntityContext()
        {
            EmptyArchetype = new Archetype(this);
        }

        public void AddBehaviour(BaseBehaviour Behaviour) => _behaviours.Add(Behaviour);
        public void RemoveBehaviour(BaseBehaviour Behaviour) => _behaviours.Remove(Behaviour);

        internal void Remove(Archetype Archetype)
        {
            _archetypes.Remove(Archetype);
        }
        internal Archetype FindOrCreateArchetype(byte[] Components)
        {
            foreach(Archetype A in _archetypes)
            {
                if (A.Equals(Components)) 
                    return A;
            }
            Archetype New = new Archetype(this, Components);
            _archetypes.Add(New);

            foreach (BaseBehaviour B in _behaviours)
                if (B.Filter.Check(Components))
                    B.Add(New);

            return New;
        }

        internal void Debug()
        {
            Console.WriteLine("Components:");
            ComponentManager.WriteDebug();
            Console.WriteLine("Archetypes:");
            foreach(Archetype A in GetArchetypes())
            {
                Console.Write($"Archetype :");
                A.WriteDebug();
            }

        }

        internal IEnumerable<Archetype> GetArchetypes()
        {
            foreach (Archetype A in _archetypes)
                yield return A;
        }
        internal IEnumerable<BaseBehaviour> GetBehaviours()
        {
            foreach (BaseBehaviour B in _behaviours)
                yield return B;
        }
        internal IEnumerable<Entity> GetEntities()
        {
            foreach (Archetype A in _archetypes)
                foreach (Entity E in A)
                    yield return E;
        }
    }
    
    /// <summary>
    /// A container object for which <see cref="IComponent"/>s can be added and removed.
    /// </summary>
    public abstract class Entity
    {
        protected internal EntityContext Manager { get; internal set; }
        internal Archetype Archetype;
        internal int ArchetypeIndex = -1;

        protected Entity(EntityContext Manager)
        {
            this.Manager = Manager;
            this.Archetype = Manager.EmptyArchetype;
            ArchetypeIndex = Manager.EmptyArchetype.Add(this);
        }

        protected TComponent AddComponent<TComponent>() where TComponent : IComponent, new()
        {
            Archetype.MoveEntityTo(this, Archetype.FindNext(ComponentType<TComponent>.ID));
            TComponent Component = Archetype.GetComponent<TComponent>(ArchetypeIndex);
            return Component;
        }
        protected TComponent RemoveComponent<TComponent>() where TComponent : IComponent, new()
        {
            TComponent Component = Archetype.GetComponent<TComponent>(ArchetypeIndex);
            Archetype.MoveEntityTo(this, Archetype.FindPrior(ComponentType<TComponent>.ID));
            return Component;
        }
        public TComponent GetComponent<TComponent>() where TComponent : IComponent, new() => Archetype.GetComponent<TComponent>(ArchetypeIndex);
        public bool HasComponent<TComponent>() where TComponent : IComponent, new() => Archetype.Has<TComponent>();
    }
}
