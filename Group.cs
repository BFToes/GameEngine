using System;
using System.Collections.Generic;
namespace ECS
{
    public partial class Archetype
    {
        /// <summary>
        /// A group of <see cref="Archetype"/>s containing a filtered selection of components.
        /// Used to perform logic over a filtered selection of <see cref="Entity"/>.
        /// </summary>
        public abstract class Group
        {
            protected List<Archetype> archetypes { get; private set; }
            private readonly byte[] _anyFilter;
            private readonly byte[] _allFilter;
            private readonly byte[] _noneFilter;

            protected Group(byte[] All, byte[] Any, byte[] None, bool thing_to_differentiate_constructors)
            {
                this._allFilter = All;
                this._anyFilter = Any;
                this._noneFilter = None;
                this.archetypes = new List<Archetype>();
            }
            protected Group(params byte[] All)
            {
                this._allFilter = All;
                this.archetypes = new List<Archetype>();
            }
            public void AddArchetype(Archetype archetype)
            {
                if (archetype.HasAll(_allFilter) &&
                    archetype.HasAny(_anyFilter) &&
                    archetype.HasNone(_noneFilter))
                {
                    archetypes.Add(archetype);
                }

            }
            public IEnumerable<Entity> GetEntities() {
                foreach (Archetype A in archetypes)
                {
                    Pool<Entity> EntityPool = A.GetPool();
                    for (int i = 0; i < A.Length; i++)
                        yield return EntityPool[i];
                }
                    
            }
            public IEnumerable<TComponent> GetComponents<TComponent>() where TComponent : IComponent, new()
            {
                foreach (Archetype A in archetypes)
                {
                    Pool<TComponent> EntityPool = A.GetPool<TComponent>();
                    for (int i = 0; i < A.Length; i++)
                        yield return EntityPool[i];
                }

            }
        }
    }
}
