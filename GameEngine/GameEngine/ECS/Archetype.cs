using System;
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

        static Archetype() {
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
        private readonly IReadOnlyList<byte> _compIDLookUp; // a lookup for the index of the component's pool

        private readonly Archetype[] _next = new Archetype[byte.MaxValue]; // a graph to find the next archetype
        private readonly Archetype[] _prev = new Archetype[byte.MaxValue]; // a graph to find the prev archetype


        internal Archetype(params byte[] CompIDs)
        {
            Archetype.All.Add(this);

            // set up componentIDs
            Array.Sort(CompIDs);
            this._compIDs = CompIDs;
            byte[] table = CompIDs.Length > 0 ? new byte[CompIDs[CompIDs.Length - 1] + 1] : new byte[0];
            for (byte i = 0; i < CompIDs.Length;)
                table[CompIDs[i]] = ++i;
            this._compIDLookUp = table;
            
            // Create Pools
            this._pools = new IPool[CompIDs.Length + 1];
            this._pools[0] = new Pool<Entity>(); // set entity pool
            for (int i = 0; i < CompIDs.Length; i++) // set component pool
                _pools[i + 1] = ComponentManager.InitPool(CompIDs[i]);

            // ToDo: BEHAVIOUR SEARCH
            //     if Archetype stores Behaviours we can do a more optimised search for 
            //     behaviours. how specific do we need search parameters? we could do a
            //     binary search array on the behaviours quite easily
            //foreach (Entity.Behaviour B in _behaviours)
            //    B.AddIfApplicable(New);
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
        
        
        

        /// <summary>
        /// Initiates <paramref name="Entity"/> with components into <see cref="Archetype"/>. 
        /// </summary>
        internal void InitEntity(Entity entity)
        {
            //Kinda unneccessary I could just exposes AddLayer() but i thought that looked messy. 
            // If this is done when the entity is already stored in an Archetype it will never 
            // get removed and would be a pretty serious memory leak. so like... dont?
            AddLayer(out int poolIndex, new List<IPoolable>() { entity });
            entity.SetArchetype(this, poolIndex);
        }

        /// <summary>
        /// Moves <see cref="Entity"/> from this <see cref="Archetype"/> to an <see cref="Archetype"/> 
        /// with the added <see cref="IComponent"/> <paramref name="component"/>
        /// </summary>
        internal void MoveEntity(in Entity entity, in byte compID, IComponent component)
        {
            List<IPoolable> Layer = RemoveLayer(entity._poolIndex);
            List<byte> new_compIDs = new List<byte>(_compIDs);

            // insertion and sort with new component
            int index = new_compIDs.BinarySearch(compID); 
            if (index < 0) index = ~index;
            else throw new ComponentAlreadyExist();
            
            new_compIDs.Insert(index, compID);
            Layer.Insert(index + 1, component);


            // look up
            if (_next[compID] == null) // if look up doesnt exist
                _next[compID] = Get(new_compIDs.ToArray()); // expensive look up
            _next[compID].AddLayer(out int poolIndex, Layer);
            entity.SetArchetype(_next[compID], poolIndex);
        }
        
        /// <summary>
        /// Moves <see cref="Entity"/> from this <see cref="Archetype"/> to an <see cref="Archetype"/> 
        /// without the removed <see cref="IComponent"/> <paramref name="RemovedComponent"/>
        /// </summary>
        internal void MoveEntity(in Entity entity, in byte compID, out IComponent component)
        {
            IList<IPoolable> Layer = RemoveLayer(entity._poolIndex);
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
            _prev[compID].AddLayer(out int poolIndex, Layer);
            entity.SetArchetype(_next[compID], poolIndex);
        }
        
        /// <summary>
        /// Moves <see cref="Entity"/> from this <see cref="Archetype"/> to a specified <paramref name="NewArchetype"/>.
        /// The <see cref="IComponent"/> are copied over. Any <see cref="IComponent"/> not in the new <see cref="Archetype"/>
        /// are removed. Any <see cref="IComponent"/> not in the old <see cref="Archetype"/> are set to default
        /// </summary>
        internal void MoveEntity(in Entity entity, in Archetype newArchetype, out List<IComponent> removedComponents, out List<IComponent> addedComponents)
        {
            List<IPoolable> oldLayer = RemoveLayer(entity._poolIndex);
            List<IPoolable> newLayer = new List<IPoolable>();

            removedComponents = new List<IComponent>();
            addedComponents = new List<IComponent>();

            newLayer.Add(oldLayer[0]); // set entity

            int new_i = 0, old_i = 0;  // component index
            do
            {
                IPoolable nextComp;
                // because both arrays are sorted if one index overtakes the other
                // the values inbetween can be ignored
                while (_compIDs[old_i] < newArchetype._compIDs[new_i])
                    removedComponents.Add((IComponent)oldLayer[++old_i]); 
                    // components lost from old
                
                if (_compIDs[old_i] == newArchetype._compIDs[new_i])
                {   // copied components from old to new
                    nextComp = oldLayer[old_i + 1]; 
                }
                else
                {   // components in new missing from old
                    nextComp = ComponentManager.InitComponent(_compIDs[new_i]);
                    addedComponents.Add((IComponent)nextComp);
                }
                newLayer.Add(nextComp);
            }
            while (++new_i < newArchetype._compIDs.Count && old_i < _compIDs.Count);


            newArchetype.AddLayer(out int poolIndex, newLayer);
            entity.SetArchetype(newArchetype, poolIndex);
        }
        #endregion
    }
}
