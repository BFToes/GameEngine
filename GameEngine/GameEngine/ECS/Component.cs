using System;
using System.Collections.Generic;
using System.Text;

namespace ECS
{
    /// <summary>
    /// A reusable module of data that can attach to entities to provide functionality. 
    /// All logic is implemented through <see cref="IComponent"/>s.
    /// </summary>
    public interface IComponent { }
    /// <summary>
    /// <see cref="ComponentManager"/> gives each <see cref="IComponent"/> type an <see cref="ComponentInitiator{TComponent}"/>.
    /// </summary>
    internal static class ComponentManager
    {
        private static readonly IComponentInitiator[] Initiators = new IComponentInitiator[byte.MaxValue];
        internal static readonly Type[] Types = new Type[byte.MaxValue];
        internal static int ComponentCount;

        internal static IComponentPool CreatePool(byte ComponentID) => Initiators[ComponentID].CreatePool();
        internal static IComponent CreateComponent(byte ComponentID) => Initiators[ComponentID].CreateComponent();

        internal static byte RegisterType<TComponent>() where TComponent : IComponent, new()
        {
            Type type = typeof(TComponent);
            Types[ComponentCount] = type;
            Initiators[ComponentCount] = new ComponentInitiator<TComponent>();
            
            return (byte)ComponentCount++;
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

        internal static void WriteDebug()
        {
            int i = 0;
            while (i < ComponentCount)
                Console.WriteLine($"Component {i} : {Types[i++]}");
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
                if (!Registered) Register();
                return _id;
            }
        }
        public static bool Registered { get; private set; } = false;
        public static void Register()
        {
            _id = ComponentManager.RegisterType<TComponent>();
            Registered = true;
        }
    }
}
