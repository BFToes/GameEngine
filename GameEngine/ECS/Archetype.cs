﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

// Instead of resizing pools create new pool


namespace ECS
{
    /// <summary>
    /// A collection of <see cref="Entity"/> which all share the same types of <see cref="IComponent"/>.
    /// </summary>
    public partial class Archetype
    {
        public readonly static Archetype Empty; // the archetype for an entity with no components
        private readonly static List<Archetype> All; // a list of all archetypes that have been created so far

        private const int DEFAULT_ARRAY_SIZE = 128;
        private int _arraySize = DEFAULT_ARRAY_SIZE; // the max number of Entities before Archetype requires resize

        private readonly IPool[] _pools;

        internal readonly ComponentSet _compSet;
        private readonly IReadOnlyList<byte> _compIDs;      // components types contained in this Archetype
        private readonly IReadOnlyList<byte> _compIDLookUp; // a lookup for the index of the component's POOL AND NOT ID

        private readonly Archetype[] _next = new Archetype[byte.MaxValue]; // a graph to find the next archetype
        private readonly Archetype[] _prev = new Archetype[byte.MaxValue]; // a graph to find the prev archetype

        /// <summary>
        /// The number of <see cref="Entity"/>s stored in this <see cref="Archetype"/>
        /// </summary>
        public int Length { get; private set; }

        static Archetype()
        {
            All = new List<Archetype>();
            Empty = new Archetype(new byte[0]);
        }

        // constructor can be private because archetype its only called from Archetype.Get()
        private Archetype(byte[] CompIDs)
        {
            this._compIDs = CompIDs; // should already be sorted
            this._compSet = new ComponentSet(CompIDs); // doesnt need it to be sorted

            // set up look up table
            byte[] table = CompIDs.Length > 0 ? new byte[CompIDs[CompIDs.Length - 1] + 1] : new byte[0];
            for (byte i = 0; i < CompIDs.Length;)
                table[CompIDs[i]] = ++i; // the index in the pool not the ID
            this._compIDLookUp = table;

            // set up pools
            this._pools = new IPool[CompIDs.Length + 1];
            this._pools[0] = new Pool<Entity>();            // set entity pool
            for (int i = 0; i < CompIDs.Length; i++)        // set component pool
                _pools[i + 1] = ComponentManager.InitPool(CompIDs[i]);

            // ToDo: BEHAVIOUR SEARCH
            //     if Archetype stores Group we can do a more optimised search for 
            //     behaviours. how specific do we need search parameters? we could do a
            //     binary search on the Group's _allFilter quite easily
        }

        /// <summary>
        /// Searches All <see cref="Archetype"/>s for matching <paramref name="ComponentIDs"/>.
        /// if none are found, creates a new <see cref="Archetype"/> matching the <paramref name="ComponentIDs"/>.
        /// </summary>
        public static Archetype Get(byte[] ComponentIDs)
        {
            ComponentSet newCompSet = new ComponentSet(ComponentIDs);
            int index = All.Search(newCompSet);
            if (index >= 0) return All[index];

            Archetype newArchetype = new Archetype(ComponentIDs);
            All.Insert(~index, newArchetype);

            return newArchetype;
        }
        
        /// <summary>
        /// Gets <see cref="Entity"/> pool.
        /// </summary>
        public Pool<Entity> GetPool()
        {
            return (Pool<Entity>)_pools[0];
        }

        /// <summary>
        /// Gets <typeparamref name="TComponent"/> pool
        /// </summary>
        public Pool<TComponent> GetPool<TComponent>() where TComponent : IComponent, new()
        {
            if (Contains(ComponentManager.ID<TComponent>(), out int index))
                return (Pool<TComponent>)_pools[index];
            else throw new ComponentNotFound();
        }

        /// <summary>
        /// Returns true if <paramref name="value"/> is contained within compIDs. 
        /// The <paramref name="index"/> is set to the index of the Pools.
        /// </summary>
        internal bool Contains(byte value, out int index) 
        {
            if (value >= _compIDLookUp.Count) 
            {
                index = -1;
                return false;
            }

            index = _compIDLookUp[value];   // index 0 is never taken by a component
            if (index == 0) return false;   // it is always the entity pool
            else return true;
        }

        #region Adding and Removing Components
        /// <summary>
        /// Adds a layer of <see cref="Entity"/> and <see cref="IComponent">Components</see> into corresponding 
        /// <see cref="IPool"/>.
        /// </summary>
        private void AddLayer(out int index, IList<IPoolable> layer)
        {
            // resize if neccessary
            if (_arraySize == Length)
            {
                _arraySize = Length * 2;
                for (int i = 0; i < _pools.Length; i++)
                    _pools[i].Resize(_arraySize);
            }

            _pools[0][Length] = layer[0]; // set entity
            for (int i = 1; i < _pools.Length; i++) // set components
                _pools[i][Length] = layer[i];

            index = Length++;
        }




        /// <summary>
        /// Removes a layer of <see cref="Entity"/> and <see cref="IComponent"/> from corresponding <see cref="IPool"/>.
        /// </summary>
        private List<IPoolable> RemoveLayer(int index)
        {
            // copy layer
            List<IPoolable> Layer = new List<IPoolable>();
            for (int i = 0; i < _pools.Length; i++)
                Layer.Add(_pools[i][index]);

            // move end of pool to overwrite layer
            if (index != --Length)
                for (int i = 0; i < _pools.Length; i++)
                    _pools[i][index] = _pools[i][Length];
            // entity has been moved so index must be updated 
            (_pools[0][index] as Entity)._poolIndex = index; 

            // remove layer at end of pools -> unneccessary if unmanaged
            for (int i = 0; i < _pools.Length; i++)
                _pools[i].Remove(Length);

            // resize if neccessary
            int newSize = _arraySize / 2;
            if (DEFAULT_ARRAY_SIZE  < Length && Length < newSize)
            {
                _arraySize = newSize;
                for (int i = 0; i < _pools.Length; i++)
                    _pools[i].Resize(newSize);
            }

            return Layer;
        }




        /// <summary>
        /// Initiates <see cref="Entity"/> into this Archetype
        /// </summary>
        internal void InitEntity(Entity entity, out int poolIndex)
        {
            IPoolable[] layer = new IPoolable[_pools.Length];
            layer[0] = entity;
            for (int i = 1; i < _pools.Length; i++) // create a component 
                layer[i] = ComponentManager.InitComponent(_compIDs[i - 1]);
            AddLayer(out poolIndex, layer);
        }




        /// <summary>
        /// Moves an <see cref="Entity"/> in this <see cref="Archetype"/> at <paramref name="poolIndex"/> to a
        /// new <see cref="Archetype"/> with the same <see cref="IComponent"/>s plus the addition of <paramref name="component"/>
        /// </summary>
        internal void MoveEntity(byte compID, IComponent component, ref int poolIndex, out Archetype archetype)
        {
            List<IPoolable> Layer = RemoveLayer(poolIndex);
            List<byte> new_compIDs = new List<byte>(_compIDs);

            // sorted insert
            int index = new_compIDs.BinarySearch(compID);
            if (index < 0) index = ~index;
            else throw new ComponentAlreadyExist();
            // insert
            new_compIDs.Insert(index, compID);
            Layer.Insert(index + 1, component);

            // look up
            if (_next[compID] == null) // if look up doesnt exist
                _next[compID] = Get(new_compIDs.ToArray()); // expensive look up
            _next[compID].AddLayer(out poolIndex, Layer);
            archetype = _next[compID];
        }
        



        /// <summary>
        /// Moves an <see cref="Entity"/> in this <see cref="Archetype"/> at <paramref name="poolIndex"/> to a
        /// new <see cref="Archetype"/> with the same <see cref="IComponent"/>s minus the addition of <paramref name="component"/>
        /// </summary>
        internal void MoveEntity(byte compID, out IComponent component, ref int poolIndex, out Archetype archetype)
        {
            List<IPoolable> newLayer = RemoveLayer(poolIndex);
            List<byte> newcompIDs = new List<byte>(_compIDs);

            if (!Contains(compID, out int index)) 
                throw new ComponentNotFound();

            component = (IComponent)newLayer[index];
            newLayer.RemoveAt(index);
            newcompIDs.RemoveAt(index - 1);

            // look up
            if (_prev[compID] == null) // if look up doesnt exist 
                _prev[compID] = Get(newcompIDs.ToArray()); // expensive look up
            _prev[compID].AddLayer(out poolIndex, newLayer);
            archetype = _prev[compID];
        }
        #endregion
    }
}