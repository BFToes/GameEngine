using System;
using System.Collections.Generic;
using System.Text;

namespace ECS
{
    public interface IComponent { }
    /// <summary>
    /// <see cref="ComponentManager"/> gives each <see cref="IComponent"/> type an <see cref="ComponentInitiator{TComponent}"/>.
    /// </summary>
    internal static class ComponentManager
    {
        private static readonly IComponentInitiator[] Initiators = new IComponentInitiator[byte.MaxValue];
        internal static readonly Type[] Types = new Type[byte.MaxValue];
        internal static int _length;

        internal static IComponentPool CreatePool(byte ComponentID) => Initiators[ComponentID].CreatePool();
        internal static IComponent CreateComponent(byte ComponentID) => Initiators[ComponentID].CreateComponent();

        internal static byte RegisterType<TComponent>() where TComponent : IComponent, new()
        {
            Type type = typeof(TComponent);
            Types[_length++] = type;
            Initiators[_length] = new ComponentInitiator<TComponent>();
            ComponentType<TComponent>.Registered = true;
            return (byte)_length;
        }
        internal static byte ID<T>() where T : IComponent, new() => ComponentType<T>.ID;
        
        internal interface IComponentInitiator
        {
            IComponentPool CreatePool();
            IComponent CreateComponent();
        }
        /// <summary>
        /// a class for initiating typed <see cref="IComponent"/>s and <see cref="IComponentPool"/> 
        /// </summary>
        private class ComponentInitiator<TComponent> : IComponentInitiator where TComponent : IComponent, new()
        {
            public IComponentPool CreatePool() => new ComponentPool<TComponent>();
            public IComponent CreateComponent() => new TComponent();
        }
    }
    /// <summary>
    /// A static class for each <see cref="IComponent"/> to store the type ID.
    /// </summary>
    /// <typeparam name="TComponent"></typeparam>
    internal static class ComponentType<TComponent> where TComponent : IComponent, new()
    {
        private static byte _id;
        public static byte ID
        {
            get
            {
                if (!Registered) _id = ComponentManager.RegisterType<TComponent>();
                return _id;
            }
        }
        public static bool Registered = false;
    }
}
