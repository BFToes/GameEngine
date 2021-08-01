using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GameEngine.ECS.Systems
{
    internal delegate void ForEachEntity(Entity entity);
    internal delegate void ForEachArchetype(Archetype archetype);
    internal delegate void ForEachEntityComponent<in T1>(Entity entity, T1 comp0) where T1 : class, IComponent, new();
    internal delegate void ForEachEntityComponent<in T1, in T2>(Entity entity, T1 comp0, T2 comp1) where T1 : class, IComponent, new() where T2 : class, IComponent, new();
    internal delegate void ForEachEntityComponent<in T1, in T2, in T3>(Entity entity, T1 comp0, T2 comp1, T3 comp2) where T1 : class, IComponent, new() where T2 : class, IComponent, new() where T3 : class, IComponent, new();
    internal delegate void ForEachEntityComponent<in T1, in T2, in T3, in T4>(Entity entity, T1 comp0, T2 comp1, T3 comp2, T4 comp3) where T1 : class, IComponent, new() where T2 : class, IComponent, new() where T3 : class, IComponent, new() where T4 : class, IComponent, new();

    /// <summary>
    /// collection of archetypes matching filter criteria
    /// </summary>
    internal class Group : IEnumerable<Archetype>
    {
        private delegate void ForEachArchetypeHandler(Archetype archetype);

        /// <summary>
        /// Сurrent group version
        /// </summary>
        public int Version { get; private set; }

        /// <summary>
        /// List of current archetypes
        /// </summary>
        private readonly List<Archetype> _archetypes;

        /// <summary>
        /// Creates a new archetype group corresponding to the specified version.
        /// </summary>
        /// <param name="version">Group version</param>
        /// <param name="archetypes">Archetype collection</param>
        public Group(int version, IEnumerable<Archetype> archetypes)
        {
            Version = version;
            _archetypes = new List<Archetype>(archetypes);
        }

        /// <summary>
        /// Adds new archetypes to the group, raises the version of the group
        /// </summary>
        /// <param name="newVersion">New Version</param>
        /// <param name="newArchetypes">New Archetypes</param>
        public void Update(int newVersion, IEnumerable<Archetype> newArchetypes)
        {
            Version = newVersion;
            _archetypes.AddRange(newArchetypes);
        }

        /// <summary>
        /// Calculate the number of entities in a group
        /// </summary>
        /// <returns>Number of entities</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CalculateCount()
        {
            int count = 0;
            foreach (Archetype archetype in _archetypes)
            {
                count += archetype.EntityCount;
            }

            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach(ForEachArchetype handler)
        {
            foreach (Archetype archetype in _archetypes)
            {
                if (archetype.EntityCount <= 0) continue;

                handler(archetype);
            }
        }

        public Entity[] ToEntityArray()
        {
            int index = 0;
            Entity[] totalEntities = new Entity[CalculateCount()];

            ForEach(archetype =>
            {
                int length = archetype.Entities.Length;
                for (int j = 0; j < length; j++)
                    totalEntities[index++] = archetype.Entities[j];
            });

            return totalEntities;
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach(ForEachEntity handler)
        {
            ForEach(archetype =>
            {
                for (int j = 0; j < archetype.Entities.Length; j++)
                    handler(archetype.Entities[j]);
            });
        }

        void ForEach<T1>(ForEachEntityComponent<T1> handler)
            where T1 : class, IComponent, new()
        {
            ForEach(archetype =>
            {
                ComponentPool<T1> comps0 = archetype.GetComponentPool<T1>();

                for (int j = 0; j < archetype.Entities.Length; j++)
                    handler(archetype.Entities[j], comps0.GetTyped(j));
            });
        }

        void ForEach<T1, T2>(ForEachEntityComponent<T1, T2> handler)
            where T1 : class, IComponent, new()
            where T2 : class, IComponent, new()
        {
            ForEach(archetype =>
            {
                ComponentPool<T1> comps0 = archetype.GetComponentPool<T1>();
                ComponentPool<T2> comps1 = archetype.GetComponentPool<T2>();

                for (int j = 0; j < archetype.Entities.Length; j++)
                    handler(archetype.Entities[j], comps0.GetTyped(j), comps1.GetTyped(j));
            });
        }

        void ForEach<T1, T2, T3>(ForEachEntityComponent<T1, T2, T3> handler)
            where T1 : class, IComponent, new()
            where T2 : class, IComponent, new()
            where T3 : class, IComponent, new()
        {
            ForEach(archetype =>
            {
                ComponentPool<T1> comps0 = archetype.GetComponentPool<T1>();
                ComponentPool<T2> comps1 = archetype.GetComponentPool<T2>();
                ComponentPool<T3> comps2 = archetype.GetComponentPool<T3>();

                for (int j = 0; j < archetype.Entities.Length; j++)
                    handler(archetype.Entities[j], comps0.GetTyped(j), comps1.GetTyped(j), comps2.GetTyped(j));
            });
        }

        void ForEach<T1, T2, T3, T4>(ForEachEntityComponent<T1, T2, T3, T4> handler)
            where T1 : class, IComponent, new()
            where T2 : class, IComponent, new()
            where T3 : class, IComponent, new()
            where T4 : class, IComponent, new()
        {
            ForEach(archetype =>
            {
                ComponentPool<T1> comps0 = archetype.GetComponentPool<T1>();
                ComponentPool<T2> comps1 = archetype.GetComponentPool<T2>();
                ComponentPool<T3> comps2 = archetype.GetComponentPool<T3>();
                ComponentPool<T4> comps3 = archetype.GetComponentPool<T4>();

                for (int j = 0; j < archetype.Entities.Length; j++)
                    handler(archetype.Entities[j], comps0.GetTyped(j), comps1.GetTyped(j), comps2.GetTyped(j), comps3.GetTyped(j));
            });
        }

        public IEnumerator<Archetype> GetEnumerator() => _archetypes.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}