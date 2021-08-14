using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ECS.Pool;
namespace ECS
{
    /// <summary>
    /// a local context of entities with behaviour systems and archetypes. 
    /// </summary>
    public partial class EntityContext
    {
        private readonly SinglePool<Archetype> _archetypes;
        private readonly List<Behaviour> _behaviours;

        internal Archetype EmptyArchetype;

        protected EntityContext()
        {
            EmptyArchetype = new Archetype(this, new List<Type> { });
        }

        public void AddBehaviour(Behaviour Behaviour)
        {
            _behaviours.Add(Behaviour);
            
            foreach (Archetype A in _archetypes)
                if (Behaviour._filter.Check(A.Types))
                    Behaviour.AddArchetype(A);
            
        }
        /// <summary>
        /// Searches for <see cref="Archetype"/> that matches <paramref name="Components"/> and returns it.
        /// if none found creates a new archetype matching the <paramref name="Components"/>.
        /// </summary>
        /// <param name="Components">the array of component IDs that this archetype uses.</param>
        /// <returns>an archetype that matches the description.</returns>
        internal Archetype FindOrCreateArchetype(Type[] Components)
        {
            foreach(Archetype A in _archetypes) // very bad verry stinky linear search
            {
                if (A.Equals(Components)) 
                    return A;
            }
            Archetype New = new Archetype(this, new List<Type>(Components));
            _archetypes.Add(New);

            foreach (Behaviour B in _behaviours)
                if (B._filter.Check(Components))
                    B.AddArchetype(New);

            return New;
        }

        internal IEnumerable<Archetype> GetArchetypes()
        {
            foreach (Archetype A in _archetypes)
                yield return A;
        }
        internal IEnumerable<Behaviour> GetBehaviours()
        {
            foreach (Behaviour B in _behaviours)
                yield return B;
        }
        internal IEnumerable<Entity> GetEntities()
        {
            foreach (Archetype A in _archetypes)
            {
                for (int i = 0; i < A.Length; i++)
                    yield return (Entity)A[0, i];
            }
        }
    }

}
