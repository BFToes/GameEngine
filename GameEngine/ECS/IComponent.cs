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
    /// a 256 bit number that represents a unique of set of <see cref="IComponent"/>s. 
    /// </summary>
    public struct ComponentSet : IEnumerable<byte>, IComparable<ComponentSet>
    {
        
        private readonly ulong[] _bits; // 256 bits = 32 bytes
        private readonly byte[] _compIDs; // 8 * 255 bits = 255bytes

        public int Count => _compIDs.Length;

        #region Constructors
        /// <summary>
        /// constructs from byte IDs. Use <see cref="ComponentManager.ID{T1, T2, T3, T4}"/>.
        /// </summary>
        /// <param name="IDs"></param>
        public ComponentSet(byte[] IDs)
        {
            _compIDs = IDs;
            _bits = new ulong[4]; // doesnt need to be sorted
            for (byte j = 0; j < IDs.Length; j++)
                _bits[IDs[j] / 64] |= (1ul << (IDs[j] % 64));
        }
        // private constructor for Adding and removing
        private ComponentSet(ulong[] _bits, byte[] compIDs)
        {
            this._bits = _bits;
            this._compIDs = compIDs;
        }
        #endregion

        public int CompareTo(ComponentSet other)
        {
            // long is little endian
            if (this._bits[3] != other._bits[3]) return this._bits[3].CompareTo(other._bits[3]);
            if (this._bits[2] != other._bits[2]) return this._bits[2].CompareTo(other._bits[2]);
            if (this._bits[1] != other._bits[1]) return this._bits[1].CompareTo(other._bits[1]);
            return this._bits[0].CompareTo(other._bits[0]); // lowest 
        }

        /// <summary>
        /// Checks if this set contains <paramref name="Component_ID"/>.
        /// </summary>
        public bool Contains(byte CompID) 
        {
            return (_bits[CompID / 64] & (1ul << (CompID % 64))) > 0;
        }

        
        #region Add/Remove Component
        /// <summary>
        /// adds a single <see cref="IComponent"/> ID and returns the new <see cref="ComponentSet"/>.
        /// </summary>
        public ComponentSet Add(byte newComp)
        {
            ulong[] newBits = new ulong[4];
            _bits.CopyTo(newBits, 0);
            (newBits[newComp / 64]) |= (1ul << (newComp % 64));

            byte[] newCompIDs = new byte[_compIDs.Length + 1];
            _compIDs.CopyTo(newCompIDs, 0);
            newCompIDs[_compIDs.Length] = newComp;

            return new ComponentSet(newBits, newCompIDs);
        }


        /// <summary>
        /// removes a single <see cref="IComponent"/> ID and returns the new <see cref="ComponentSet"/>.
        /// </summary>
        public ComponentSet Remove(byte oldComp)
        {
            ulong[] newBits = new ulong[4];
            _bits.CopyTo(newBits, 0);
            (newBits[oldComp / 64]) &= ~(1ul << (oldComp % 64));

            int i = 0;
            byte[] newCompIDs = new byte[_compIDs.Length - 1];
            foreach (byte compID in _compIDs)
                if (compID != oldComp)
                    newCompIDs[i++] = compID;

            return new ComponentSet(newBits, newCompIDs);
        }       
        #endregion
        
        public IEnumerator<byte> GetEnumerator() => ((IEnumerable<byte>)_compIDs).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _compIDs.GetEnumerator();

        public string ToHexString()
        {
            string str = _bits[0].ToString().PadRight(3, ' ') + " : ";

            for (int i = 3; i >= 0; i--)
            {
                byte[] bytes = BitConverter.GetBytes(_bits[i]); // little end is stored first
                for (int j = 7; j >= 0; j--)
                    str += Convert.ToString(bytes[j], 16).PadLeft(2, '0');
                

            }
            return str;
        }

        public string ToBinString()
        {
            /* 
            // Whole binary string not very useful
            string str = _bits[0].ToString().PadRight(3, ' ') + " : ";

            for (int i = 3; i >= 0; i--)
            {
                byte[] bytes = BitConverter.GetBytes(_bits[i]); // little end is stored first
                for (int j = 7; j >= 0; j--)
                    str += Convert.ToString(bytes[j], 2).PadLeft(8, '0');
                

            }
            return str;
            */
            // makes big endian??? I want little endian
            string str = _bits[0].ToString().PadRight(3, ' ') + " : ";
            str += Convert.ToString(BitConverter.GetBytes(_bits[0])[0], 2).PadLeft(8, '0');

            return str;
        }
    }


    public static partial class ListExtensions
    {
        /// <summary>
        /// Binary search to find an index which equals <paramref name="value"/>.
        /// </summary>
        /// <returns>
        /// An index of the <paramref name="value"/> in the <paramref name="list"/>. 
        /// If an index is not found, returns a bitwise complement index into which the value 
        /// should be inserted.
        /// </returns>
        public static int BinarySearch<T>(this List<T> list, ComponentSet value) where T : IComparable<ComponentSet>
        {
            int index = 0;
            int lower = 0;
            int upper = list.Count - 1;

            while (lower <= upper)
            {
                index = (upper + lower) / 2;

                int diff = list[index].CompareTo(value);
                if (diff > 0) upper = index - 1;
                else if (diff < 0) lower = index + 1;
                else return index;
            }
            if (index == upper) index += 1;
            return ~index;
        }
        
        /// <summary>
        /// Binary search to find the first index which equals <paramref name="value"/>.
        /// </summary>
        /// <returns>
        /// The first index of the <paramref name="value"/> in the <paramref name="list"/>. 
        /// If an index is not found, returns a bitwise complement index into which the value 
        /// should be inserted.
        /// </returns>
        public static int BinaryFirst<T>(this List<T> list, ComponentSet value) where T :  IComparable<ComponentSet>
        {
            int result = -1;
            int index = 0;
            int lower = 0;
            int upper = list.Count - 1;

            while (lower <= upper)
            {
                index = (upper + lower) / 2;

                int diff = list[index].CompareTo(value);
                if (diff > 0) upper = index - 1;
                else if (diff < 0) lower = index + 1;
                else
                {
                    result = index;
                    upper = index - 1;
                }
            }
            if (result == -1)
            {
                if (index == upper) index += 1;
                return ~index;

            }
            return result;
           
        }
        
        /// <summary>
        /// Binary search to find the last index which equals <paramref name="value"/>.
        /// </summary>
        /// <returns>
        /// The first index of the <paramref name="value"/> in the <paramref name="list"/>. 
        /// If an index is not found, returns a bitwise complement index into which the value 
        /// should be inserted.
        /// </returns>
        public static int BinaryLast<T>(this List<T> list, ComponentSet value) where T :  IComparable<ComponentSet>
        {
            int result = -1;
            int index = 0;
            int lower = 0;
            int upper = list.Count - 1;

            while (lower <= upper)
            {
                index = (upper + lower) / 2;

                int diff = list[index].CompareTo(value);
                if (diff > 0) upper = index - 1;
                else if (diff < 0) lower = index + 1;
                else
                {
                    result = index;
                    lower = index + 1;
                }
            }
            if (result == -1)
            {
                if (index == upper) index += 1;
                return ~index;
            }
            return result;
        }
    
    
        /// <summary>
        /// Assuming the list is sorted for the bit, 
        /// finds the first index the value with an <paramref name="n"/>th bit. 
        /// If no bit is found returns -1.
        /// </summary>
        internal static int FindFirstWithBit(this List<Archetype> list, byte n, int start, int count)
        {
            int result = -1;
            int index = 0;
            int lower = start;
            int upper = start + count - 1;

            while (lower <= upper)
            {
                index = (upper + lower) / 2;
               
                if (list[index].compSet.Contains(n))
                {
                    result = index;
                    upper = index - 1;
                }
                else
                {
                    lower = index + 1;
                }

            }
            return result;
        }
    }
}
