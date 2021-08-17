﻿using System;
using System.Collections.Generic;
using System.Text;
using ListExtensions;

namespace ECS
{
    /// <summary>
    /// A container object for which <see cref="IComponent"/>s can be added and removed.
    /// </summary>
    public abstract partial class Entity : Entity.Archetype.IPoolable
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

        private readonly EntityContext _context;
        private Archetype _archetype;

        private Entity _parent;
        private List<Entity> _children;
        private int _poolIndex;

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
            // if Archetype null adds to empty archetype in context
            this._archetype = Archetype ?? _context.EmptyArchetype;
            this._archetype.InitEntity(this); // initiates entity in archetype
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
        /// Sets all <see cref="IComponent"/>s related to this <see cref="Entity"/> to different <see cref="IComponent"/>s.
        /// matching <see cref="IComponent"/>s in <see cref="Entity"/> before function call will copied over. 
        /// Use <see cref="ComponentManager.ID{T1, T2, T3, T4}"/> to get <see cref="IComponent"/>'s ID.
        /// </summary> 
        public void SetComponents(params byte[] compIDs)
        {
            Array.Sort(compIDs);
            List<IComponent> RemoveComponents, AddedComponents;
            _archetype.MoveEntity(this, _context.FindOrCreateArchetype(compIDs), out RemoveComponents, out AddedComponents);
            foreach(IComponent C in RemoveComponents)
                ComponentRemoved?.Invoke(this, C);
            foreach(IComponent C in AddedComponents)
                ComponentAdded?.Invoke(this, C);
        }

        /// <summary>
        /// return <typeparamref name="TComponent"/> by reference. 
        /// </summary>
        public ref TComponent GetComponent<TComponent>() where TComponent : IComponent, new()
        {
            return ref _archetype.GetComponentPool<TComponent>()[_poolIndex];
        }
        /// <summary>
        /// sets <typeparamref name="TComponent"/>. 
        /// </summary>
        public void SetComponent<TComponent>(in TComponent value) where TComponent : IComponent, new()
        {
            _archetype.GetComponentPool<TComponent>()[_poolIndex] = value;
        }
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