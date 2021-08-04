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
        private readonly EntityContext Manager;
        private readonly byte[] ComponentIDs;
        private readonly IComponentPool[] ComponentPools;
        private Entity[] Entities = new Entity[1];
        public int EntityCount { private set; get; }

        internal Archetype(EntityContext Manager, params byte[] ComponentIDs)
        {
            this.Manager = Manager;

            this.ComponentIDs = ComponentIDs;
            Array.Sort(ComponentIDs); // Components must be sorted

            this.ComponentPools = new IComponentPool[ComponentIDs.Length];

            for (int i = 0; i < ComponentIDs.Length; i++)
                ComponentPools[i] = ComponentManager.CreatePool(ComponentIDs[i]);
        }

        internal void MoveEntityTo(Entity Entity, Archetype Archetype)
        {
            int NewEntityIndex = Archetype.Add(Entity);

            // copy components to new Archetype
            for (int i = 0; i < ComponentIDs.Length; i++)
            {
                IComponent Component = ComponentPools[i][Entity.ArchetypeIndex];
                int ComponentIndex = Array.FindIndex(Archetype.ComponentIDs, ID => ID == ComponentIDs[i]);
                Archetype.ComponentPools[ComponentIndex][NewEntityIndex] = Component;
            }
            
            Remove(Entity);
            Entity.Archetype = Archetype;
            Entity.ArchetypeIndex = NewEntityIndex;
        }

        internal int Add(Entity Entity)
        {
            // if entity array too small
            if (EntityCount == Entities.Length)
            {
                int newSize = Entities.Length << 1; // make bigger
                foreach (IComponentPool Pool in ComponentPools)
                    Pool.Resize(newSize);

                Array.Resize(ref Entities, newSize);
            }
            // create component pools
            Entities[EntityCount] = Entity;
            for (int i = 0; i < ComponentIDs.Length; i++)
                ComponentPools[i][EntityCount] = ComponentManager.CreateComponent(ComponentIDs[i]);
            return EntityCount++;
        }
        internal void Remove(Entity Entity)
        {
            // if entity not at the end move it to the end
            if (Entity.ArchetypeIndex != EntityCount)
            {
                foreach (IComponentPool Pool in ComponentPools)
                    Pool.Replace(Entity.ArchetypeIndex, EntityCount);
                Entities[Entity.ArchetypeIndex] = Entities[EntityCount];
            }

            // remove entity at the end
            foreach (IComponentPool Pool in ComponentPools)
                Pool.Clear(Entity.ArchetypeIndex);
            Entities[Entity.ArchetypeIndex] = null;

            // if entity array larger than needed resize array
            if (EntityCount < Entities.Length >> 1)
            {
                int newSize = Entities.Length >> 1; // make smaller
                foreach (IComponentPool Pool in ComponentPools)
                    Pool.Resize(newSize);

                Array.Resize(ref Entities, newSize);
            }
            
            // if no entities in list, cease existing
            if (EntityCount == 0)
                Manager.Remove(this);
        }
        internal Entity GetEntity(int Index) => Entities[Index];
        internal T GetComponent<T>(int Index) where T : IComponent, new()
        {
            int CompIndex = Array.FindIndex(ComponentIDs , ID => ID == ComponentManager.ID<T>());
            return (T)ComponentPools[CompIndex][Index];
        }
        internal ComponentPool<T> GetComponentPool<T>() where T : IComponent, new()
        {
            int CompIndex = Array.FindIndex(ComponentIDs , ID => ID == ComponentManager.ID<T>());
            return (ComponentPool<T>)ComponentPools[CompIndex];
        }

        internal Archetype FindNext(byte Component)
        {
            // rearrange component array
            byte[] Components = ComponentIDs;
            Array.Resize(ref Components, Components.Length + 1); // make space for new element

            int i = Components.Length - 2; // starting at one before the end
            while (i >= 0 && Component < Components[i]) // shift elements along unit correct position found
                Components[i + 1] = Components[i--];  
            Components[++i] = Component; // add element in position found

            return Manager.FindOrCreateArchetype(Components);
        }
        internal Archetype FindPrior(byte Component)
        {
            // rearrange component array
            byte[] Components = new byte[ComponentIDs.Length - 1];
            int i = 0;
            foreach (byte C in ComponentIDs)
                if (C != Component) Components[i++] = C;

            return Manager.FindOrCreateArchetype(Components);
        }
        
        public bool Has<T>() where T : IComponent, new() => Array.FindIndex(ComponentIDs, ID => ID == ComponentType<T>.ID) != -1;

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
                yield return Entities[i];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// a local context of entities with behaviour systems and archetypes. 
    /// </summary>
    public abstract class EntityContext 
    {
        private readonly List<Archetype> _archetypes = new List<Archetype>();
        private readonly List<Behaviour> _behaviours = new List<Behaviour>();

        internal Archetype EmptyArchetype;

        protected EntityContext()
        {
            EmptyArchetype = new Archetype(this);
        }

        internal void Remove(Archetype Archetype) => _archetypes.Remove(Archetype);
        internal Archetype FindOrCreateArchetype(byte[] Components)
        {
            foreach(Archetype A in _archetypes)
            {
                if (A.Equals(Components)) 
                    return A;
            }
            Archetype New = new Archetype(this, Components);
            _archetypes.Add(New);
            return New;
        }

        internal void Debug()
        {

            int i = 0;
            Console.WriteLine($"Component Type Count : {ComponentManager._length}");
            while (i < ComponentManager._length)
                Console.WriteLine($"ComponentType {i}: {ComponentManager.Types[i++]}");
            /*
            i = 0;
            Console.WriteLine($"Archetype Count : {_archetypes.Count}");
            foreach (Archetype A in _archetypes)
            {
                Console.Write($"Archetype {i++}: ");
                foreach (byte C in A.ComponentIDs)
                    Console.Write($"{C}, ");
                Console.Write("\n");
            }
            */
        }

        internal IEnumerable<Archetype> GetArchetypes()
        {
            foreach (Archetype A in _archetypes)
                yield return A;
        }
        internal IEnumerable<Entity> GetEntities()
        {
            foreach (Archetype A in _archetypes)
                foreach (Entity E in A)
                yield return E;
        }
        internal IEnumerable<Behaviour> GetBehaviours()
        {
            foreach (Behaviour B in _behaviours)
                yield return B;
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
            TComponent Component = Archetype.GetComponent<TComponent>(ArchetypeIndex);
            Archetype.MoveEntityTo(this, Archetype.FindNext(ComponentType<TComponent>.ID));
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
