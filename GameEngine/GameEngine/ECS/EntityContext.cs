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
        protected readonly List<Entity.Archetype> _archetypes = new List<Entity.Archetype>();
        protected readonly List<Entity.Behaviour> _behaviours = new List<Entity.Behaviour>();

        internal Entity.Archetype EmptyArchetype;

        protected EntityContext()
        {
            EmptyArchetype = new Entity.Archetype(this, new byte[0]);
        }

        /// <summary>
        /// Searches for <see cref="Archetype"/> that matches <paramref name="Components"/> and returns it.
        /// if none found creates a new archetype matching the <paramref name="Components"/>.
        /// </summary>
        /// <param name="Components">the array of component IDs that this archetype uses.</param>
        /// <returns>an archetype that matches the description.</returns>
        internal Entity.Archetype FindOrCreateArchetype(byte[] Components)
        {
            foreach (Entity.Archetype A in _archetypes)
                if (A.Equals(Components)) 
                    return A;

            Entity.Archetype New = new Entity.Archetype(this, Components);
            _archetypes.Add(New);
            

            // ToDo: BEHAVIOUR SEARCH
            //     if Archetype stores Behaviours we can do a more optimised search for 
            //     behaviours. how specific do we need search parameters. we could do a
            //     binary search array on the behaviours quite easily
            //foreach (Entity.Behaviour B in _behaviours)
            //    B.AddIfApplicable(New);
            return New;
        }
    }
}
