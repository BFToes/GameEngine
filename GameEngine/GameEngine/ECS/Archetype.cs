using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ECS
{
    /// <summary>
    /// A collection of <see cref="Entity"/> which all share the same types of <see cref="IComponent"/>.
    /// </summary>
    public class Archetype
    {
        private EntityContext _context;
        private const int DEFAULT_ARRAY_SIZE = 128;
        private readonly IPool[] _pools;
        private readonly byte[] _compIDs;
        private int _arraySize;

        private Archetype[] _next = new Archetype[byte.MaxValue];
        private Archetype[] _prev = new Archetype[byte.MaxValue];

        public int Length { get; private set; }
        public Iitem this[int PoolIndex, int LayerIndex] { get => _pools[PoolIndex][LayerIndex]; }

        public Archetype(EntityContext Context, byte[] CompIDs)
        {
            this._context = Context;
            this._compIDs = CompIDs;
            _pools = CompIDs
                .OrderBy(compID => compID) // sort components
                .Select(ID => ComponentManager.InitPool(ID)) // create Component Pools
                .Prepend(new Pool<Entity>()) // add entity pool to start
                .ToArray(); // store as array
        }
        /// <summary>
        /// Adds a layer of <see cref="Entity"/> and <see cref="IComponent"/> into corresponding <see cref="IPool"/>.
        /// </summary>
        /// <param name="Layer">an entity followed by the individual components.
        /// <see cref="null"/> can be applied to any component which requires initiating.</param>
        internal void AddLayer(params Iitem[] Layer)
        {
            // resize if neccessary
            if (_arraySize == Length)
            {
                _arraySize = Length << 1;
                for (int i = 0; i < _pools.Length; i++)
                    _pools[i].Resize(Length);
            }

            _pools[0][Length] = Layer[0]; // set entity
            for (int i = 1; i < _pools.Length; i++) // set components
                _pools[i][Length] = Layer[i] ?? ComponentManager.InitComponent(_compIDs[i]);
        }
        /// <summary>
        /// Removes a layer of <see cref="Entity"/> and <see cref="IComponent"/> from corresponding <see cref="IPool"/>.
        /// </summary>
        /// <param name="index">the index of the layer.</param>
        /// <returns>an array of <see cref="Iitem"/> corresponding to the removed layer</returns>
        internal Iitem[] RemoveLayer(int index)
        {
            // copy layer
            Iitem[] Layer = new Iitem[_pools.Length];
            for (int i = 0; i < _pools.Length; i++)
                Layer[i] = _pools[i][index];

            // move layer to end of pools
            if (index != --Length)
                for (int i = 0; i < _pools.Length; i++)
                    _pools[i][index] = _pools[i][Length];

            // remove layer at end of pools
            for (int i = 0; i < _pools.Length; i++)
                _pools[i][Length] = default;

            // resize if neccessary
            int newSize = _arraySize >> 1;
            if (Length < newSize)
            {
                _arraySize = newSize;
                for (int i = 0; i < _pools.Length; i++)
                    _pools[i].Resize(newSize);
            }

            return Layer;
        }

        /// <summary>
        /// Moves <see cref="Entity"/> from this <see cref="Archetype"/> to an <see cref="Archetype"/> 
        /// with the added <see cref="IComponent"/> <paramref name="AddedComponent"/>
        /// </summary>
        internal void MoveEntity(Entity Entity, byte CompID, IComponent AddedComponent)
        {
            Iitem[] Layer = RemoveLayer(Entity.PoolIndex);
            byte[] newCompIDs = _compIDs;

            Array.Resize(ref newCompIDs, newCompIDs.Length + 1);
            Array.Resize(ref Layer, Layer.Length + 1);

            // re-arrangle Layer with added component

            // create new CompIDs and sort
            int i = newCompIDs.Length - 2; // starting at one before the end
            while (i >= 1 && CompID < newCompIDs[i]) // shift elements along unit correct position found
            {
                newCompIDs[i + 1] = newCompIDs[i];
                Layer[i + 1] = Layer[i--];
            }
            Layer[++i] = AddedComponent; // add element in position found
            newCompIDs[i] = CompID;


            // look up
            if (_next[CompID] == null) // if look up doesnt exist
                _next[CompID] = _context.FindOrCreateArchetype(newCompIDs); // expensive look up
            _next[CompID].AddLayer(Layer);

        }
        /// <summary>
        /// Moves <see cref="Entity"/> from this <see cref="Archetype"/> to an <see cref="Archetype"/> 
        /// with the removed <see cref="IComponent"/> <paramref name="AddedComponent"/>
        /// </summary>
        internal void MoveEntity(Entity Entity, byte CompID, out IComponent RemovedComponent)
        {
            Iitem[] Layer = RemoveLayer(Entity.PoolIndex);
            byte[] newCompIDs = _compIDs;

            // re-arrangle Layer with removed component
            int i = newCompIDs.Length - 1; // start at end
            byte tempID; // temp variables
            Iitem tempComp = default; // temp variable for layer will be component
            while (i >= 0 && newCompIDs[i] != CompID)
            {
                // layer +1 because Entity is at index 0
                tempComp = Layer[i + 1];
                Layer[i] = tempComp;

                tempID = newCompIDs[i--];
                newCompIDs[i] = tempID;

            }
            Array.Resize(ref newCompIDs, newCompIDs.Length - 1);
            Array.Resize(ref Layer, Layer.Length - 1);
            // loops through from back until CompID is found and shuffles all proceeding
            // items back one index overwriting the removed ID. Then resize arrays.

            RemovedComponent = (IComponent)tempComp;

            // look up
            if (_prev[CompID] == null) // if look up doesnt exist 
                _prev[CompID] = _context.FindOrCreateArchetype(newCompIDs); // expensive look up
            Archetype newArchetype = _prev[CompID];

            newArchetype.AddLayer(Layer);
        }
        /// <summary>
        /// Moves <see cref="Entity"/> from this <see cref="Archetype"/> to a specified <paramref name="Archetype"/>.
        /// The <see cref="IComponent"/> are copied over. Any <see cref="IComponent"/> not in the new <see cref="Archetype"/>
        /// are removed. Any <see cref="IComponent"/> not in the old <see cref="Archetype"/> are set to default
        /// </summary>
        internal void MoveEntity(Entity Entity, Archetype Archetype)
        {
            Iitem[] oldLayer = RemoveLayer(Entity.PoolIndex);
            Iitem[] newLayer = new Iitem[Archetype._compIDs.Length + 1];

            newLayer[0] = oldLayer[0];
            if (_compIDs.Length < Archetype._compIDs.Length) // loop through shorter array and copy Layer
                for (int c_i = 0; c_i < _compIDs.Length; c_i++)
                {
                    int new_i = Archetype.FindComponent(_compIDs[c_i]);
                    if (new_i != -1) // if Layer contains component
                        newLayer[new_i] = oldLayer[c_i + 1]; // copy component
                }
            else
                for (int c_i = 0; c_i < Archetype._compIDs.Length; c_i++)
                {
                    int old_i = Archetype.FindComponent(_compIDs[c_i]);
                    if (old_i != -1) // if Layer contains component
                        newLayer[c_i + 1] = oldLayer[old_i]; // copy component
                }
            Archetype.AddLayer(newLayer);
        }


        public Pool<TComponent> GetPool<TComponent>() where TComponent : IComponent, new() => (Pool<TComponent>)_pools[FindComponent<TComponent>()];
        public int FindComponent<T>() where T : IComponent, new() => FindComponent(ComponentManager.ID<T>());
        private int FindComponent(byte ID) => Array.BinarySearch(_compIDs, ID);
        public bool Equals(byte[] Components) => _compIDs == Components;



        /// <summary>
        /// <see cref="Archetype.Iitem"/> is an object thats stored in an <see cref="Archetype"/>.
        /// </summary>
        public interface Iitem
        {
            int PoolIndex { get; set; }
        }

        /// <summary>
        /// A simple array collection used in archetpye
        /// </summary>
        internal interface IPool
        {
            Iitem this[int index] { get; set; }
            void Resize(int newSize);
        }

        /// <summary>
        /// implements <see cref="IPool"/> 
        /// </summary>
        public class Pool<T> : IPool where T : Iitem
        {
            private T[] _array = new T[DEFAULT_ARRAY_SIZE];

            Iitem IPool.this[int index]
            {
                get => _array[index];
                set
                {
                    _array[index] = (T)value;
                    _array[index].PoolIndex = index;
                }
            }
            public T this[int index]
            {
                get => _array[index];
                set => _array[index] = value;
            }

            void IPool.Resize(int newSize) => Array.Resize(ref _array, newSize);
        }
    }
}
