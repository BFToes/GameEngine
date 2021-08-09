using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ECS
{
    [Serializable]
    /// <summary>
    /// a local context of entities with behaviour systems and archetypes. 
    /// </summary>
    public abstract partial class EntityContext 
    {
        private readonly List<Archetype> _archetypes = new List<Archetype>();
        private readonly List<Behaviour> _behaviours = new List<Behaviour>();

        internal Archetype EmptyArchetype;

        protected EntityContext()
        {
            EmptyArchetype = new Archetype(this);
        }

        public void AddBehaviour(Behaviour Behaviour)
        {
            _behaviours.Add(Behaviour);
            
            foreach (Archetype A in _archetypes)
                if (Behaviour.Filter.Check(A.ComponentIDs))
                    Behaviour.AddArchetype(A);
            
        }
        public void RemoveBehaviour(Behaviour Behaviour)
        {
            _behaviours.Remove(Behaviour);
        }
        internal void RemoveArchetype(Archetype Archetype)
        {
            _archetypes.Remove(Archetype);
        }
        internal Archetype FindOrCreateArchetype(byte[] Components)
        {
            foreach(Archetype A in _archetypes)
            {
                if (A.Equals(Components)) 
                    return A;
            }
            Archetype New = new Archetype(this, Components);
            _archetypes.Add(New);

            foreach (Behaviour B in _behaviours)
                if (B.Filter.Check(Components))
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
                foreach (Entity E in A)
                    yield return E;
        }
    }

}
