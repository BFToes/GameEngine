﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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
            Empty = new Archetype();
            All = new List<Archetype>();
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


        internal Archetype(params byte[] CompIDs)
        {
            Archetype.All.Add(this); // Add self to list of all archetypes
            this._compIDs = CompIDs; // should already be sorted

            // set up look up table
            byte[] table = CompIDs.Length > 0 ? new byte[CompIDs[CompIDs.Length - 1] + 1] : new byte[0];
            for (byte i = 0; i < CompIDs.Length;)
                table[CompIDs[i]] = ++i;
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
            foreach (Archetype A in All) // we can do a better search than this
                if (A._compIDs == ComponentIDs)
                    return A;

            Archetype newArchetype = new Archetype(ComponentIDs);
            All.Add(newArchetype);
            return newArchetype;
        }

        /// <summary>
        /// Gets <see cref="Entity"/> <see cref="Pool{T}"/>.
        /// </summary>
        public Pool<Entity> GetPool()
        {
            return (Pool<Entity>)_pools[0];
        }

        /// <summary>
        /// Gets <typeparamref name="TComponent"/> of type T and casts <see cref="IPool"/> to <see cref="Pool{T}"><typeparamref name="TComponent"/> Pool</see>.
        /// </summary>
        public Pool<TComponent> GetPool<TComponent>() where TComponent : IComponent, new()
        {
            int index = _compIDLookUp[ComponentManager.ID<TComponent>()];
            if (index == 0) throw new ComponentNotFound();
            return (Pool<TComponent>)_pools[index];
        }

        /// <summary>
        /// gets all pools inside archetype
        /// </summary>
        /// <returns></returns>
        public IPool[] GetPools() => _pools;

        #region Component set operations
        /// <summary>
        /// returns true if All of the Components in <paramref name="CompIDs"/> appear in this archetype. 
        /// to skip check set <paramref name="CompIDs"/> to null.
        /// </summary>
        public bool HasAll(params byte[] CompIDs)
        {
            if (CompIDs == null) return true;
            foreach (byte ID in CompIDs)
                if (_compIDLookUp[ID] == 0) return false;
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
                if (_compIDLookUp[ID] != 0) return true;
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
                if (_compIDLookUp[ID] != 0) return false;
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
                _arraySize = Length << 1;
                for (int i = 0; i < _pools.Length; i++)
                    _pools[i].Resize(Length);
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

            // move layer to end of pools
            if (index != --Length)
                for (int i = 0; i < _pools.Length; i++)
                    _pools[i][index] = _pools[i][Length];

            // remove layer at end of pools
            for (int i = 0; i < _pools.Length; i++)
                _pools[i].Remove(Length);

            // resize if neccessary
            int newSize = _arraySize >> 1;
            if (Length < newSize && Length > DEFAULT_ARRAY_SIZE)
            {
                _arraySize = newSize;
                for (int i = 0; i < _pools.Length; i++)
                    _pools[i].Resize(newSize);
            }

            return Layer;
        }


        // should only be called by entity

        internal void InitEntity(Entity entity, out int poolIndex)
        {
            AddLayer(out poolIndex, new List<IPoolable>() { entity });
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
            List<IPoolable> Layer = RemoveLayer(poolIndex);
            List<byte> new_compIDs = new List<byte>(_compIDs);

            // re-structure _compIDs and Layer
            int comp_index = _compIDLookUp[compID] - 1; // -1 because of entity pool -> happy coincidence
            if (comp_index < 0) throw new ComponentNotFound();

            component = (IComponent)Layer[comp_index + 1];
            Layer.RemoveAt(comp_index + 1);
            new_compIDs.RemoveAt(comp_index);

            // look up
            if (_prev[compID] == null) // if look up doesnt exist 
                _prev[compID] = Get(new_compIDs.ToArray()); // expensive look up
            _prev[compID].AddLayer(out poolIndex, Layer);
            archetype = _prev[compID];
        }

        internal void MoveEntity(byte[] CompIDs, ref int poolIndex, out Archetype archetype)
        {
            archetype = Get(CompIDs);

            List<IPoolable> oldLayer = RemoveLayer(poolIndex);
            List<IPoolable> newLayer = new List<IPoolable>();

            newLayer.Add(oldLayer[0]); // set entity

            int new_i = 0, old_i = 0;  // component index
            do
            {
                IPoolable nextComp;
                // because both arrays are sorted if one index overtakes the other
                // the values inbetween can be ignored
                while (_compIDs[old_i] < archetype._compIDs[new_i]) old_i++; // components lost from old

                if (_compIDs[old_i] == archetype._compIDs[new_i])
                    nextComp = oldLayer[old_i + 1];
                else
                    nextComp = ComponentManager.InitComponent(_compIDs[new_i]);

                newLayer.Add(nextComp);
            }
            while (++new_i < archetype._compIDs.Count && old_i < _compIDs.Count);


            archetype.AddLayer(out poolIndex, newLayer);
        }
        #endregion
    }
}