using System;
using Serializer = BinarySerializer.BinarySerializer;

namespace GameEngine.ECS
{
    /// <summary>
    /// an empty interface to represent a component
    /// </summary>
    public interface IComponent { }

    /// <summary>
    /// create component pool creators for each new component and stores it.
    /// </summary>
    internal static class TypeManager
    {
        public static readonly IComponentPoolCreator[] ComponentPoolCreators = new IComponentPoolCreator[byte.MaxValue];
        public static readonly Type[] Types = new Type[byte.MaxValue];

        private static byte _length;
        internal static byte RegisterType<T>() where T : class, IComponent, new()
        {
            Type type = typeof(T);

            int index = Array.IndexOf(Types, type); // search if Type already exists
            if (index > -1) return (byte)index; // if found, return value

            index = _length++; // add to end of list

            Types[index] = type;
            ComponentPoolCreators[index] = new ComponentPoolCreator<T>();
            return (byte)index;
        }

        internal static IComponent CreateComponent(byte index) => ComponentPoolCreators[index].CreateComponent();
    }
    /// <summary>
    /// a class for registering each component type with the type manager
    /// </summary>
    /// <typeparam name="T">the component type</typeparam>
    internal static class ComponentType<T> where T : class, IComponent, new()
    {
        private static byte _index;
        private static bool _isRegister;


        public static byte Index
        {
            get
            {
                if (!_isRegister) throw new InvalidOperationException("component must be registered before use");
                return _index;
            }
        }

        public static void Register()
        {
            _isRegister = true;
            _index = TypeManager.RegisterType<T>();

            Serializer.RegisterType(typeof(T));
        }
    }
}