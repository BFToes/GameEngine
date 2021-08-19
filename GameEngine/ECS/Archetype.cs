﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using ListExtensions;

namespace ECS
{
    /// <summary>
    /// A collection of <see cref="Entity"/> which all share the same types of <see cref="IComponent"/>.
    /// </summary>
    public partial class Archetype
    {
        /// <summary>
        /// the archetype for an entity with no components
        /// </summary>
        public readonly static Archetype Empty;
        /// <summary>
        /// a list of all archetypes currently in use
        /// </summary>
        internal readonly static List<Archetype> All;

        static Archetype()
        {
            All = new List<Archetype>();
            Empty = new Archetype();
        }

        /// <summary>
        /// The number of <see cref="Entity"/>s stored in this <see cref="Archetype"/>
        /// </summary>
        public int Length { get; private set; }

        private const int DEFAULT_ARRAY_SIZE = 128;
        private int _arraySize = DEFAULT_ARRAY_SIZE;

        private readonly IPool[] _pools;

        private readonly IReadOnlyList<byte> _compIDs; // components types contained in this Archetype
        private readonly IReadOnlyList<byte> _compIDLookUp; // a lookup for the index of the component's POOL AND NOT ID

        private readonly Archetype[] _next = new Archetype[byte.MaxValue]; // a graph to find the next archetype
        private readonly Archetype[] _prev = new Archetype[byte.MaxValue]; // a graph to find the prev archetype

        // constructor can be private because archetype its only called from Archetype.Get()
        private Archetype(params byte[] CompIDs)
        {
            Archetype.All.Add(this); // Add self to list of all archetypes
            this._compIDs = CompIDs; // should already be sorted

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
            //     if Archetype stores Behaviours we can do a more optimised search for 
            //     behaviours. how specific do we need search parameters? we could do a
            //     binary search on the behaviour's _allFilter quite easily
        }

        /// <summary>
        /// Searches All <see cref="Archetype"/>s for matching <paramref name="ComponentIDs"/>.
        /// if none found creates a new archetype matching the <paramref name="ComponentIDs"/>.
        /// </summary>
        /// <param name="ComponentIDs">the array of component IDs that this archetype uses.</param>
        /// <returns>an archetype that matches the description.</returns>
        public static Archetype Get(byte[] ComponentIDs)
        {
            // ToDo: Insertion Sort for arrays and behaviours
            //      then binary search instead of this VVV
            foreach (Archetype A in All) // we can do a better search than this
                if (ComponentIDs.SequenceEqual(A._compIDs)) // sequenceEqual bad
                    return A;
            
            return new Archetype(ComponentIDs);
        }

        /// <summary>
        /// Gets <see cref="Entity"/> <see cref="Pool{T}"/>.
        /// </summary>
        public Pool<Entity> GetPool()
        {
            return (Pool<Entity>)_pools[0];
        }

        /// <summary>
        /// Gets <typeparamref name="TComponent"/> pool and casts.
        /// </summary>
        public Pool<TComponent> GetPool<TComponent>() where TComponent : IComponent, new()
        {
            if (Contains(ComponentManager.ID<TComponent>(), out int index))
                return (Pool<TComponent>)_pools[index];
            else throw new ComponentNotFound();
            
        }

        /// <summary>
        /// Returns true if <paramref name="value"/> is contained within compIDs. 
        /// The <paramref name="index"/> is set to the index in <see cref="_pools"/>.
        /// </summary>
        private bool Contains(byte value, out int index) 
        {
            // re-structure _compIDs and Layer
            if (value >= _compIDLookUp.Count) 
            {
                index = -1;
                return false;
            }

            index = _compIDLookUp[value]; // -1 because of entity pool -> happy coincidence
            if (index == 0) return false;
            else return true;
        }

        #region Component Compare Operations
        /// <summary>
        /// returns true if All of the Components in <paramref name="CompIDs"/> appear in this archetype. 
        /// to skip check set <paramref name="CompIDs"/> to null.
        /// </summary>
        public bool HasAll(params byte[] CompIDs)
        {
            if (CompIDs == null) return true;
            foreach (byte ID in CompIDs)
                if (!Contains(ID, out _)) return false;
            return true;
        }
        /// <summary>
        /// returns true if Any Component in <paramref name="CompIDs"/>" appears in this archetype.
        /// to skip check set <paramref name="CompIDs"/> to null.
        /// </summary>
        public bool HasAny(params byte[] CompIDs)
        {
            if (CompIDs == null) return true;
            foreach (byte ID in CompIDs)
                if (Contains(ID, out _)) return true;
            return false;
        }
        /// <summary>
        /// returns true if None of the Components in <paramref name="CompIDs"/> appear in this archetype.
        /// to skip check set <paramref name="CompIDs"/> to null.
        /// </summary>
        public bool HasNone(params byte[] CompIDs)
        {
            if (CompIDs == null) return true;
            foreach (byte ID in CompIDs)
                if (Contains(ID, out _)) return false;
            return false;
        }
        #endregion


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
        /// <returns>an array of <see cref="IPoolable"/> corresponding to the removed layer</returns>
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

            // remove layer at end of pools
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


        
        internal void InitEntity(Entity entity, out int poolIndex)
        {
            IPoolable[] layer = new IPoolable[_pools.Length];
            layer[0] = entity;
            for (int i = 1; i < _pools.Length; i++)
                layer[i] = ComponentManager.InitComponent(_compIDs[i - 1]);
            AddLayer(out poolIndex, layer);
        }

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

        internal void MoveEntity(byte[] newCompIDs, ref int poolIndex, out Archetype archetype)
        {
            archetype = Get(newCompIDs);

            List<IPoolable> oldLayer = RemoveLayer(poolIndex);
            List<IPoolable> newLayer = new List<IPoolable>();

            newLayer.Add(oldLayer[0]); // copy entity over

            int new_i = 0, old_i = 0;  // component index
            while (new_i < archetype._compIDs.Count)
            {
                // because both arrays are sorted if one index overtakes the other
                // the values inbetween can be ignored
                while (++old_i < _compIDs.Count && _compIDs[old_i] < newCompIDs[new_i]); // components lost from old

                if (old_i < _compIDs.Count && _compIDs[old_i] == newCompIDs[new_i])
                    newLayer.Add(oldLayer[old_i + 1]);
                else
                    newLayer.Add(ComponentManager.InitComponent(newCompIDs[new_i]));

                new_i++;
            }

            archetype.AddLayer(out poolIndex, newLayer);
        }
        #endregion
    }
}