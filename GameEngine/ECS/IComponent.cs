using System;
using System.Collections;
using System.Collections.Generic;

namespace ECS
{
    /// <summary>
    /// A module of data that can attach to entities to provide functionality. 
    /// All data relating to an <see cref="Entity"/> is stored through an <see cref="IComponent"/>.
    /// </summary>
    public interface IComponent : IPoolable { }

    /// <summary>
    /// Assigns each <see cref="IComponent"/> an ID used for early binding initiation
    /// </summary>
    public static class ComponentManager
    {
        private static byte _count;
        private static Type[] _types = new Type[byte.MaxValue];
        private static IInitiator[] _initiators = new IInitiator[byte.MaxValue];
        
        private static byte RegisterID<TComponent>() where TComponent : IComponent, new()
        {
            if (_count == byte.MaxValue) throw new MaxComponentLimitExceeded();

            _types[_count] = typeof(TComponent);
            _initiators[_count] = new Initiator<TComponent>();
            return _count++;
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

        internal static byte Count => _count;
        internal static Type GetType(byte ID) => _types[ID];
        internal static IComponent InitComponent(byte ID) => _initiators[ID].CreateComponent();
        internal static Archetype.IPool InitPool(byte ID) => _initiators[ID].CreatePool();
        
        private interface IInitiator
        {
            IComponent CreateComponent();
            Archetype.IPool CreatePool();
        }
        private class Initiator<TComponent> : IInitiator where TComponent : IComponent, new()
        {
            IComponent IInitiator.CreateComponent() => new TComponent();
            Archetype.IPool IInitiator.CreatePool() => new Archetype.Pool<TComponent>();
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

    /// <summary>
    /// A 256 bit number that represents a unique of set of <see cref="IComponent"/>s. 
    /// </summary>
    /// <remarks>
    /// ComponentSet also stores an array of bytes which have been added to this Set.
    /// This Array is not regulated, so repeat values may exist and affect performance.
    ///<remarks/>
    public struct ComponentSet : IEnumerable<byte>
    {
        
        internal readonly ulong[] bits; // 256 bits = 32 bytes
        internal readonly byte[] compIDs; // 8 * 255 bits = 255bytes

        public int Count => compIDs.Length;

        /// <summary>
        /// constructs from byte IDs. Use <see cref="ComponentManager.ID{T1, T2, T3, T4}"/>.
        /// </summary>
        /// <param name="IDs"></param>
        internal ComponentSet(byte[] IDs)
        {
            compIDs = IDs;
            bits = new ulong[4]; // doesnt need to be sorted
            for (byte j = 0; j < IDs.Length; j++)
                bits[IDs[j] / 64] |= (1ul << (IDs[j] % 64)); // ORs each bit
        }
        // private constructor for Adding and removing
        private ComponentSet(ulong[] bits, byte[] compIDs)
        {
            this.bits = bits;
            this.compIDs = compIDs;
        }

        public int CompareTo(ComponentSet other)
        {
            // long is little endian
            if (this.bits[3] != other.bits[3]) return this.bits[3].CompareTo(other.bits[3]);
            if (this.bits[2] != other.bits[2]) return this.bits[2].CompareTo(other.bits[2]);
            if (this.bits[1] != other.bits[1]) return this.bits[1].CompareTo(other.bits[1]);
            return this.bits[0].CompareTo(other.bits[0]); // lowest 
        }
        
        /// <summary>
        /// Checks if this set contains <paramref name="Component_ID"/>.
        /// </summary>
        public bool Contains(byte CompID) 
        {
            return (bits[CompID / 64] & (1ul << (CompID % 64))) > 0;
        }
        
        /// <summary>
        /// adds a single <see cref="IComponent"/> ID and returns the new <see cref="ComponentSet"/>.
        /// </summary>
        internal ComponentSet Add(byte newComp)
        {
            ulong[] newBits = new ulong[4];
            bits.CopyTo(newBits, 0);
            (newBits[newComp / 64]) |= (1ul << (newComp % 64));

            byte[] newCompIDs = new byte[compIDs.Length + 1];
            compIDs.CopyTo(newCompIDs, 0);
            newCompIDs[compIDs.Length] = newComp;

            return new ComponentSet(newBits, newCompIDs);
        }

        /// <summary>
        /// removes a single <see cref="IComponent"/> ID and returns the new <see cref="ComponentSet"/>.
        /// </summary>
        internal ComponentSet Remove(byte oldComp)
        {
            ulong[] newBits = new ulong[4];
            bits.CopyTo(newBits, 0);
            (newBits[oldComp / 64]) &= ~(1ul << (oldComp % 64));

            int i = 0;
            byte[] newCompIDs = new byte[compIDs.Length - 1];
            foreach (byte compID in compIDs)
                if (compID != oldComp)
                    newCompIDs[i++] = compID;

            return new ComponentSet(newBits, newCompIDs);
        }       
        
        public IEnumerator<byte> GetEnumerator() => ((IEnumerable<byte>)compIDs).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => compIDs.GetEnumerator();

        public string ToHexString()
        {
            string str = bits[0].ToString().PadRight(3, ' ') + " : ";

            for (int i = 3; i >= 0; i--)
            {
                byte[] bytes = BitConverter.GetBytes(bits[i]); // little end is stored first
                for (int j = 7; j >= 0; j--)
                    str += Convert.ToString(bytes[j], 16).PadLeft(2, '0');
                

            }
            return str;
        }

        public string ToBinString()
        {
            string str = bits[0].ToString().PadRight(3, ' ') + " : ";
            str += Convert.ToString(BitConverter.GetBytes(bits[0])[0], 2).PadLeft(8, '0');

            return str;
        }
    }
}
