using System;
using System.Collections;
using System.Collections.Generic;
namespace ECS
{
    /// <summary>
    /// A group of <see cref="Archetype"/>s containing a filtered selection of components.
    /// Used to perform logic over a filtered selection of <see cref="Entity"/>.
    /// </summary>
    public abstract class Behaviour : IComparable<ComponentSet>
    {
        internal static List<Behaviour> All;
        static Behaviour()
        {
            All = new List<Behaviour>();
        }
        internal ComponentSet allFilter;

        protected List<Archetype> archetypes { get; private set; }
        
        protected Behaviour(byte[] allFilter, byte[] anyFilter, byte[] noneFilter)
        {
            // any represented as both all and none
            this.allFilter = new ComponentSet(allFilter);
            int index = All.BinarySearch(this.allFilter); // sorted by all filter
            if (index < 0) index = ~index;
            All.Insert(index, this);
            archetypes = Archetype.SearchAll(new Archetype.Query(allFilter, anyFilter, noneFilter));
        }

        public int CompareTo(ComponentSet other) => allFilter.CompareTo(other);
    }
}
