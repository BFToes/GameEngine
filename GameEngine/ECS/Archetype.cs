﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ECS
{
    /// <summary>
    /// A collection of <see cref="Entity"/> which all share the same types of <see cref="IComponent"/>.
    /// </summary>
    internal partial class Archetype
    {
        public readonly static Archetype Empty;                                 // the archetype for an entity with no components
        private readonly static List<Archetype> All;                            // a list of all archetypes that have been created so far, sorted by "compSet"

        internal readonly ComponentSet compSet;                                 // a bit array represent which components this Archetype contains
        internal readonly IPool[] pools = new IPool[byte.MaxValue + 2];         // the storage for components

        private readonly Archetype[] _next = new Archetype[byte.MaxValue + 1];  // a graph to find the next archetype
        private readonly Archetype[] _prev = new Archetype[byte.MaxValue + 1];  // a graph to find the prev archetype

        /// <summary>
        /// The number of <see cref="Entity"/>s stored in this <see cref="Archetype"/>
        /// </summary>
        public int Length { get; private set; }
        
        private const int DEFAULT_ARRAY_SIZE = 128;
        private int _arraySize = DEFAULT_ARRAY_SIZE;            // the max number of entities before pools require resizing
        private const int ENTITY_POOL = byte.MaxValue + 1;      // index of entity pool

        static Archetype()
        {
            All = new List<Archetype>();
            Empty = new Archetype(new ComponentSet(new byte[0]));
            All.Add(Empty);
        }


        /// <summary>
        /// Searches All <see cref="Archetype"/>s for matching <paramref name="ComponentIDs"/>.
        /// if none are found, creates a new <see cref="Archetype"/> matching the <paramref name="ComponentIDs"/>.
        /// </summary>
        public static Archetype FindOrCreate(ComponentSet compSet)
        {
            int index = All.Search(compSet);
            if (index >= 0) return All[index];

            Archetype newArchetype = new Archetype(compSet);
            All.Insert(~index, newArchetype);

            return newArchetype;
        }
        // TODO rewrite summary
        internal Archetype FindNext(byte compID)
        {
            ComponentSet newCompSet = compSet.Add(compID);
            
            // look up
            if (_next[compID] == null) 
                _next[compID] = FindOrCreate(newCompSet);
            return _next[compID];
        }

        // TODO rewrite summary
        internal Archetype FindPrev(byte compID)
        {
            ComponentSet newCompSet = compSet.Remove(compID);
           
            // look up
            if (_prev[compID] == null)
                _prev[compID] = FindOrCreate(newCompSet);
            return _prev[compID];
        }

        // constructor can be private because archetype its only called from Archetype.FindOrCreate() and static constructor
        private Archetype(ComponentSet CompSet)
        {
            this.compSet = CompSet;

            foreach (byte CompID in compSet)
                pools[CompID] = ComponentManager.InitPool(CompID); // init component pools
            pools[ENTITY_POOL] = new Pool<Entity>();               // init entity pool


            // TODO: BEHAVIOUR SEARCH
            //     if Archetype stores Group we can do a more optimised search for 
            //     behaviours. how specific do we need search parameters? we could do a
            //     binary search on the Group's _allFilter quite easily
        }

       
        /// <summary>
        /// Gets <see cref="Entity"/> pool.
        /// </summary>
        public Pool<Entity> GetEntityPool()
        {
            return (Pool<Entity>)pools[byte.MaxValue + 1];
        }

        /// <summary>
        /// Gets <typeparamref name="TComponent"/> pool
        /// </summary>
        public Pool<TComponent> GetPool<TComponent>() where TComponent : IComponent, new()
        {
            return (Pool<TComponent>)pools[ComponentManager.ID<TComponent>()];
        }

        #region Constructing & Deconstructing Entity
        /// <summary>
        /// Initiates <see cref="Entity"/> into this Archetype
        /// </summary>
        internal void InitEntity(Entity entity, out int poolIndex)
        {
            if (Length == _arraySize)
            {
                _arraySize *= 2;
                pools[byte.MaxValue + 1].Resize(_arraySize);
                foreach (byte compID in compSet)
                    pools[compID].Resize(_arraySize);
            }
            
            pools[ENTITY_POOL][Length] = entity;
            foreach (byte compID in compSet)
                pools[compID][Length] = ComponentManager.InitComponent(compID);
            poolIndex = Length++;
        }
        
        /// <summary>
        /// Removes the <see cref="Entity"/> into this Archetype
        /// </summary>
        internal void DestroyEntity(int index)
        {
            // move end of pool to overwrite layer
            if (index != --Length)
                foreach(byte compID in compSet)
                    pools[compID][index] = pools[compID][Length];
            // entity has been moved so index must be updated 
            (pools[ENTITY_POOL][index] as Entity)._poolIndex = index;

            // remove layer at end of pools
            foreach(byte compID in compSet)
                pools[compID].Remove(Length);

            // resize if neccessary
            if (DEFAULT_ARRAY_SIZE < Length && Length < _arraySize / 2)
            {
                _arraySize /= 2;
                pools[byte.MaxValue + 1].Resize(_arraySize);
                foreach (byte compID in compSet)
                    pools[compID].Resize(_arraySize);
            }
        }
        #endregion

        #region Moving Entities
        /// <summary>
        /// moves the <see cref="Entity"/> at <paramref name="poolIndex"/> in this <see cref="Archetype"/> 
        /// to the given <paramref name="newArchetype"/>. 
        /// Copies <see cref="IComponent"/>s over. 
        /// Does not initialize any missing <see cref="IComponent"/>s.
        /// Does not change the value of _archetype in the <see cref="Entity"/>.
        /// </summary>
        internal Archetype MoveEntity(ref int poolIndex, Archetype newArchetype)
        {
            // increase size of new archetype if necessary
            if (newArchetype.Length == newArchetype._arraySize)
            {
                newArchetype._arraySize *= 2;
                newArchetype.pools[ENTITY_POOL].Resize(newArchetype._arraySize);
                foreach (byte compID in newArchetype.compSet)
                    newArchetype.pools[compID].Resize(newArchetype._arraySize);
            }
            
            // copy components and entity over
            newArchetype.pools[ENTITY_POOL][newArchetype.Length] = pools[ENTITY_POOL][poolIndex];
            foreach(byte compID in compSet)
                if (newArchetype.compSet.Contains(compID))
                    newArchetype.pools[compID][newArchetype.Length] = pools[compID][poolIndex];
            


            // remove component from 
            // move end of pool to overwrite layer
            if (poolIndex != --Length)
            {
                foreach(byte compID in compSet)
                    pools[compID][poolIndex] = pools[compID][Length];
                // entity has been moved so index must be updated 
                (pools[ENTITY_POOL][poolIndex] as Entity)._poolIndex = poolIndex;
            }
            

            // remove layer at end of pools
            foreach(byte compID in compSet)
                pools[compID].Remove(Length);

            // dcrease size of this archetype if neccessary
            if (DEFAULT_ARRAY_SIZE < Length && Length < _arraySize / 2)
            {
                _arraySize /= 2;
                pools[byte.MaxValue + 1].Resize(_arraySize);
                foreach (byte compID in compSet)
                    pools[compID].Resize(_arraySize);
            }

            poolIndex = newArchetype.Length;
            newArchetype.Length++;
            return newArchetype;
        }
        #endregion

        public override string ToString() // shows component array
        {
            return compSet.ToString();
        }
    }
}