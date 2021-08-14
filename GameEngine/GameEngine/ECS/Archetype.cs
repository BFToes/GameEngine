using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ECS.Pool;
using System.Linq;
using ListExtensions;

namespace ECS
{
    /// <summary>
    /// A module of data that can attach to entities to provide functionality. 
    /// All data relating to an <see cref="Entity"/> is stored through <see cref="IComponent"/>s.
    /// </summary>
    public interface IComponent : IPoolItem { }

    /// <summary>
    /// A collection of <see cref="Entity"/> which all share the same types of <see cref="IComponent"/>.
    /// </summary>
    public sealed class Archetype : BundlePoolContainer, IPoolItem, IEquatable<Type[]>
    {
        /* the Linked pool collection is managed so that the first pool is the entity
         * and all following pools are for the seperate components.
         * 
         */
        public int ID { get; set; }
        private readonly EntityContext _manager;

        /// <summary>
        /// initialises <see cref="Archetype"/> with the <see cref="IComponent"/>s <paramref name="Types"/>
        /// </summary>
        internal Archetype(EntityContext Manager, List<Type> Types) : base(Types
            .AsParallel().OrderBy(Type => Type.GetHashCode()).Prepend(typeof(Entity))) // sorts components and adds Entity to first index
        {
            this._manager = Manager;
        }
        /// <summary>
        /// initialize empty <see cref="Archetype"/> with no <see cref="IComponent"/>.
        /// </summary>
        /// <param name="Manager"></param>
        internal Archetype(EntityContext Manager) : base(new Type[] { typeof(Entity) })
        {
            this._manager = Manager;
        }
        /// <summary>
        /// assigns <see cref="Entity"/> to this <see cref="Archetype"/> Creating new components.
        /// </summary>
        public void Add(Entity Entity)
        {
            int newIndex = AddLayer(false); // add new layer and initiate all but the first
            this[newIndex, 0] = Entity; // Assign Entity into the first index
        }
        /// <summary>
        /// Unassigns <see cref="Entity"/> from this <see cref="Archetype"/>, Deleting all components
        /// </summary>
        public void Remove(Entity Entity)
        {
            if (Entity == this[Entity.ID, 0]) // if entity matches entity in archetype
                RemoveLayer(Entity.ID); // remove all data relating to this entity
        }
        /// <summary>
        /// Moves <see cref="Entity"/> to a new <see cref="Archetype"/>
        /// and copies <see cref="IComponent"/>s to new <see cref="Archetype"/>.
        /// </summary>
        /// <param name="Entity">the <see cref="Entity"/> to be moved in this <see cref="Archetype"/>.</param>
        /// <param name="Archetype">the <see cref="Archetype"/> the <see cref="Entity"/> will be moved to.</param>
        internal void MoveEntityTo(Entity Entity, Archetype Archetype)
        {
            int newIndex = AddLayer();
            this[newIndex, 0] = Entity; // Assign Entity into the first
            IEnumerable<IPoolItem> Items = GetLayer(Entity.ID, false);
            // needs more bitmask data to match up types
            // SetLayer(Items, )
        }

        /// <summary>
        /// Finds or creates the <see cref="Archetype"/> with the added component <paramref name="component"/>
        /// </summary>
        internal Archetype FindNext(Type component)
        {
            Type[] newTypes = Types; // copy components
            Array.Resize(ref newTypes, newTypes.Length + 1); // resize

            // linear inserts Type into sorted list
            int i = newTypes.Length - 2; // starting at one before the end
            while (i >= 1 && component.GetHashCode() < Types[i].GetHashCode()) // shift elements along unit correct position found
                newTypes[i + 1] = Types[i--];
            newTypes[++i] = component; // add element in position found

            return _manager.FindOrCreateArchetype(newTypes);
        }
        /// <summary>
        /// Finds or creates the <see cref="Archetype"/> with out the removed component <paramref name="component"/>
        /// </summary>
        internal Archetype FindPrior(Type component)
        {
            Type[] newPoolItemTypes = new Type[Types.Length - 1];

            int i = newPoolItemTypes.Length - 1; // start at end
            while (i >= 1 && newPoolItemTypes[i] != component) // if keeping value
                Types[i - 1] = newPoolItemTypes[i--]; // shuffle value down 1 index, overwriting previous

            return _manager.FindOrCreateArchetype(newPoolItemTypes);
        }

        

        /// <summary>
        /// returns true if Archetype contains Component of type <typeparamref name="TComponent"/>.
        /// </summary>
        public bool Has<TComponent>() where TComponent : IComponent => FindType<TComponent>() != -1;
        /// <summary>
        /// returns true if Archetype contains Component of type <typeparamref name="TComponent"/>
        /// with out variable for the component.
        /// </summary>
        public bool Has<TComponent>(int Index, out TComponent Component) where TComponent : IComponent, new()
        {
            int CompIndex = FindType<TComponent>();
            if (CompIndex == -1) 
            {
                Component = default;
                return false;
            }
            else
            {
                Component = (TComponent)this[CompIndex, Index];
                return true;
            }
        }

        bool IEquatable<Type[]>.Equals(Type[] otherTypes) => Types == otherTypes;
    }
}
