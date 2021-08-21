using System;

namespace ECS
{
    /// <summary>
    /// A container object for which <see cref="IComponent"/>s can be added and removed.
    /// <see cref="Entity"/> stores methods to access <see cref="IComponent"/>s but doesn't
    /// store the <see cref="IComponent"/> themselves
    /// </summary>
    public class Entity : Archetype.IPoolable
    {
        internal Archetype _archetype; // the archetype it belongs to
        internal int _poolIndex; // the index in the archetype of itself and all its components
        
        /// <summary>
        /// initiates empty <see cref="Entity"/> into <see cref="Archetype"/>
        /// </summary>
        public Entity(Archetype Archetype = null)
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
            _archetype.MoveEntity(ComponentManager.ID<TComponent>(), Component, ref _poolIndex, out _archetype);
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
            return _archetype.Contains(ComponentManager.ID<TComponent>(), out _);
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