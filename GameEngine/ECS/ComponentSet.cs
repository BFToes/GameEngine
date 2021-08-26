using System;
using System.Collections;
using System.Collections.Generic;

namespace ECS
{
    public struct Query
    {
        // 256 bits x 2 = 64 bytes
        private readonly ulong[] bits1; // all
        private readonly ulong[] bits2; // none
        internal readonly byte firstBit;
        internal readonly byte lastBit;
        internal bool doAnySearch;

        public Query(byte[] all, byte[] any, byte[] none)
        {
            bits1 = new ulong[4];
            bits2 = new ulong[4];

            foreach (byte allID in all)
                bits1[allID / 64] = bits1[allID / 64] | (1ul << (allID % 64));

            foreach (byte noneID in none)
                bits2[noneID / 64] = bits2[noneID / 64] | (1ul << (noneID % 64));

            foreach (byte anyID in any)
            {
                bits1[anyID / 64] = bits1[anyID / 64] | (1ul << (anyID % 64));
                bits2[anyID / 64] = bits2[anyID / 64] | (1ul << (anyID % 64));
            }

            firstBit = byte.MinValue;
            lastBit = byte.MaxValue;
            foreach (byte id in all)
            {
                if (id > firstBit) firstBit = id;
                if (id < lastBit) lastBit = id;
            }
            foreach (byte id in any)
            {
                if (id > firstBit) firstBit = id;
                if (id < lastBit) lastBit = id;
            }
            foreach (byte id in none)
            {
                if (id > firstBit) firstBit = id;
                if (id < lastBit) lastBit = id;
            }

            doAnySearch = any.Length > 0;
        }

        /// <summary>
        /// gets the query state of <paramref name="ID"/>.
        /// </summary>
        internal Bit GetBit(byte ID)
        {
            int longIndex = ID / 64;
            int bitIndex = ID % 64;
            if ((bits1[longIndex] & (1ul << (bitIndex))) > 0)
                if ((bits2[longIndex] & (1ul << (bitIndex))) > 0)
                    return Bit.any; // has both all and none
                else
                    return Bit.all; // has only all
            else
                if ((bits2[longIndex] & (1ul << (bitIndex))) > 0)
                return Bit.none; // has only none
            else
                return Bit.blank; // has nothing
        }

        /// <summary>
        /// returns true if <paramref name="compSet"/> would pass query check.
        /// </summary>
        public bool Check(ComponentSet compSet)
        {
            bool All =  (((bits1[0] & ~bits2[0]) & compSet.bits[0]) == bits1[0]) && // All
                        (((bits1[1] & ~bits2[1]) & compSet.bits[1]) == bits1[1]) &&
                        (((bits1[2] & ~bits2[2]) & compSet.bits[2]) == bits1[2]) &&
                        (((bits1[3] & ~bits2[3]) & compSet.bits[3]) == bits1[3]);

            bool None = (((bits2[0] & ~bits1[0]) & compSet.bits[0]) == 0) && // None
                        (((bits2[1] & ~bits1[1]) & compSet.bits[1]) == 0) &&
                        (((bits2[2] & ~bits1[2]) & compSet.bits[2]) == 0) &&
                        (((bits2[3] & ~bits1[3]) & compSet.bits[3]) == 0);

            bool Any =  !doAnySearch || (
                        (((bits1[0] & bits2[0]) & compSet.bits[0]) > 0) && // Any
                        (((bits1[1] & bits2[1]) & compSet.bits[1]) > 0) &&
                        (((bits1[2] & bits2[2]) & compSet.bits[2]) > 0) &&
                        (((bits1[3] & bits2[3]) & compSet.bits[3]) > 0));

            return All && Any && None;
        }

        internal enum Bit
        {
            blank,  // 0 0
            all,    // 0 1
            none,   // 1 0
            any,    // 1 1
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