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
    }
}
