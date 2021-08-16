using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ECS
{
    /// <summary>
    /// a local context of entities with behaviour systems and archetypes. 
    /// </summary>
    public partial class EntityContext
    {
        protected readonly List<Archetype> _archetypes = new List<Archetype>();
        protected readonly List<Behaviour> _behaviours = new List<Behaviour>();

        internal Archetype EmptyArchetype;

        protected EntityContext()
        {
            EmptyArchetype = new Archetype(this, new byte[0]);
        }

        /// <summary>
        /// Searches for <see cref="Archetype"/> that matches <paramref name="Components"/> and returns it.
        /// if none found creates a new archetype matching the <paramref name="Components"/>.
        /// </summary>
        /// <param name="Components">the array of component IDs that this archetype uses.</param>
        /// <returns>an archetype that matches the description.</returns>
        internal Archetype FindOrCreateArchetype(byte[] Components)
        {
            foreach (Archetype A in _archetypes)
                if (A.Equals(Components)) 
                    return A;

            Archetype New = new Archetype(this, Components);
            _archetypes.Add(New);

            foreach (Behaviour B in _behaviours)
                B.AddIfApplicable(New);
            return New;
        }       
    }
}
