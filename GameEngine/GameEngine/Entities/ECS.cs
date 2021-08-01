using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ECS
{
    /// <summary>
    /// A collection of <see cref="Entity"/> which all share the same types of <see cref="IComponent"/>.
    /// </summary>
    public class Archetype
    {
        private uint EntityCount;
        private readonly IComponentPool[] ComponentPools;
        private Entity[] Entities = new Entity[1];
        private readonly EntityContext Manager;
        internal readonly byte[] ComponentIDs;

        internal Archetype(EntityContext Manager, params byte[] ComponentIDs)
        {
            this.Manager = Manager;

            this.ComponentIDs = ComponentIDs; 
            Array.Sort(ComponentIDs); // Components must be sorted

            this.ComponentPools = new IComponentPool[ComponentIDs.Length];
            
            for (int i = 0; i < ComponentIDs.Length; i++)
                ComponentPools[i] = ComponentManager.CreatePool(ComponentIDs[i]);
        }

        public void Add(Entity Entity)
        {
            if (EntityCount == Entities.Length)
            {
                int newSize = Entities.Length << 1; // make bigger
                foreach (IComponentPool Pool in ComponentPools)
                    Pool.Resize(newSize);

                Array.Resize(ref Entities, newSize);
            }

            Entities[EntityCount] = Entity;
            for (int i = 0; i < ComponentIDs.Length; i++)
                ComponentPools[i].Set(ComponentManager.CreateComponent(ComponentIDs[i]), EntityCount);
            EntityCount++;
        }
        public void Remove(Entity Entity)
        {
            EntityCount--;
            int EntityIndex = Array.FindIndex(Entities, E => E == Entity);
            if (EntityIndex < 0) return;

            if (EntityIndex != EntityCount) 
            {
                foreach (IComponentPool Pool in ComponentPools)
                    Pool.Replace(EntityIndex, EntityCount);

                Entities[EntityIndex] = Entities[EntityCount]; 
            }
            
            if (EntityCount < Entities.Length >> 1)
            {
                int newSize = Entities.Length >> 1; // make smaller
                foreach (IComponentPool Pool in ComponentPools)
                    Pool.Resize(newSize);

                Array.Resize(ref Entities, newSize);
            }
        }
        public bool Has<T>() where T : IComponent, new() => Has(ComponentType<T>.ID);
        internal bool Has(byte Component)
        {
            if (Array.FindIndex(ComponentIDs, ID => ID == Component) != -1) return true;
            return false;
        }
        public T GetComponent<T>(Entity Entity) where T : IComponent, new()
        {
            int EntityIndex = Array.FindIndex(Entities, E => E == Entity);
            byte CompID = ComponentManager.ID<T>();
            for (int i = 0; i < ComponentIDs.Length; i++) // find entity index
                if (CompID == ComponentIDs[i])
                    return (T)ComponentPools[i][EntityIndex];
            throw new Exception("Component Not Found");
        }
        internal Archetype FindNext(byte Component) => Manager.NextArchetype(this, Component);
        internal Archetype FindPrior(byte Component) => Manager.NextArchetype(this, Component);
    }

    /// <summary>
    /// a local context of entities with behaviour systems and archetypes. 
    /// </summary>
    public abstract class EntityContext 
    {
        private readonly List<Entity> _entities = new List<Entity>();
        private readonly List<Archetype> _archetypes = new List<Archetype>();

        internal Archetype Empty => new Archetype(this);
        public Archetype GetArchetype(byte[] Components)
        {
            foreach(Archetype A in _archetypes)
            {
                if (A.ComponentIDs == Components)
                    return A;
            }
            return new Archetype(this, Components);
        }
        internal Archetype PriorArchetype(Archetype Archetype, byte Component) 
        {
            // rearrange component array
            byte[] Components = new byte[Archetype.ComponentIDs.Length - 1];
            int i = 0;
            foreach (byte C in Archetype.ComponentIDs) 
                if (C != Component) Components[i++] = C;

            return GetArchetype(Components);
        }
        internal Archetype NextArchetype(Archetype Archetype, byte Component) 
        {
            // rearrange component array
            byte[] ComponentIDs = Archetype.ComponentIDs;
            int i = Archetype.ComponentIDs.Length - 1; // start at the end
            Array.Resize(ref ComponentIDs, ComponentIDs.Length + 1); // make space for new element
            while (i >= 0 && Component < ComponentIDs[i]) ComponentIDs[i + 1] = ComponentIDs[i]; // loop down shifting elements right

            Console.WriteLine(ComponentIDs);
            
            ComponentIDs[++i] = Component; // add element in position found

            return GetArchetype(ComponentIDs);
        }
        internal void AddEntity(Entity E)
        {
            E.Manager = this;
            _entities.Add(E);
        }
        internal void RemoveEntity(Entity E)
        {
            E.Manager = this;
            _entities.Add(E);
        }
    }

    public abstract class Entity
    {
        protected internal EntityContext Manager { get; internal set; }
        private Archetype Archetype;

        protected Entity(EntityContext Manager)
        {
            this.Manager = Manager;
        }

        protected void AddComponent<T>() where T : IComponent, new()
        {
            Archetype.Remove(this);
            Archetype = Archetype.FindNext(ComponentType<T>.ID);
            Archetype.Add(this);

        }
        protected void RemoveComponent<T>() where T : IComponent, new()
        {
            Archetype.Remove(this);
            Archetype = Archetype.FindPrior(ComponentType<T>.ID);
            Archetype.Add(this);
        }
        public TComponent GetComponent<TComponent>() where TComponent : IComponent, new() => Archetype.GetComponent<TComponent>(this);
        public bool HasComponent<TComponent>() where TComponent : IComponent, new() => Archetype.Has(ComponentType<TComponent>.ID);
    }

    
}
