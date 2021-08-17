using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ECS
{
    public abstract partial class Entity
    {
        /// <summary>
        /// A collection of <see cref="Entity"/> which all share the same types of <see cref="IComponent"/>.
        /// </summary>
        public partial class Archetype
        {
            public delegate void EntityMoved(Archetype archeype, Entity entity);

            public event EntityMoved EntityRemoved;
            public event EntityMoved EntityAdded;


            private const int DEFAULT_ARRAY_SIZE = 128;
            private readonly EntityContext _context;
            private readonly IPool[] _pools;

            private readonly IReadOnlyList<byte> _compIDs;
            private readonly IReadOnlyList<byte> _compIDLookUp;

            private readonly Archetype[] _next = new Archetype[byte.MaxValue];
            private readonly Archetype[] _prev = new Archetype[byte.MaxValue];
            

            private int _arraySize = DEFAULT_ARRAY_SIZE;
            /// <summary>
            /// The number of <see cref="Entity">Entities</see> stored in this <see cref="Archetype"/>
            /// </summary>
            public int Length { get; private set; }

            public Archetype(EntityContext Context, params byte[] CompIDs)
            {
                Array.Sort(CompIDs);
                this._context = Context;
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
            }

            /// <summary>
            /// Gets <see cref="Entity">Entities'</see> <see cref="Pool{T}"/>.
            /// </summary>
            public Pool<Entity> GetEntityPool()
            {
                return (Pool<Entity>)_pools[0];
            }
            /// <summary>
            /// Gets <typeparamref name="TComponent"/> of type T and casts <see cref="IPool"/> to <see cref="Pool{T}"><typeparamref name="TComponent"/> Pool</see>.
            /// </summary>
            public Pool<TComponent> GetComponentPool<TComponent>() where TComponent : IComponent, new()
            {
                int ID = ComponentManager.ID<TComponent>();
                return (Pool<TComponent>)_pools[_compIDLookUp[ID]];
            }

            #region Move Entity
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
                AddLayer(out entity._poolIndex, new List<IPoolable>() { entity });
                EntityAdded?.Invoke(this, entity);
            }

            /// <summary>
            /// Moves <see cref="Entity"/> from this <see cref="Archetype"/> to an <see cref="Archetype"/> 
            /// with the added <see cref="IComponent"/> <paramref name="AddedComponent"/>
            /// </summary>
            internal void MoveEntity(in Entity entity, in byte compID, IComponent component)
            {
                List<IPoolable> Layer = RemoveLayer(entity._poolIndex);
                List<byte> new_compIDs = new List<byte>(_compIDs);

                int index = new_compIDs.BinarySearch(compID);
                if (index < 0) index = ~index;
                new_compIDs.Insert(index, compID);
                Layer.Insert(index + 1, component);


                // look up
                if (_next[compID] == null) // if look up doesnt exist
                    _next[compID] = _context.FindOrCreateArchetype(new_compIDs.ToArray()); // expensive look up
                _next[compID].AddLayer(out entity._poolIndex, Layer);
                entity._archetype = _next[compID];
                
                EntityRemoved?.Invoke(this, entity);
                EntityAdded?.Invoke(_next[compID], entity);
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
                int comp_index = new_compIDs.BinarySearch(compID);
                component = (IComponent)Layer[comp_index + 1];
                Layer.RemoveAt(comp_index + 1);
                new_compIDs.RemoveAt(comp_index);

                // look up
                if (_prev[compID] == null) // if look up doesnt exist 
                    _prev[compID] = _context.FindOrCreateArchetype(new_compIDs.ToArray()); // expensive look up
                _prev[compID].AddLayer(out entity._poolIndex, Layer);
                entity._archetype = _prev[compID];

                EntityRemoved?.Invoke(this, entity);
                EntityAdded?.Invoke(_prev[compID], entity);
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


                newArchetype.AddLayer(out entity._poolIndex, newLayer);
                entity._archetype = newArchetype;

                EntityRemoved?.Invoke(this, entity);
                EntityAdded?.Invoke(newArchetype, entity);

            }
            #endregion

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
                public ref T this[int index] => ref _array[index];
                IPoolable IPool.this[int index]
                {
                    get => _array[index]; 
                    set => _array[index] = (T)value;
                }
                void IPool.Remove(int index) => _array[index] = default;
                void IPool.Resize(int newSize) => Array.Resize(ref _array, newSize);
            }
        }
    }
}
