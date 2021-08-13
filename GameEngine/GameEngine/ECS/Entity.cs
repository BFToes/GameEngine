using System;
using System.Collections.Generic;
using System.Text;

namespace ECS
{
    /// <summary>
    /// A container object for which <see cref="IComponent"/>s can be added and removed.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// Delegate for ComponentAdded and ComponentRemoved.
        /// </summary>
        public delegate void EntityComponentChanged(Entity Entity, IComponent Component);
        /// <summary>
        /// Delegate for ChildAdded and ChildRemoved.
        /// </summary>
        public delegate void EntityHierarchyChanged(Entity Child);

        /// <summary>
        /// called when a component is added
        /// </summary>
        public event EntityComponentChanged ComponentAdded;
        /// <summary>
        /// called when a component is removed
        /// </summary>
        public event EntityComponentChanged ComponentRemoved;
        /// <summary>
        /// called when a child is added
        /// </summary>
        public event EntityHierarchyChanged ChildAdded;
        /// <summary>
        /// called when a child is removed. 
        /// </summary>
        public event EntityHierarchyChanged ChildRemoved;
        /// <summary>
        /// called when parent is set
        /// </summary>
        public event EntityHierarchyChanged ParentChanged;
        /// <summary>
        /// unique ID for each entity instance
        /// </summary>
        public readonly int EntityID;

        protected internal EntityContext Context { get; internal set; }
        internal Archetype Archetype;
        internal int ArchetypeIndex;

        private Entity _parent;
        public Entity Parent
        {
            get => _parent; private set
            {
                _parent = value;
                ParentChanged?.Invoke(value);
            }
        }
        private List<Entity> _children;

        protected Entity(EntityContext Context)
        {
            this.Context = Context; 
            this.Archetype = Context.EmptyArchetype;
            this.EntityID = Context.GetEntityID();
            ArchetypeIndex = Context.EmptyArchetype.AddEntity(this);
        }

        #region Component Methods
        /// <summary>
        /// Adds a new <typeparamref name="TComponent"/> to <see cref="Entity"/>.
        /// Moves Entity to new Archetype.
        /// </summary>
        /// <returns>New <typeparamref name="TComponent"/></returns>
        public TComponent AddComponent<TComponent>() where TComponent : IComponent, new()
        {
            byte ComponentID = ComponentManager.ID<TComponent>();
            Archetype.MoveEntityTo(this, Archetype.FindNext(ComponentID));
            IComponent Component = Archetype.GetComponent(ComponentID, ArchetypeIndex);
            ComponentAdded?.Invoke(this, Component);
            Component.OnAdded(this);
            return (TComponent)Component;
        }
        /// <summary>
        /// Removes <typeparamref name="TComponent"/> from <see cref="Entity"/>. 
        /// Moves Entity to new Archetype.
        /// </summary>
        /// <returns>Removed <typeparamref name="TComponent"/></returns>
        public TComponent RemoveComponent<TComponent>() where TComponent : IComponent, new()
        {
            byte ComponentID = ComponentManager.ID<TComponent>();
            IComponent Component = Archetype.GetComponent(ComponentID, ArchetypeIndex);
            Archetype.MoveEntityTo(this, Archetype.FindPrior(ComponentID));
            ComponentRemoved?.Invoke(this, Component);
            Component.OnRemoved(this);
            return (TComponent)Component;
        }
        /// <summary>
        /// removes all components from this entity
        /// </summary>
        public void ClearComponents()
        {
            IEnumerable<IComponent> Components = Archetype.GetAllComponents(ArchetypeIndex);
            Archetype.MoveEntityTo(this, Context.EmptyArchetype);
            foreach (IComponent C in Components)
                ComponentRemoved(this, C);
        }
        /// <summary>
        /// Gets the <see cref="IComponent"/> of type <typeparamref name="TComponent"/> on this entity.
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        public TComponent GetComponent<TComponent>() where TComponent : IComponent, new() => (TComponent)Archetype.GetComponent(ComponentManager.ID<TComponent>(), ArchetypeIndex);
        /// <summary>
        /// If Entity Implements a <see cref="IComponent"/> of type <typeparamref name="TComponent"/> return true
        /// </summary>
        public bool HasComponent<TComponent>() where TComponent : IComponent, new() => Archetype.Has<TComponent>();
        #endregion

        #region Hierarchy Methods
        /// <summary>
        /// associates <paramref name="Entity"/> as child in hierarchy.
        /// Removes previous association.
        /// </summary>
        public virtual void AddChild(Entity Entity) 
        {
            if (Entity.Parent != null)
                Parent.RemoveChild(Entity);
            
            _children.Add(Entity);            
            Entity.Parent = this;
            ChildAdded?.Invoke(Entity);
        }
        /// <summary>
        /// removes association of <paramref name="Entity"/> as child in Hierarchy.
        /// Removes previous association.
        /// </summary>
        public virtual void RemoveChild(Entity Entity)
        {
            _children.Remove(Entity);
            Entity.Parent = null;
            ChildRemoved?.Invoke(Entity);
        }
        /// <summary>
        /// returns the children entities associated with this entity
        /// </summary>
        public IEnumerable<Entity> GetChildren() => _children;
        #endregion
    }
}
