using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.GameEngine.Entities
{
    public class World
    {
        private ArchetypeManager archetypeManager = new ArchetypeManager();
        private readonly Dictionary<uint, Entity> _entities = new Dictionary<uint, Entity>();
        public void AddEntity(Entity E) 
        {
            E.World = this;
            
        }
        public void RemoveEntity(Entity E) 
        {
            E.World = null;
            
        }
    }



    public interface IComponent 
    { 
    }

    internal interface IComponentPool : IEnumerable<IComponent>
    {
        IComponent this[int Index] { get; }
        void Add(IComponent Comp);
        void Replace(int FreeIndex);
    }
    /// <summary>
    /// Component pool stores <see cref="IComponent"/> of the same type,
    /// </summary>
    /// <typeparam name="TComponent"></typeparam>
    internal class ComponentPool<TComponent> : IComponentPool where TComponent : IComponent, new()
    {
        private TComponent[] Components = new TComponent[1];
        private uint Length = 0;

        IComponent IComponentPool.this[int Index]
        {
            get
            {
                #if DEBUG
                if (Index >= Length) throw new Exception();
                #endif
                return Components[Index];
            }
        }
        void IComponentPool.Add(IComponent Comp) => Add((TComponent)Comp);
        void IComponentPool.Replace(int FreeIndex) => Components[FreeIndex] = Components[--Length];

        private void Add(TComponent C)
        {
            if (Length >= Components.Length)
                Array.Resize(ref Components, Components.Length << 1); // doubles
            Components[Length++] = C;
        }

        public IEnumerator<IComponent> GetEnumerator() 
        {
            foreach (IComponent C in Components) yield return C;
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal interface IComponentInitiator 
    {
        IComponentPool CreatePool();
        IComponent CreateComponent();
    }
    internal class ComponentInitiator<TComponent> : IComponentInitiator where TComponent : IComponent, new()
    {
        public IComponentPool CreatePool() => new ComponentPool<TComponent>();
        public IComponent CreateComponent() => new TComponent();
    }


    public static class ComponentManager 
    {
        internal static readonly IComponentInitiator[] Initiators = new IComponentInitiator[byte.MaxValue];
        internal static readonly Type[] Types = new Type[byte.MaxValue];
        internal static int _length;

        public static byte RegisterType<T>() where T : IComponent, new()
        {
            Type type = typeof(T);
            int Component = Array.IndexOf(Types, type);
            if (Component > -1) return (byte)Component; // if found, return value

            Component = _length++; // add to end of list

            Types[Component] = type;
            Initiators[Component] = new ComponentInitiator<T>();
            return (byte)Component;
        }

        public static byte ID<T>(T C) where T : IComponent, new() => ComponentType<T>.ID;
    }
    internal static class ComponentType<TComponent> where TComponent : IComponent, new()
    {
        public static byte ID { get; private set; }
        public static bool Registered;
    }

    public abstract class Archetype
    {
        private readonly uint ID;
        private HashSet<byte> ComponentIDs;
        public Entity[] Entities { get; private set; } = new Entity[1];
        internal IComponentPool[] ComponentPools { get; private set; } = new IComponentPool[byte.MaxValue];

        internal Archetype(uint ID, params byte[] ComponentIDs)
        {
            this.ID = ID;
            this.ComponentIDs = new HashSet<byte>(ComponentIDs);

            foreach (byte Component in ComponentIDs)
                ComponentPools[Component] = ComponentManager.Initiators[Component].CreatePool();
        }

    }
    internal class ArchetypeManager 
    {
        private readonly List<IArchetype> archetypes;     

    }

    public abstract class Entity
    {
        internal uint ID { get; private set; }
        internal HashSet<byte> ComponentIDs = new HashSet<byte>();
        protected internal World World { get; internal set; }

        internal void Initialize(uint ID)
        {
            this.ID = ID;
        }

        public void AddComponent<T>() where T : IComponent, new()
        {
            byte compID = ComponentType<T>.ID;
            ComponentIDs.Add(compID);

        }
        public void RemoveComponent<T>() where T : IComponent, new()
        {
            byte compID = ComponentType<T>.ID;
            ComponentIDs.Remove(compID);

            World.UpdateEntityArchetype();

        }
        public TComponent GetComponent<TComponent>() where TComponent : IComponent, new() => throw new NotImplementedException();
        public bool HasComponent<TComponent>() where TComponent : IComponent, new() => ComponentIDs.Contains(ComponentType<TComponent>.ID);

        public void Destroy()
        {
            World.RemoveEntity(this);

            OnDestroy();
        }
        protected virtual void OnDestroy() { }
    }
}
