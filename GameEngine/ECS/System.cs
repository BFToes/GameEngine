using System;
using System.Collections;
using System.Collections.Generic;
namespace ECS
{
    /// <summary>
    /// A group of <see cref="Archetype"/>s containing a filtered selection of components.
    /// Used to perform logic over a filtered selection of <see cref="Entity"/>.
    /// </summary>
    public abstract class System : IComparable<ComponentSet>
    {
        internal static List<System> All;
        static System()
        {
            All = new List<System>();
        }

        protected List<Archetype> archetypes { get; private set; }
        
        // any represented as both all and none
        internal readonly ComponentSet _allFilter; 
        internal readonly ComponentSet _noneFilter; 

        protected System(byte[] allFilter, byte[] anyFilter, byte[] noneFilter)
        {
            // any represented as both all and none
            this._allFilter = new ComponentSet(allFilter);
            this._noneFilter = new ComponentSet(noneFilter);
            this.archetypes = new List<Archetype>();

            int index = All.BinarySearch(_allFilter); // sorted by all filter
            if (index < 0) index = ~index;
            All.Insert(index, this);

            //TODO: Archetype Search
        }

        public int CompareTo(ComponentSet other) => _allFilter.CompareTo(other);
    }
}
