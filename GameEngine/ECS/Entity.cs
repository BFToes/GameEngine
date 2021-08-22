using System;

namespace ECS
{
    /// <summary>
    /// A container object for which <see cref="IComponent"/>s can be added and removed.
    /// <see cref="Entity"/> stores methods to access <see cref="IComponent"/>s but doesn't
    /// store the <see cref="IComponent"/> themselves
    /// </summary>
    public class Entity : IPoolable
    {
        internal Archetype _archetype; // the archetype it belongs to
        internal int _poolIndex; // the index in the archetype of itself and all its components
        
        /// <summary>
        /// initiates <see cref="Entity"/> with <paramref name="Components"/>.
        /// </summary>
        public Entity(byte[] Components)
        {
            // if Archetype null adds to empty archetype in context
            this._archetype = Archetype.FindOrCreate(new ComponentSet(Components));
            this._archetype.InitEntity(this, out _poolIndex); // initiates entity in archetype
        }
        /// <summary>
        /// initiates empty <see cref="Entity"/>.
        /// </summary>
        public Entity()
        {
            this._archetype = Archetype.Empty;
            this._archetype.InitEntity(this, out _poolIndex); // initiates entity in archetype
        }
       
        /// <summary>
        /// Adds a new <typeparamref name="TComponent"/> to <see cref="Entity"/>.
        /// Moves Entity to new <see cref="Archetype"/>.
        /// </summary>
        public void AddComponent<TComponent>(TComponent Component = default) where TComponent : IComponent, new()
        {
            byte compID = ComponentManager.ID<TComponent>();
            _archetype = _archetype.MoveEntity(ref _poolIndex, _archetype.FindNext(compID));
            _archetype.pools[compID][_poolIndex] = Component;
        }

        /// <summary>
        /// Removes <typeparamref name="TComponent"/> from <see cref="Entity"/>. 
        /// Moves Entity to new <see cref="Archetype"/>.
        /// </summary>
        /// <returns>Removed <typeparamref name="TComponent"/></returns>
        public TComponent RemoveComponent<TComponent>() where TComponent : IComponent, new()
        {
            byte compID = ComponentManager.ID<TComponent>();
            TComponent Component = (TComponent)_archetype.pools[compID][_poolIndex];
            _archetype = _archetype.MoveEntity(ref _poolIndex, _archetype.FindPrev(compID));
            return Component;
        }


        /// <summary>
        /// returns true if <see cref="Entity"/> contains <typeparamref name="TComponent"/>.
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        public bool Has<TComponent>() where TComponent : IComponent, new()
        {
            return _archetype.compSet.Contains(ComponentManager.ID<TComponent>());
        }



        /// <summary>
        /// return <typeparamref name="TComponent"/> by reference. Use in property.
        /// </summary>
        public ref TComponent Get<TComponent>() where TComponent : IComponent, new()
        {
            return ref _archetype.GetPool<TComponent>()[_poolIndex];
        }
    }
}