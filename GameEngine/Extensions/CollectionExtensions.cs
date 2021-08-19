using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ListExtensions
{
    internal static class ArrayExtensions
    {
        #region Binary Search
        /// <summary>
        /// Assuming array is sorted, searches for each of the elements in <paramref name="search"/>
        /// using an optimised binary search. 
        /// </summary>
        /// <param name="search">the search parameters being looked for</param>
        /// <returns>the index of each corresponding search parameter. if no value was 
        /// found, index will equal -1. 
        /// </returns>
        public static int[] BinarySearch<T>(this T[] array, params T[] search) where T : IComparable
        {
            // store given order of search array
            List<KeyValuePair<int, T>> paired = new List<KeyValuePair<int, T>>();
            for (int i = 0; i < search.Length; i++)
                paired.Add(new KeyValuePair<int, T>(i, search[i]));

            paired.Sort((x, y) => x.Value.CompareTo(y.Value));

            int[] Indexes = new int[search.Length];

            int array_L = 0;                    // array index lower
            int array_U = array.Length;         // array index upper

            int search_L = 0;                   // search index lower
            int search_U = search.Length;       // search index upper
            do
            {
                // searches for lower bound
                array_L = Array.BinarySearch(array, array_L, array_U - array_L, paired[search_L].Value);

                if (array_L < 0)
                {
                    Indexes[paired[search_L].Key] = -1;
                    array_L = -array_L - 1;
                }
                else
                {
                    Indexes[paired[search_L].Key] = array_L;
                    ++array_L;
                }
                search_L++;

                if (search_L >= search_U || array_L >= array_U) break; // if upper and lower bound have crossed in the middle

                // searches for upper bound
                array_U = Array.BinarySearch(array, array_L, array_U - array_L, paired[search_U - 1].Value);
                if (array_U < 0)
                {
                    Indexes[paired[search_U - 1].Key] = -1;
                    array_U = -array_U - 1;
                }
                else
                {
                    Indexes[paired[search_U - 1].Key] = array_U;
                }
                search_U--;
            }
            while (search_L < search_U && array_L < array_U); // if upper and lower bound have crossed in the middle

            return Indexes;
        }
        /// <summary>
        /// searches for the <paramref name="search"/> parameter using a binary search
        /// </summary>
        public static int BinarySearch<T>(this T[] array, T search)
        {
            int index = Array.BinarySearch(array, search);
            return index < 0 ? -1 : index;
        }
        #endregion

        #region Contains
        /// <summary>
        /// returns if all search parameters are within array 
        /// </summary>
        public static bool Contains<T>(this T[] array, params T[] search)
        {
            Array.Sort(search);

            int[] Indexes = new int[search.Length];

            int array_L = 0;                    // array index lower
            int array_U = array.Length;         // array index upper

            int search_L = 0;                   // search index lower
            int search_U = search.Length;       // search index upper
            do
            {
                // searches for lower bound
                array_L = Array.BinarySearch(array, array_L, array_U - array_L, search[search_L]);

                if (array_L < 0) return false;
                else Indexes[search_L] = array_L++;

                search_L++;

                if (search_L >= search_U || array_L >= array_U) break; // if upper and lower bound have crossed in the middle

                // searches for upper bound
                array_U = Array.BinarySearch(array, array_L, array_U - array_L, search[search_U - 1]);

                if (array_U < 0) return false;
                else Indexes[search_U - 1] = array_U;

                search_U--;
            }
            while (search_L < search_U && array_L < array_U); // if upper and lower bound have crossed in the middle

            return true;
        }
        /// <summary>
        /// returns if this array contains search parameter
        /// </summary>
        public static bool Contains<T>(this T[] array, T search)
        {
            int index = Array.BinarySearch(array, search);
            return index >= 0;
        }
        #endregion
    }
}
