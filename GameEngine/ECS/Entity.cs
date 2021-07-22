using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.ECS
{
    public abstract class Entity
    {
        public Scene Scene;

        private Entity _parent;
        public Entity Parent
        {
            get => _parent;
            set => SetParent(_parent = value);
        }
        public List<Entity> Children { get; private set; } = new List<Entity>();
        public List<EntityComponent> Components { get; protected set; } = new List<EntityComponent>();

        protected void AddComponent<T>() where T : EntityComponent, new()
        {
            if (ContainsComponent<T>()) throw new Exception("Component Already Added");
            T Component = new T();
            Components.Add(Component);
            SetParent += Component.OnSetParent;
        }

        public void Add(Entity Child) 
        { 
            Children.Add(Child);
            Child.Parent = this;
        }
        public void Remove(Entity Child) 
        { 
            Children.Remove(Child);
            Child.Parent = null;
        }
        public bool ContainsComponent<T>() where T : EntityComponent => Components.OfType<T>().Any(); 
        public T GetComponent<T>() where T : EntityComponent => Components.OfType<T>().First();

        private event Action<Entity> SetParent = delegate { };
    }


    public abstract class EntityComponent
    {
        public Entity entity;
        public virtual void OnSetParent(Entity Parent) { }
        public virtual void OnAdded(Entity Parent) { }
    }
}
