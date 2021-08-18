using System;

namespace ECS
{
    /// <summary>
    /// A container object for which <see cref="IComponent"/>s can be added and removed.
    /// <see cref="Entity"/> stores methods to access <see cref="IComponent"/>s but doesn't
    /// store the <see cref="IComponent"/> themselves
    /// </summary>
    public abstract class Entity : Archetype.IPoolable
    {
        private Archetype _archetype; // the archetype it belongs to
        private int _poolIndex; // the index in the archetype of itself and all its components
        
        /// <summary>
        /// initiates empty <see cref="Entity"/> into <see cref="Archetype"/>
        /// </summary>
        protected Entity(Archetype Archetype = null)
        {
            // if Archetype null adds to empty archetype in context
            this._archetype = Archetype ?? Archetype.Empty;
            this._archetype.InitEntity(this, out _poolIndex); // initiates entity in archetype
        }

       
        /// <summary>
        /// Adds a new <typeparamref name="TComponent"/> to <see cref="Entity"/>.
        /// Moves Entity to new <see cref="Archetype"/>.
        /// </summary>
        /// <returns>New <typeparamref name="TComponent"/></returns>
        public void AddComponent<TComponent>(TComponent Component = default) where TComponent : IComponent, new()
        {
            _archetype.MoveEntity(ComponentManager.ID<TComponent>(), Component ?? new TComponent(), ref _poolIndex, out _archetype);
        }

        /// <summary>
        /// Removes <typeparamref name="TComponent"/> from <see cref="Entity"/>. 
        /// Moves Entity to new <see cref="Archetype"/>.
        /// </summary>
        /// <returns>Removed <typeparamref name="TComponent"/></returns>
        public TComponent RemoveComponent<TComponent>() where TComponent : IComponent, new()
        {
            _archetype.MoveEntity(ComponentManager.ID<TComponent>(), out IComponent Component, ref _poolIndex, out _archetype);
            return (TComponent)Component;
        }
        /// <summary>
        /// returns true if <see cref="Entity"/> contains <typeparamref name="TComponent"/>.
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        public bool Has<TComponent>() where TComponent : IComponent, new()
        {
            return _archetype.HasAll(ComponentManager.ID<TComponent>());
        }

        /// <summary>
        /// Sets all <see cref="IComponent"/>s related to this <see cref="Entity"/> to new <paramref name="compIDs"/>.
        /// Matching <see cref="IComponent"/>s will be copied over. 
        /// Use <see cref="ComponentManager.ID{T1, T2, T3, T4}"/> to get <see cref="IComponent"/>'s ID.
        /// </summary> 
        public void SetComponents(params byte[] compIDs)
        {
            Array.Sort(compIDs);
            _archetype.MoveEntity(compIDs, ref _poolIndex, out _archetype);
        }

        /// <summary>
        /// return <typeparamref name="TComponent"/> by reference. Use in property.
        /// </summary>
        public ref TComponent GetComponent<TComponent>() where TComponent : IComponent, new()
        {
            return ref _archetype.GetPool<TComponent>()[_poolIndex];
        }
    }
}