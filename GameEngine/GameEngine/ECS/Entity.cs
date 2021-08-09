using System;
using System.Collections.Generic;
using System.Text;

namespace ECS
{
    /// <summary>
    /// A container object for which <see cref="IComponent"/>s can be added and removed.
    /// </summary>
    public abstract partial class Entity
    {
        protected internal EntityContext Manager { get; internal set; }
        internal Archetype Archetype;
        internal int ArchetypeIndex = -1;

        protected Entity(EntityContext Manager)
        {
            this.Manager = Manager;
            this.Archetype = Manager.EmptyArchetype;
            ArchetypeIndex = Manager.EmptyArchetype.AddEntity(this);
        }
        /// <summary>
        /// Adds a new <typeparamref name="TComponent"/> to <see cref="Entity"/>.
        /// Moves Entity to new Archetype.
        /// </summary>
        /// <returns>New <typeparamref name="TComponent"/></returns>
        protected TComponent AddComponent<TComponent>() where TComponent : IComponent, new()
        {
            byte ComponentID = ComponentManager.ID<TComponent>();
            Archetype.MoveEntityTo(this, Archetype.FindNext(ComponentID));
            TComponent Component = (TComponent)Archetype.GetComponent(ComponentID, ArchetypeIndex);
            return Component;
        }
        /// <summary>
        /// Removes <typeparamref name="TComponent"/> from <see cref="Entity"/>. 
        /// Moves Entity to new Archetype.
        /// </summary>
        /// <returns>Removed <typeparamref name="TComponent"/></returns>
        protected TComponent RemoveComponent<TComponent>() where TComponent : IComponent, new()
        {
            byte ComponentID = ComponentManager.ID<TComponent>();
            TComponent Component = (TComponent)Archetype.GetComponent(ComponentID, ArchetypeIndex);
            Archetype.MoveEntityTo(this, Archetype.FindPrior(ComponentID));
            return Component;
        }
        /// <summary>
        /// Gets the <typeparamref name="TComponent"/> on this entity.
        /// 
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        public TComponent GetComponent<TComponent>() where TComponent : IComponent, new() => (TComponent)Archetype.GetComponent(ComponentManager.ID<TComponent>(), ArchetypeIndex);
        public bool HasComponent<TComponent>() where TComponent : IComponent, new() => Archetype.Has<TComponent>();
    }
}
