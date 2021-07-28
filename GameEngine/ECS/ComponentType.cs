using System;
using Serializer = BinarySerializer.BinarySerializer;

namespace GameEngine.ECS
{
    /// <summary>
    /// an empty interface to represent a component
    /// </summary>
    public interface IComponent { }

    /// <summary>
    /// Manages the types of component and stores them in individual <see cref="ComponentPool{T}"/>
    /// </summary>
    internal static class TypeManager 
    {
        public static readonly IComponentPoolCreator[] ComponentPoolCreators = new IComponentPoolCreator[byte.MaxValue];
        public static readonly Type[] Types = new Type[byte.MaxValue];

        private static byte _length;

        /// <summary>
        /// registers component type. initiates component pool and component creator.
        /// </summary>
        /// <typeparam name="T">the component's type</typeparam>
        /// <returns></returns>
        internal static byte RegisterType<T>() where T : class, IComponent, new()
        {
            Type type = typeof(T);

            int Component = Array.IndexOf(Types, type); // search if Type already exists
            if (Component > -1) return (byte)Component; // if found, return value

            Component = _length++; // add to end of list

            Types[Component] = type;
            ComponentPoolCreators[Component] = new ComponentPoolCreator<T>();
            return (byte)Component;
        }

        internal static IComponent CreateComponent(byte Component) => ComponentPoolCreators[Component].CreateComponent();
    }
    /// <summary>
    /// a class for registering each component type with the type manager
    /// </summary>
    /// <typeparam name="T">the component type</typeparam>
    internal static class ComponentType<T> where T : class, IComponent, new()
    {
        private static byte _index;
        private static bool _registered;

        public static byte ID
        {
            // index of the component pool
            get
            {
                if (!_registered) throw new InvalidOperationException("component must be registered before use");
                return _index;
            }
        }

        public static void Register()
        {
            _registered = true;
            _index = TypeManager.RegisterType<T>();

            Serializer.RegisterType(typeof(T));
        }
    }
}