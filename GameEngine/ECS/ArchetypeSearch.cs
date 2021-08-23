using System;
using System.Collections.Generic;

namespace ECS
{
    public partial class Archetype 
    {
        /// <summary>
        /// Finds all archetypes which apply to this <paramref name="Query"/>.
        /// </summary>
        public static List<Archetype> SearchAll(Query query)
        {
            List<Archetype> result = new List<Archetype>();
            Stack<SearchData> working = new Stack<SearchData>();


            working.Push(new SearchData(ComponentManager.Count, 0, All.Count, query.doAnySearch));
            // starts at most significant bit

            while (working.Count > 0)
            {
                SearchData data = working.Pop();

                int index = 0;
                switch(query.GetBit(data.id))
                {




                    case Query.Bit.any:
                        index = All.FindFirstWithBit(data.id, data.start, data.count);
                        if (index != -1)
                        {
                            if (data.id == query.lastBit)
                                result.AddRange(All.GetRange(index, data.count + data.start - index));
                            else
                            {
                                data.id--;
                                working.Push(new SearchData(data.id, index, data.count + data.start - index, true));
                            }
                        }
                        break;

                    
                    
                    
                    
                    
                    case Query.Bit.blank:       // |---|  |X--|
                        index = All.FindFirstWithBit(data.id, data.start, data.count);
                        if (index != -1)
                        {
                            if (data.id == query.lastBit)
                                result.AddRange(All.GetRange(data.start, data.count));
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
                                result.AddRange(All.GetRange(data.start, data.count));
                            else
                            {
                                data.id--;
                                working.Push(data);
                            }
                        }
                        break;


                    
                    
                    
                    
                    case Query.Bit.none:        // |---|   X--
                        index = All.FindFirstWithBit(data.id, data.start, data.count);
                        if (index != -1)
                        {
                            if (data.id == query.lastBit)
                                result.AddRange(All.GetRange(data.start, index - data.start));
                            else 
                            {
                                data.id--;
                                working.Push(new SearchData(data.id, data.start, index - data.start, data.hasAny));
                            }
                        }
                        else 
                        {
                            if (data.id == query.lastBit)
                                result.AddRange(All.GetRange(data.start, data.count));
                            else 
                            {
                                data.id--;
                                working.Push(data); // no bit found means all range is applicable
                            }
                        }
                        break;


                    
                    
                    
                    
                    
                    case Query.Bit.all:         //  ---   |X--| 
                        index = All.FindFirstWithBit(data.id, data.start, data.count);
                        if (index != -1)
                        {
                            if (data.id == query.lastBit)
                                result.AddRange(All.GetRange(index, data.count + data.start - index));
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
                    bits1[allID / 64] = bits1[allID  / 64] | (1ul << (allID % 64));
                
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
            
            internal enum Bit
            {
                blank,  // 0 0
                all,    // 0 1
                none,   // 1 0
                any,    // 1 1
            }
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