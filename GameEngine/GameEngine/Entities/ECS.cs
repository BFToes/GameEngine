using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECS
{


    public class Filter<T>
    {
        private HashSet<T> Any;
        private HashSet<T> All;
        private HashSet<T> None;

        public void AnyOf(params T[] Items) => Any = new HashSet<T>(Items);
        public void AllOf(params T[] Items) => All = new HashSet<T>(Items);
        public void NoneOf(params T[] Items) => None = new HashSet<T>(Items);

        public static bool AnyCheck(HashSet<T> Any, T[] Items)
        {
            if (Any.Count < 1) 
                return true;
            foreach(T Item in Items)
                if (Any.Contains(Item)) 
                    return true;
            return false;
        }
        public static bool AllCheck(HashSet<T> All, T[] Items)
        {
            if (All.Count < 1)
                return true;
            HashSet<T> _items = new HashSet<T>(Items);
            foreach (T Item in All)
                if (!_items.Contains(Item)) return false;
                else continue;
            return true;
        }
        public static bool NoneCheck(HashSet<T> None, T[] Items)
        {
            if (None.Count < 1)
                return true;
            HashSet<T> _items = new HashSet<T>(Items);
            foreach (T Item in None)
                if (_items.Contains(Item)) return false;
                else continue;
            return true;
        }
        public bool FilterCheck(params T[] Items) => AnyCheck(Any, Items) && AllCheck(All, Items) && NoneCheck(None, Items);

    }

    /// <summary>
    /// A collection of <see cref="Archetype"/>s which fulfils a <see cref="Filter{T}"/> condition. 
    /// Used to perform logic over filtered selection of <see cref="Entity"/>.
    /// </summary>
    public class Behaviour 
    {
        public Behaviour(Filter<byte> Filter) { }
    
    }
    
    
    /// <summary>
    /// A collection of <see cref="Entity"/> which all share the same types of <see cref="IComponent"/>.
    /// </summary>
    public class Archetype
    {
        private int EntityCount;
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


            Console.Write($"New Archetype : ");
            foreach (byte C in ComponentIDs)
                Console.Write($"{C}, ");
            Console.Write("\n");
        }

        internal void Add(Entity Entity)
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
        internal void Remove(Entity Entity)
        {
            EntityCount--;
            int EntityIndex = Array.FindIndex(Entities, E => E == Entity);
            if (EntityIndex < 0) return;

            if (EntityIndex == EntityCount)
            {
                foreach (IComponentPool Pool in ComponentPools)
                    Pool.Clear(EntityIndex);
                Entities[EntityIndex] = null;
            }
            else
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
            if (EntityCount == 0)
                Manager.Remove(this);

        }
        internal bool Has(byte Component) => Array.FindIndex(ComponentIDs, ID => ID == Component) != -1;

        internal T GetComponent<T>(Entity Entity) where T : IComponent, new()
        {
            int EntityIndex = Array.FindIndex(Entities, E => E == Entity);
            byte CompID = ComponentManager.ID<T>();

            for (int i = 0; i < ComponentIDs.Length; i++) // linear search for componentID
                if (CompID == ComponentIDs[i])
                    return (T)ComponentPools[i][EntityIndex];




            throw new Exception("Component Not Found");
        }
        internal Archetype FindNext(byte Component) => Manager.NextArchetype(this, Component);
        internal Archetype FindPrior(byte Component) => Manager.PriorArchetype(this, Component);

        public bool Has<T>() where T : IComponent, new() => Has(ComponentType<T>.ID);

        public bool Equals(byte[] ComponentIDs) => Equals(this.ComponentIDs, ComponentIDs);
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
    }

    /// <summary>
    /// a local context of entities with behaviour systems and archetypes. 
    /// </summary>
    public abstract class EntityContext 
    {
        private readonly List<Entity> _entities = new List<Entity>();
        private readonly List<Archetype> _archetypes = new List<Archetype>();
        private readonly List<Behaviour> _behaviours = new List<Behaviour>();

        internal Archetype Empty;

        protected EntityContext()
        {
            Empty = new Archetype(this);
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
        internal Archetype PriorArchetype(Archetype Archetype, byte Component) 
        {
            // rearrange component array
            byte[] Components = new byte[Archetype.ComponentIDs.Length - 1];
            int i = 0;
            foreach (byte C in Archetype.ComponentIDs) 
                if (C != Component) Components[i++] = C;

            return FindOrCreateArchetype(Components);
        }
        internal Archetype NextArchetype(Archetype Archetype, byte Component) 
        {
            // rearrange component array
            byte[] ComponentIDs = Archetype.ComponentIDs;
            int i = Archetype.ComponentIDs.Length - 1; // start at the end
            Array.Resize(ref ComponentIDs, ComponentIDs.Length + 1); // make space for new element
            while (i >= 0 && Component < ComponentIDs[i]) 
                ComponentIDs[i + 1] = ComponentIDs[i--]; // loop down shifting elements right

            ComponentIDs[++i] = Component; // add element in position found

            return FindOrCreateArchetype(ComponentIDs);
        }
        internal void DebugArchetypes()
        {

            int i = 0;
            Console.WriteLine($"Component Type Count : {ComponentManager._length}");
            while (i < ComponentManager._length)
                Console.WriteLine($"ComponentType {i}: {ComponentManager.Types[i++]}");

            i = 0;
            Console.WriteLine($"Archetype Count : {_archetypes.Count}");
            foreach (Archetype A in _archetypes)
            {
                Console.Write($"Archetype {i++}: ");
                foreach (byte C in A.ComponentIDs)
                    Console.Write($"{C}, ");
                Console.Write("\n");
            }
            i = 0;
            Console.WriteLine($"Entity Count : {_entities.Count}");
            foreach (Entity E in _entities)
            {
                Console.WriteLine($"Entity {i++}: {E}");
                Console.Write("\n");
            }
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
    /// <summary>
    /// A container object for which <see cref="IComponent"/>s can be added and removed.
    /// </summary>
    public abstract class Entity
    {
        protected internal EntityContext Manager { get; internal set; }
        private Archetype Archetype;

        protected Entity(EntityContext Manager)
        {
            this.Manager = Manager;
            this.Archetype = Manager.Empty;
            Manager.AddEntity(this);
        }

        protected void AddComponent<TComponent>() where TComponent : IComponent, new()
        {
            Archetype.Remove(this);
            Archetype = Archetype.FindNext(ComponentType<TComponent>.ID);
            Archetype.Add(this);

        }
        protected void RemoveComponent<TComponent>() where TComponent : IComponent, new()
        {
            Archetype.Remove(this);
            Archetype = Archetype.FindPrior(ComponentType<TComponent>.ID);
            Archetype.Add(this);
        }
        public TComponent GetComponent<TComponent>() where TComponent : IComponent, new() => Archetype.GetComponent<TComponent>(this);
        public bool HasComponent<TComponent>() where TComponent : IComponent, new() => Archetype.Has(ComponentType<TComponent>.ID);
    }

    
}
