using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GameEngine.ECS.Systems;
namespace GameEngine.ECS
{


    // https://github.com/voledyhil/MiniEcs
    /// <summary>
    /// World manages the filtering of all entities in the World.
    /// World maintains a list of archetypes and organizes object-related data
    /// for optimal performance.
    /// </summary>
    public partial class World
    {
        public int EntitiesCount => _entities.Count;
        public int ArchetypeCount => _archetypeManager.ArchetypeCount;
        public int EntitiesInProcessing => _entitiesPool.Count;
        public IEntity this[uint id] => _entities[id];
        private uint _entityCounter;

        private readonly Dictionary<Filter, Group> _groups = new Dictionary<Filter, Group>();
        private readonly Dictionary<uint, EntityExtended> _entities = new Dictionary<uint, EntityExtended>();
        private readonly Queue<EntityExtended> _entitiesPool = new Queue<EntityExtended>();
        private readonly ArchetypeManager _archetypeManager = new ArchetypeManager();

        public IEntity CreateEntity() 
        {
            EntityExtended entity = _entitiesPool.Count <= 0
                ? new EntityExtended(_entitiesPool, _entities, _archetypeManager)
                : _entitiesPool.Dequeue();
            uint id = _entityCounter++;
            entity.Initialize(id);
            _entities.Add(id, entity);
            return entity;
        }
        public IEntity CreateEntity<T1>(T1 component0) where T1 : class, IComponent, new()
        {
            EntityExtended entity = _entitiesPool.Count <= 0 
                ? new EntityExtended(_entitiesPool, _entities, _archetypeManager)
                : _entitiesPool.Dequeue();

            uint id = _entityCounter++;
            entity.Initialize(id, component0);
            _entities.Add(id, entity);
            return entity;
        }
        public IEntity CreateEntity<T1, T2>(T1 component0, T2 component1) 
            where T1 : class, IComponent, new()
            where T2 : class, IComponent, new()
        {
            EntityExtended entity = _entitiesPool.Count <= 0
                ? new EntityExtended(_entitiesPool, _entities, _archetypeManager)
                : _entitiesPool.Dequeue();
            uint id = _entityCounter++;
            entity.Initialize(id, component0, component1);
            _entities.Add(id, entity);
            return entity;
        }
        public IEntity CreateEntity<T1, T2, T3>(T1 component0, T2 component1, T3 component2) 
            where T1 : class, IComponent, new()
            where T2 : class, IComponent, new()
            where T3 : class, IComponent, new()
        {
            EntityExtended entity = _entitiesPool.Count <= 0
                ? new EntityExtended(_entitiesPool, _entities, _archetypeManager)
                : _entitiesPool.Dequeue();
            uint id = _entityCounter++;
            entity.Initialize(id, component0, component1, component2);
            _entities.Add(id, entity);
            return entity;
        }
        public IEntity CreateEntity<T1, T2, T3, T4>(T1 component0, T2 component1, T3 component2, T4 component3) 
            where T1 : class, IComponent, new()
            where T2 : class, IComponent, new()
            where T3 : class, IComponent, new()
            where T4 : class, IComponent, new()
        {
            EntityExtended entity = _entitiesPool.Count <= 0
                ? new EntityExtended(_entitiesPool, _entities, _archetypeManager)
                : _entitiesPool.Dequeue();
            uint id = _entityCounter++;
            entity.Initialize(id, component0, component1, component2, component3);
            _entities.Add(id, entity);
            return entity;
        }

        public T GetOrCreateSingleton<T>() where T : class, IComponent, new()
        {
            Archetype archetype = _archetypeManager.FindOrCreateArchetype(ComponentType<T>.Index);
            Entity[] entities = archetype.GetEntities(out int length);

            for (int i = 0; i < length;)
                return entities[i].GetComponent<T>();

            T component = new T();
            CreateEntity(component);
            return component;
        }

        public IArchetype GetArchetype<T>() 
            where T : class, IComponent, new() => _archetypeManager.FindOrCreateArchetype(ComponentType<T>.Index);
        public IArchetype GetArchetype<T1, T2>()
            where T1 : class, IComponent, new()
            where T2 : class, IComponent, new() => _archetypeManager.FindOrCreateArchetype(ComponentType<T1>.Index, ComponentType<T2>.Index);
        public IArchetype GetArchetype<T1, T2, T3>()
            where T1 : class, IComponent, new()
            where T2 : class, IComponent, new()
            where T3 : class, IComponent, new() => _archetypeManager.FindOrCreateArchetype(ComponentType<T1>.Index, ComponentType<T2>.Index, ComponentType<T3>.Index);
        public IArchetype GetArchetype<T1, T2, T3, T4>()
            where T1 : class, IComponent, new()
            where T2 : class, IComponent, new()
            where T3 : class, IComponent, new()
            where T4 : class, IComponent, new() => _archetypeManager.FindOrCreateArchetype(ComponentType<T1>.Index, ComponentType<T2>.Index, ComponentType<T3>.Index, ComponentType<T4>.Index);

        /// <summary>
        /// Get a collection of archetypes for the specified filter.
        /// Each request caches the resulting set of archetypes for future use.
        /// As new archetypes are added to the world, the group of archetypes is updated.
        /// </summary>
        /// <param name="filter">
        /// A query defines a set of types of components that
        /// an archetype should include
        /// </param>
        /// <returns>Archetypes group</returns>
        public IGroup Filter(Filter filter)
        {
            int version = _archetypeManager.ArchetypeCount - 1;
            if (_groups.TryGetValue(filter, out Group group))
                if (group.Version >= version)
                    return group;

            byte[] all = filter.All?.ToArray();
            byte[] any = filter.Any?.ToArray();
            byte[] none = filter.None?.ToArray();

            if (group != null)
            {
                group.Update(version, GetArchetypes(all, any, none, group.Version));
                return group;
            }

            group = new Group(version, GetArchetypes(all, any, none, 0));
            _groups.Add(filter.Clone(), group);
            return group;
        }

        /// <summary>
        /// Retrieves all archetypes that match the search criteria.
        /// </summary>
        /// <param name="all">All component types in this array must exist in the archetype</param>
        /// <param name="any">At least one of the component types in this array must exist in the archetype</param>
        /// <param name="none">None of the component types in this array can exist in the archetype</param>
        /// <param name="startID">Archetype start id</param>
        /// <returns>Archetype enumerator</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<Archetype> GetArchetypes(byte[] all, byte[] any, byte[] none, int startID)
        {
            HashSet<Archetype> buffer1 = null;
            HashSet<Archetype> buffer2 = null;

            if (all != null || any != null)
            {
                if (all != null)
                {
                    IReadOnlyList<Archetype>[] archetypes = new IReadOnlyList<Archetype>[all.Length];
                    for (int i = 0; i < all.Length; i++) 
                        archetypes[i] = _archetypeManager.GetArchetypes(all[i], startID).ToArray();


                    Array.Sort(archetypes, (a, b) => a.Count - b.Count);

                    buffer1 = new HashSet<Archetype>(archetypes[0]);
                    for (int i = 1; i < all.Length; i++) 
                        buffer1.IntersectWith(archetypes[i]);

                }

                if (any != null)
                {
                    buffer2 = new HashSet<Archetype>(_archetypeManager.GetArchetypes(any[0], startID));
                    for (int i = 1; i < any.Length; i++)
                        buffer2.UnionWith(_archetypeManager.GetArchetypes(any[i], startID));
                }

                if (buffer1 != null && buffer2 != null)
                    buffer1.IntersectWith(buffer2);
                else if (buffer2 != null)
                    buffer1 = buffer2;

            }
            else buffer1 = new HashSet<Archetype>(_archetypeManager.GetArchetypes(startID));

            if (none != null)
                foreach (byte type in none)
                    buffer1.ExceptWith(_archetypeManager.GetArchetypes(type, startID));

            return buffer1;
        }


        private class EntityExtended : Entity
        {
            private readonly Queue<EntityExtended> _entityPool;
            private readonly Dictionary<uint, EntityExtended> _entities;

            public EntityExtended(Queue<EntityExtended> entityPool, Dictionary<uint, EntityExtended> entities, ArchetypeManager archetypeManager) : base(archetypeManager)
            {
                _entityPool = entityPool;
                _entities = entities;
            }

            protected override void OnDestroy()
            {
                _entities.Remove(ID);
                _entityPool.Enqueue(this);
            }
        }
    }
}
