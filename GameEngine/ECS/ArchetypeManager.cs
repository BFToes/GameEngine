using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GameEngine.ECS
{
    internal class ArchetypeManager
    {
        public int ArchetypeCount => _archetypes.Count;
        public Archetype Empty => _emptyArchetype;
        private int _archetypeIdCounter;
        private readonly Archetype _emptyArchetype;
        private readonly List<Archetype> _archetypes;
        private readonly List<Archetype>[] _archetypeIndices;

        public ArchetypeManager()
        {
            _emptyArchetype = new Archetype(_archetypeIdCounter++, new byte[] { });
            _archetypes = new List<Archetype> { _emptyArchetype };
            _archetypeIndices = new List<Archetype>[byte.MaxValue];

            for (int i = 0; i < _archetypeIndices.Length; i++)
                 _archetypeIndices[i] = new List<Archetype>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IEnumerable<Archetype> GetArchetypes(int startId)
        {
            for (int i = startId; i < _archetypes.Count; i++)
                yield return _archetypes[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IEnumerable<Archetype> GetArchetypes(byte index, int startId)
        {
            List<Archetype> archetypes = _archetypeIndices[index];

            for (int i = archetypes.Count - 1; i >= 0; i--)
            {
                Archetype archetype = archetypes[i];
                if (archetype.ID <= startId) break;
                else yield return archetype;
            }
        }

        internal Archetype FindOrCreateArchetype(params byte[] ComponentIDs)
        {
            Array.Sort(ComponentIDs);
            return InnerFindOrCreateArchetype(ComponentIDs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Archetype InnerFindOrCreateArchetype(byte[] ComponentIDs)
        {
            Archetype curArchetype = _emptyArchetype;
            for (int i = 0; i < ComponentIDs.Length; i++) // for each component index
            {
                byte index = ComponentIDs[i];
                Archetype nextArchetype = curArchetype.Next[index];

                if (nextArchetype == null)
                {
                    byte[] archetypeIndices = new byte[i + 1];
                    for (int j = 0; j < archetypeIndices.Length; j++)
                        archetypeIndices[j] = ComponentIDs[j];

                    nextArchetype = new Archetype(_archetypeIdCounter++, archetypeIndices);
                    nextArchetype.Prior[index] = curArchetype;
                    foreach (ushort componentType in nextArchetype.ComponentIDs)
                        _archetypeIndices[componentType].Add(nextArchetype);
                    
                    curArchetype.Next[index] = nextArchetype;

                    _archetypes.Add(nextArchetype);
                }

                curArchetype = nextArchetype;
            }

            return curArchetype;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Archetype FindOrCreateNextArchetype(Archetype archetype, byte addIndex)
        {
            Archetype nextArchetype = archetype.Next[addIndex];
            if (nextArchetype != null) return nextArchetype;

            bool added = false;
            int length = 0;
            byte[] indices = new byte[archetype.ComponentIDs.Count + 1];
            foreach (byte index in archetype.ComponentIDs)
            {
                if (addIndex < index && !added)
                {
                    indices[length++] = addIndex;
                    added = true;
                }

                indices[length++] = index;
            }

            if (!added)
                indices[length] = addIndex;

            return InnerFindOrCreateArchetype(indices);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Archetype FindOrCreatePriorArchetype(Archetype archetype, byte removeIndex)
        {
            Archetype priorArchetype = archetype.Prior[removeIndex];
            if (priorArchetype != null)
                return priorArchetype;

            int length = 0;
            byte[] indices = new byte[archetype.ComponentIDs.Count - 1];
            foreach (byte index in archetype.ComponentIDs)
            {
                if (index != removeIndex)
                    indices[length++] = index;
            }

            return InnerFindOrCreateArchetype(indices);
        }
    }
}