using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ListExtensions
{
    internal static class ListExtensions
    {
        /// <summary>
        /// fills a list with a value
        /// </summary>
        /// <typeparam name="T">type contained in list</typeparam>
        /// <param name="list">the list</param>
        /// <param name="value">the value to fill list with</param>
        /// <returns></returns>
        public static List<T> Fill<T>(this List<T> list, T value = default)
        {
            for (int i = 0; i < list.Capacity; i++) list.Add(value);
            return list;
        }
        /// <summary>
        /// changes the size of list
        /// </summary>
        /// <typeparam name="T">type contained in list</typeparam>
        /// <param name="list">the list</param>
        /// <param name="size">new size of list</param>
        /// <returns></returns>
        public static List<T> Grow<T>(this List<T> list, int size)
        {
            if (size > list.Count)
            {
                if (list.Capacity < size) list.Capacity = size + (size / 2); // size * 1.5
                int count = size - list.Count;

                for (int i = 0; i < count; i++) list.Add(default(T));
            }
            return list;
        }
        /// <summary>
        /// sets index of list to value
        /// </summary>
        /// <typeparam name="T">the type contained by the list</typeparam>
        /// <param name="list">the list</param>
        /// <param name="index">the index of the value to be changed</param>
        /// <param name="value">the new value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSafely<T>(this List<T> list, int index, T value)
        {
            if (index >= list.Count) list.Grow(index + 1);
            list[index] = value;
        }
    }
    
    internal static class ArrayExtensions
    {
        /// <summary>
        /// fills an array with a value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T[] Fill<T>(this T[] Array, T value = default)
        {
            for (int i = 0; i < Array.Length; i++) Array[i] = value;
            return Array;
        }
        /// <summary>
        /// Assuming array is sorted, searches for each of the elements in <paramref name="search"/>
        /// using an optimised binary search. 
        /// </summary>
        /// <param name="search">the search parameters being looked for</param>
        /// <returns>the index of each corresponding search parameter. if no value was 
        /// found, index will equal -1. 
        /// </returns>
        public static int[] BinarySearch<T>(this T[] array, params T[] search)
        {
            Array.Sort(search); 
            List<KeyValuePair<T, int>> paired = new List<KeyValuePair<T, int>>();
            for (int i = 0; i < search.Length; i++)
                paired.Add(new KeyValuePair<T, int>(search[i], i));

            int[] Indexes = new int[search.Length];

            int array_L = 0;                    // array index lower
            int array_U = array.Length;         // array index upper

            int search_L = 0;                   // search index lower
            int search_U = search.Length;       // search index upper
            do
            {
                // searches for lower bound
                array_L = Array.BinarySearch(array, array_L, array_U - array_L, paired[search_L].Key); 
                
                if (array_L < 0) {
                    Indexes[paired[search_L].Value] = -1;
                    array_L = -array_L - 1;
                }
                else {
                    Indexes[paired[search_L].Value] = array_L;
                    ++array_L;
                }
                search_L++;

                if (search_L >= search_U || array_L >= array_U) break; // if upper and lower bound have crossed in the middle

                // searches for upper bound
                array_U = Array.BinarySearch(array, array_L, array_U - array_L, paired[search_U - 1].Key);
                if (array_U < 0) {
                    Indexes[paired[search_U - 1].Value] = -1;
                    array_U = -array_U;
                }
                else {
                    Indexes[paired[search_U - 1].Value] = array_U;
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
        /// <summary>
        /// returns if all search parameters are within array 
        /// </summary>
        public static bool Contains<T>(this T[] array, params T[] search)
        {
            Array.Sort(search); 
            List<KeyValuePair<T, int>> paired = new List<KeyValuePair<T, int>>();
            for (int i = 0; i < search.Length; i++)
                paired.Add(new KeyValuePair<T, int>(search[i], i));

            int[] Indexes = new int[search.Length];

            int array_L = 0;                    // array index lower
            int array_U = array.Length;         // array index upper

            int search_L = 0;                   // search index lower
            int search_U = search.Length;       // search index upper
            do
            {
                // searches for lower bound
                array_L = Array.BinarySearch(array, array_L, array_U - array_L, paired[search_L].Key); 
                
                if (array_L < 0) return false;
                else Indexes[paired[search_L].Value] = array_L++;

                search_L++;

                if (search_L >= search_U || array_L >= array_U) break; // if upper and lower bound have crossed in the middle

                // searches for upper bound
                array_U = Array.BinarySearch(array, array_L, array_U - array_L, paired[search_U - 1].Key);
                
                if (array_U < 0) return false;
                else Indexes[paired[search_U - 1].Value] = array_U;
                
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
    }
}
