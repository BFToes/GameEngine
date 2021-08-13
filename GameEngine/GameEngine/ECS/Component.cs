using System;
using System.Collections.Generic;
using System.Text;

namespace ECS
{
    /// <summary>
    /// A module of data that can attach to entities to provide functionality. 
    /// All data relating to an <see cref="Entity"/> is stored through <see cref="IComponent"/>s.
    /// </summary>
    public interface IComponent
    {
        /// <summary>
        /// called when this <see cref="IComponent"/> is added to <see cref="Entity"/> <paramref name="Parent"/>
        /// </summary>
        public virtual void OnAdded(Entity Parent) { }
        /// <summary>
        /// called when this <see cref="IComponent"/> is removed from <see cref="Entity"/> <paramref name="Parent"/>
        /// </summary>
        public virtual void OnRemoved(Entity Parent) { }
    }
    
    /// <summary>
    /// <see cref="ComponentManager"/> gives each <see cref="IComponent"/> type an <see cref="ComponentInitiator{TComponent}"/>.
    /// </summary>
    public static class ComponentManager
    {
        private static readonly IComponentInitiator[] Initiators = new IComponentInitiator[byte.MaxValue];
        private static readonly Type[] Types = new Type[byte.MaxValue];
        internal static int ComponentCount;

        /// <summary>
        /// Generates a ComponentID  for the <typeparamref name="TComponent"/>. The component ID is the index of the type in the Types array.
        /// </summary>
        private static byte RegisterType<TComponent>() where TComponent : IComponent, new()
        {
            Type type = typeof(TComponent);
            Types[ComponentCount] = type;
            Initiators[ComponentCount] = new ComponentInitiator<TComponent>();

            return (byte)ComponentCount++;
        }
        /// <summary>
        /// Creates a componentpool and returns it
        /// </summary>
        internal static IComponentPool CreatePool(byte ComponentID) => Initiators[ComponentID].CreatePool();
        /// <summary>
        /// Creates a Component and return it.
        /// </summary>
        internal static IComponent CreateComponent(byte ComponentID) => Initiators[ComponentID].CreateComponent();
        /// <summary>
        /// Gets the ID of the <typeparamref name="TComponent"/>
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        internal static byte ID<TComponent>() where TComponent : IComponent, new() => ComponentType<TComponent>.ID;
        /// <summary>
        /// <see cref="IComponentInitiator"/> Creates <see cref="IComponent"/>s and <see cref="IComponentPool"/>s
        /// </summary>
        private interface IComponentInitiator
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
        /// <summary>
        /// A static class for each <see cref="IComponent"/> to store the type ID.
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        private static class ComponentType<TComponent> where TComponent : IComponent, new()
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
                _id = RegisterType<TComponent>();
                Registered = true;
            }
        }
    }
}
