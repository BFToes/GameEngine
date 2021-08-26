using System;
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
            bool All =  ((bits1[0] & compSet.bits[0]) == bits1[0]) && // All
                        ((bits1[1] & compSet.bits[1]) == bits1[1]) &&
                        ((bits1[2] & compSet.bits[2]) == bits1[2]) &&
                        ((bits1[3] & compSet.bits[3]) == bits1[3]);

            bool None = ((bits2[0] & compSet.bits[0]) == 0) && // None
                        ((bits2[1] & compSet.bits[1]) == 0) &&
                        ((bits2[2] & compSet.bits[2]) == 0) &&
                        ((bits2[3] & compSet.bits[3]) == 0);

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

    public static partial class ListExtensions
    {
        /// <summary>
        /// Assuming the list is sorted for the bit, 
        /// finds the first index with an <paramref name="n"/>th bit. 
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
    

        /// <summary>
        /// Finds all <see cref="Archetype">s which apply to this <paramref name="Query"/>.
        /// </summary>
        public static List<Archetype> Search(this List<Archetype> list, Query query)
        {
            List<Archetype> result = new List<Archetype>();
            Stack<SearchData> working = new Stack<SearchData>();
            byte StartingBit = ComponentManager.Count;  // all bits upto last component will be blank
            byte EndingBit = query.lastBit;             // all bits after last query bit dont matter
            
            // starts at most significant bit
            working.Push(new SearchData(StartingBit, 0, list.Count, query.doAnySearch));

            while (working.Count > 0)
            {
                SearchData data = working.Pop();

                int index = 0;
                switch(query.GetBit(data.id))
                {
                    case Query.Bit.any:
                        index = list.FindFirstWithBit(data.id, data.start, data.count);
                        if (index != -1)
                        {
                            if (data.id == EndingBit && data.hasAny)
                                result.AddRange(list.GetRange(index, data.count + data.start - index));
                            else
                            {
                                data.id--;
                                working.Push(new SearchData(data.id, index, data.count + data.start - index, true));
                            }
                        }
                        break;
                    
                    case Query.Bit.blank:       // |---|  |X--|
                        index = list.FindFirstWithBit(data.id, data.start, data.count);
                        if (index != -1)
                        {
                            if (data.id == EndingBit && data.hasAny)
                                result.AddRange(list.GetRange(data.start, data.count));
                            else
                            {
                                data.id--;
                                working.Push(new SearchData(data.id, data.start, index - data.start, data.hasAny));
                                working.Push(new SearchData(data.id, index, data.count + data.start - index, data.hasAny));
                            }
                        }
                        else 
                        {
                            if (data.id == query.lastBit)
                                result.AddRange(list.GetRange(data.start, data.count));
                            else
                            {
                                data.id--;
                                working.Push(data);
                            }
                        }
                        break;
                    
                    case Query.Bit.none:        // |---|   X--
                        index = list.FindFirstWithBit(data.id, data.start, data.count);
                        if (index != -1)
                        {
                            if (data.id == EndingBit && data.hasAny)
                                result.AddRange(list.GetRange(data.start, index - data.start));
                            else 
                            {
                                data.id--;
                                working.Push(new SearchData(data.id, data.start, index - data.start, data.hasAny));
                            }
                        }
                        else
                        {
                            if (data.id == EndingBit && data.hasAny)
                                    result.AddRange(list.GetRange(data.start, data.count));
                            else 
                            {
                                data.id--;
                                working.Push(data); // no bit found means all range is applicable
                            }
                        }
                        break;
                    
                    case Query.Bit.all:         //  ---   |X--| 
                        index = list.FindFirstWithBit(data.id, data.start, data.count);
                        if (index != -1)
                        {
                            if (data.id == EndingBit)
                                    result.AddRange(list.GetRange(index, data.count + data.start - index));
                            else
                            {
                                data.id--;
                                working.Push(new SearchData(data.id, index, data.count + data.start - index, data.hasAny));
                            }
                        }
                        break;

                }
            }

            return result;
        }

        private struct SearchData
        {
            public byte id;
            public bool hasAny;
            public int start;
            public int count;
            
            public SearchData(byte id, int start, int count, bool hasAny)
            {
                this.hasAny = hasAny;
                this.start = start;
                this.count = count;
                this.id = id;
            }
        }

    }
}