using System;
using System.Collections.Generic;
using System.Text;

namespace ECS
{
    /// <summary>
    /// A container object for which <see cref="IComponent"/>s can be added and removed.
    /// </summary>
    public abstract class Entity : Archetype.Iitem
    {
        #region Events And Delegates
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
        #endregion

        private EntityContext _context;
        private Archetype _archetype;

        private Entity _parent;
        private List<Entity> _children;

        /// <summary>
        /// Archetype Index
        /// </summary>
        public int PoolIndex { get; set; }

        public byte EntityLayer { get; private set; } = 0;
        public IReadOnlyCollection<Entity> Children => _children.AsReadOnly();
        public Entity Parent
        {
            get => _parent;
            private set
            {
                _parent = value;
                EntityLayer = _parent?.EntityLayer ?? 0;
                EntityLayer++;
                ParentChanged?.Invoke(value);
            }
        }


        /// <summary>
        /// initiates empty <see cref="Entity"/> into <see cref="Archetype"/>
        /// </summary>
        protected Entity(EntityContext Context, Archetype Archetype = null)
        {
            this._children = new List<Entity>();
            this._context = Context;
            this._archetype = Archetype ?? _context.EmptyArchetype;
            this._archetype.AddLayer(this); // if Archetype null adds to empty archetype in context
        }

        #region Component Methods
        /// <summary>
        /// Adds a new <typeparamref name="TComponent"/> to <see cref="Entity"/>.
        /// Moves Entity to new <see cref="Archetype"/>.
        /// </summary>
        /// <returns>New <typeparamref name="TComponent"/></returns>
        public void AddComponent<TComponent>(TComponent Component = default) where TComponent : IComponent, new()
        {
            byte CompID = ComponentManager.ID<TComponent>();
            _archetype.MoveEntity(this, CompID, Component ?? new TComponent());
            ComponentAdded?.Invoke(this, Component);
        }
        /// <summary>
        /// Removes <typeparamref name="TComponent"/> from <see cref="Entity"/>. 
        /// Moves Entity to new Archetype.
        /// </summary>
        /// <returns>Removed <typeparamref name="TComponent"/></returns>
        public TComponent RemoveComponent<TComponent>() where TComponent : IComponent, new()
        {
            byte CompID = ComponentManager.ID<TComponent>();
            _archetype.MoveEntity(this, CompID, out IComponent Component);
            ComponentRemoved?.Invoke(this, Component);
            return (TComponent)Component;
        }
        /// <summary>
        /// Gets the <see cref="IComponent"/> of type <typeparamref name="TComponent"/> on this entity.
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        public TComponent GetComponent<TComponent>() where TComponent : IComponent, new() => (TComponent)_archetype[_archetype.FindComponent<TComponent>(), PoolIndex];
        /// <summary>
        /// If Entity Implements a <see cref="IComponent"/> of type <typeparamref name="TComponent"/> return true
        /// </summary>
        public bool HasComponent<TComponent>() where TComponent : IComponent, new() => _archetype.FindComponent<TComponent>() != -1;
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
