using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace ECS
{
    /// <summary>
    /// A module of data that can attach to entities to provide functionality. 
    /// All data relating to an <see cref="Entity"/> is stored through an <see cref="IComponent"/>.
    /// </summary>
    public interface IComponent : Entity.Archetype.IPoolable { }
    
    /// <summary>
    /// Assigns each <see cref="IComponent"/> an ID which is used for early binding initiation
    /// </summary>
    public static class ComponentManager
    {
        private static byte Count;
        private static Type[] Types = new Type[byte.MaxValue];
        private static IInitiator[] Initiators = new IInitiator[byte.MaxValue];
        
        private static byte RegisterID<TComponent>() where TComponent : IComponent, new()
        {
            if (Count == byte.MaxValue)
                throw new Exception();

            Types[Count] = typeof(TComponent);
            Initiators[Count] = new Initiator<TComponent>();
            return Count++;
        }
        
        public static byte ID<T>() where T : IComponent, new() => ComponentType<T>.ID;
        public static byte[] ID<T1, T2>()
            where T1 : IComponent, new()
            where T2 : IComponent, new()
        {
            return new byte[]
            {
                ComponentType<T1>.ID,
                ComponentType<T2>.ID,
            };
        }
        public static byte[] ID<T1, T2, T3>()
            where T1 : IComponent, new()
            where T2 : IComponent, new()
            where T3 : IComponent, new()
        {
            return new byte[]
            {
                ComponentType<T1>.ID,
                ComponentType<T2>.ID,
                ComponentType<T3>.ID,
            };
        }
        public static byte[] ID<T1, T2, T3, T4>()
            where T1 : IComponent, new()
            where T2 : IComponent, new()
            where T3 : IComponent, new()
            where T4 : IComponent, new()
        {
            return new byte[]
            {
                ComponentType<T1>.ID,
                ComponentType<T2>.ID,
                ComponentType<T3>.ID,
                ComponentType<T4>.ID,
            };
        }

        internal static IComponent InitComponent(byte ID) => Initiators[ID].CreateComponent();
        internal static Entity.Archetype.IPool InitPool(byte ID) => Initiators[ID].CreatePool();
        
        private interface IInitiator
        {
            IComponent CreateComponent();
            Entity.Archetype.IPool CreatePool();
        }
        private class Initiator<TComponent> : IInitiator where TComponent : IComponent, new()
        {
            IComponent IInitiator.CreateComponent() => new TComponent();
            Entity.Archetype.IPool IInitiator.CreatePool() => new Entity.Archetype.Pool<TComponent>();
        }
        private static class ComponentType<TComponent> where TComponent : IComponent, new()
        {
            public static readonly byte ID;
            static ComponentType()
            {
                ID = RegisterID<TComponent>();
            }
        }
    }   
}
