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


            working.Push(new SearchData(255, 0, All.Count)); 
            // starts at most significant bit

            while (working.Count > 0)
            {
                SearchData data = working.Pop();
                
                if (data.id == 0)
                {
                    result.AddRange(All.GetRange(data.start, data.count));
                    continue;
                }

                int index = 0;
                switch(query.GetBit(data.id))
                {




                    case Query.Bit.any:
                        //result.AddRange(All.GetRange(data.start, data.count));
                        throw new NotImplementedException();
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
                                working.Push(new SearchData(data.id, data.start, index - data.start));
                                working.Push(new SearchData(data.id, index, data.count + data.start - index));
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
                                working.Push(new SearchData(data.id, data.start, index - data.start));
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
                                working.Push(new SearchData(data.id, index, data.count + data.start - index));
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
            public int start;
            public int count;
            
            public SearchData(byte id, int start, int count)
            {
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
}