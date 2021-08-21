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
            internal static List<Group> All;
            static Group()
            {
                All = new List<Group>();
            }

            protected List<Archetype> archetypes { get; private set; }
            
            internal readonly ComponentSet _allFilter; // sorted by all filter
            internal readonly ComponentSet _anyFilter;
            internal readonly ComponentSet _noneFilter;

            protected Group(byte[] allFilter, byte[] anyFilter, byte[] noneFilter)
            {
                this._allFilter = new ComponentSet(allFilter);
                this._anyFilter = new ComponentSet(anyFilter);
                this._noneFilter = new ComponentSet(noneFilter);
                this.archetypes = new List<Archetype>();

                int index = All.Search(_allFilter);
                if (index < 0) index = ~index;
                All.Insert(index, this);

                // ToDo: SEARCH ARCHETYPES FOR APPLICABLE
                //      archetypes and groups should be sorted for a binary search. use
                //      insertion sort because its nearly sorted
            }
            internal void AddArchetype(Archetype archetype)
            {
                throw new NotImplementedException();
                // ToDo: CHECK IF ARCHETYPE APPLICABLE AND ADD TO LIST

            }
        }
    }
}
