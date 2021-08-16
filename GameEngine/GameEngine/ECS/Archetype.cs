using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ListExtensions;

namespace ECS
{
    public abstract partial class Entity
    {
        /// <summary>
        /// A collection of <see cref="Entity"/> which all share the same types of <see cref="IComponent"/>.
        /// </summary>
        public class Archetype : IEnumerable<Entity>
        {
            private const int DEFAULT_ARRAY_SIZE = 128;
            private readonly EntityContext _context;
            private readonly IPool[] _pools;
            private readonly byte[] _compIDs;
            private readonly Archetype[] _next = new Archetype[byte.MaxValue];
            private readonly Archetype[] _prev = new Archetype[byte.MaxValue];

            private int _arraySize = DEFAULT_ARRAY_SIZE;
            public int Length { get; private set; }

            public Archetype(EntityContext Context, params byte[] CompIDs)
            {
                Array.Sort(CompIDs);
                this._context = Context;
                this._compIDs = CompIDs;
                this._pools = new IPool[CompIDs.Length + 1];
                this._pools[0] = new Pool<Entity>(); // set entity pool
                for (int i = 0; i < CompIDs.Length; i++) // set component pool
                    _pools[i + 1] = ComponentManager.InitPool(CompIDs[i]);
            }



            #region Move Entity
            /// <summary>
            /// Initiates Entity with components into archetype. Exposes <see cref="AddLayer(out int, IList{IPoolable})"/>
            /// </summary>
            internal void InitEntity(out int PoolIndex, Entity Entity)
            {
                AddLayer(out PoolIndex, new List<IPoolable>() { Entity });
            }
            
            /// <summary>
            /// Adds a layer of <see cref="Entity"/> and <see cref="IComponent"/> into corresponding 
            /// <see cref="IPool"/>. Called privately and in <see cref="Entity"/> only.
            /// </summary>
            /// <param name="Layer">an entity followed by the individual components.
            /// <see cref="null"/> can be applied to any component which requires initiating.</param>
            private void AddLayer(out int index, IList<IPoolable> Layer)
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

                index = Length++;
            }
            
            /// <summary>
            /// Removes a layer of <see cref="Entity"/> and <see cref="IComponent"/> from corresponding <see cref="IPool"/>.
            /// </summary>
            /// <param name="index">the index of the layer.</param>
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
            /// Moves <see cref="Entity"/> from this <see cref="Archetype"/> to an <see cref="Archetype"/> 
            /// with the added <see cref="IComponent"/> <paramref name="AddedComponent"/>
            /// </summary>
            internal void MoveEntity(in Entity Entity, in byte CompID, IComponent Component)
            {
                List<IPoolable> Layer = RemoveLayer(Entity._poolIndex);
                List<byte> new_compIDs = new List<byte>(_compIDs);

                int index = new_compIDs.BinarySearch(CompID);
                if (index < 0) index = ~index;
                new_compIDs.Insert(index, CompID);
                Layer.Insert(index + 1, Component);


                // look up
                if (_next[CompID] == null) // if look up doesnt exist
                    _next[CompID] = _context.FindOrCreateArchetype(new_compIDs.ToArray()); // expensive look up
                _next[CompID].AddLayer(out Entity._poolIndex, Layer);
                Entity._archetype = _next[CompID];
            }
            
            /// <summary>
            /// Moves <see cref="Entity"/> from this <see cref="Archetype"/> to an <see cref="Archetype"/> 
            /// with the removed <see cref="IComponent"/> <paramref name="AddedComponent"/>
            /// </summary>
            internal void MoveEntity(in Entity Entity, in byte CompID, out IComponent RemovedComponent)
            {
                IList<IPoolable> Layer = RemoveLayer(Entity._poolIndex);
                List<byte> new_compIDs = new List<byte>(_compIDs);

                // re-structure _compIDs and Layer
                int comp_index = new_compIDs.BinarySearch(CompID);
                RemovedComponent = (IComponent)Layer[comp_index + 1];
                Layer.RemoveAt(comp_index + 1);
                new_compIDs.RemoveAt(comp_index);

                // look up
                if (_prev[CompID] == null) // if look up doesnt exist 
                    _prev[CompID] = _context.FindOrCreateArchetype(new_compIDs.ToArray()); // expensive look up
                _prev[CompID].AddLayer(out Entity._poolIndex, Layer);
                Entity._archetype = _prev[CompID];
            }
            
            /// <summary>
            /// Moves <see cref="Entity"/> from this <see cref="Archetype"/> to a specified <paramref name="NewArchetype"/>.
            /// The <see cref="IComponent"/> are copied over. Any <see cref="IComponent"/> not in the new <see cref="Archetype"/>
            /// are removed. Any <see cref="IComponent"/> not in the old <see cref="Archetype"/> are set to default
            /// </summary>
            internal void MoveEntity(in Entity Entity, in Archetype NewArchetype)
            {
                IList<IPoolable> oldLayer = RemoveLayer(Entity._poolIndex);
                IPoolable[] newLayer = new IPoolable[NewArchetype._compIDs.Length + 1];

                newLayer[0] = oldLayer[0]; // set entity

                int new_i = 0, old_i = 0;
                do
                {
                    while (_compIDs[old_i] < NewArchetype._compIDs[new_i]) old_i++;
                    if (_compIDs[old_i] == NewArchetype._compIDs[new_i])
                        newLayer[new_i + 1] = oldLayer[old_i + 1];
                }
                while (++new_i < NewArchetype._compIDs.Length && old_i < _compIDs.Length);


                NewArchetype.AddLayer(out Entity._poolIndex, newLayer);
            }
            #endregion

            #region Get Pools
            /// <summary>
            /// Gets the Entity Pool
            /// </summary>
            public Pool<Entity> GetPool()
            {
                return (Pool<Entity>)_pools[0];
            }
            
            /// <summary>
            /// Gets a single <see cref="Pool{T}"/> where T is <typeparamref name="TComponent"/>
            /// </summary>
            public Pool<Entity> GetPool<T1>(out Pool<T1> C1) 
                where T1 : IComponent, new()
            {
                C1 = (Pool<T1>)_pools[_compIDs.BinarySearch(ComponentManager.ID<T1>())];
                return (Pool<Entity>)_pools[0];
            }

            /// <inheritdoc cref="GetPools{T1, T2, T3, T4}"/>
            public Pool<Entity> GetPools<T1, T2>(out Pool<T1> C1, out Pool<T2> C2)
                where T1 : IComponent, new()
                where T2 : IComponent, new()
            {
                int[] indexes = _compIDs.BinarySearch(
                    ComponentManager.ID<T1>(),
                    ComponentManager.ID<T2>());

                C1 = (Pool<T1>)_pools[indexes[0]];
                C2 = (Pool<T2>)_pools[indexes[0]];
                
                return (Pool<Entity>)_pools[0];
            }

            /// <inheritdoc cref="GetPools{T1, T2, T3, T4}"/>
            public Pool<Entity> GetPools<T1, T2, T3>(out Pool<T1> C1, out Pool<T2> C2, out Pool<T3> C3)
                where T1 : IComponent, new()
                where T2 : IComponent, new()
                where T3 : IComponent, new()
            {
                int[] indexes = _compIDs.BinarySearch(
                    ComponentManager.ID<T1>(),
                    ComponentManager.ID<T2>(),
                    ComponentManager.ID<T3>());

                C1 = (Pool<T1>)_pools[indexes[0]];
                C2 = (Pool<T2>)_pools[indexes[0]];
                C3 = (Pool<T3>)_pools[indexes[0]];

                return (Pool<Entity>)_pools[0];
            }

            /// <summary>
            /// Gets multiple Component Pools with multi-parameter binary search. 
            /// </summary>
            public Pool<Entity> GetPools<T1, T2, T3, T4>(out Pool<T1> C1, out Pool<T2> C2, out Pool<T3> C3, out Pool<T4> C4)
                where T1 : IComponent, new()
                where T2 : IComponent, new()
                where T3 : IComponent, new()
                where T4 : IComponent, new()
            {
                int[] indexes = _compIDs.BinarySearch(
                    ComponentManager.ID<T1>(), 
                    ComponentManager.ID<T2>(), 
                    ComponentManager.ID<T3>(), 
                    ComponentManager.ID<T4>());

                C1 = (Pool<T1>)_pools[indexes[0]];
                C2 = (Pool<T2>)_pools[indexes[0]];
                C3 = (Pool<T3>)_pools[indexes[0]];
                C4 = (Pool<T4>)_pools[indexes[0]];

                return (Pool<Entity>)_pools[0];
            }
            #endregion

            #region Get Components
            /// <summary>
            /// Gets a single <see cref="Pool{T}"/> where T is <typeparamref name="TComponent"/>
            /// </summary>
            public T1 GetComponent<T1>(Entity Entity)
                where T1 : IComponent, new()
            {
                return (T1)_pools[_compIDs.BinarySearch(ComponentManager.ID<T1>())][Entity._poolIndex];
            }

            /// <inheritdoc cref="GetPools{T1, T2, T3, T4}"/>
            public void GetComponents<T1, T2>(Entity Entity, out T1 C1, out T2 C2)
                where T1 : IComponent, new()
                where T2 : IComponent, new()
            {
                int[] indexes = _compIDs.BinarySearch(
                    ComponentManager.ID<T1>(),
                    ComponentManager.ID<T2>());

                C1 = (T1)_pools[indexes[0]][Entity._poolIndex];
                C2 = (T2)_pools[indexes[0]][Entity._poolIndex];
            }

            /// <inheritdoc cref="GetPools{T1, T2, T3, T4}"/>
            public void GetComponents<T1, T2, T3>(Entity Entity, out T1 C1, out T2 C2, out T3 C3)
                where T1 : IComponent, new()
                where T2 : IComponent, new()
                where T3 : IComponent, new()
            {
                int[] indexes = _compIDs.BinarySearch(
                    ComponentManager.ID<T1>(),
                    ComponentManager.ID<T2>(),
                    ComponentManager.ID<T3>());

                C1 = (T1)_pools[indexes[0]][Entity._poolIndex];
                C2 = (T2)_pools[indexes[0]][Entity._poolIndex];
                C3 = (T3)_pools[indexes[0]][Entity._poolIndex];
            }

            /// <summary>
            /// Gets multiple Component Pools with multi-parameter binary search. 
            /// </summary>
            public void GetComponents<T1, T2, T3, T4>(Entity Entity, out T1 C1, out T2 C2, out T3 C3, out T4 C4)
                where T1 : IComponent, new()
                where T2 : IComponent, new()
                where T3 : IComponent, new()
                where T4 : IComponent, new()
            {
                int[] indexes = _compIDs.BinarySearch(
                    ComponentManager.ID<T1>(),
                    ComponentManager.ID<T2>(),
                    ComponentManager.ID<T3>(),
                    ComponentManager.ID<T4>());

                C1 = (T1)_pools[indexes[0]][Entity._poolIndex];
                C2 = (T2)_pools[indexes[0]][Entity._poolIndex];
                C3 = (T3)_pools[indexes[0]][Entity._poolIndex];
                C4 = (T4)_pools[indexes[0]][Entity._poolIndex];
            }
            #endregion

            /// <summary>
            /// Gets the componentIDs of this <see cref="Archetype"/>
            /// </summary>
            public byte[] GetComponentIDs() => _compIDs;

            public IEnumerator<Entity> GetEnumerator()
            {
                Pool<Entity> EntPool = (Pool<Entity>)_pools[0];
                for (int i = 0; i < Length; i++)
                    yield return EntPool[i];
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <summary>
            /// <see cref="IPoolable"/> is an object thats stored in an <see cref="Pool{T}"/>.
            /// </summary>
            public interface IPoolable { }

            /// <summary>
            /// A simple array collection used in archetype. 
            /// </summary>
            public interface IPool
            {
                IPoolable this[int index] { get; set; }
                internal void Resize(int newSize);
                internal void Remove(int index);
            }

            /// <summary>
            /// implements <see cref="IPool"/> 
            /// </summary>
            public class Pool<T> : IPool where T : IPoolable
            {
                private T[] _array = new T[DEFAULT_ARRAY_SIZE];
                IPoolable IPool.this[int index]
                {
                    get => _array[index];
                    set => _array[index] = (T)value;
                }
                public T this[int index]
                {
                    get => _array[index];
                    set => _array[index] = value;
                }
                void IPool.Remove(int index) => _array[index] = default;
                void IPool.Resize(int newSize) => Array.Resize(ref _array, newSize);
            }
        }
    }
}
