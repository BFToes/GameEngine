using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace GameEngine.ECS
{
    public class Filter : IEquatable<Filter>
    {
        public HashSet<byte> Any;
        public HashSet<byte> All;
        public HashSet<byte> None;

        public Filter AnyOf<T>() 
            where T : class, IComponent, new() => AnyOf(ComponentType<T>.ID);
        public Filter AnyOf<T1, T2>() 
            where T1 : class, IComponent, new() 
            where T2 : class, IComponent, new() => AnyOf(ComponentType<T1>.ID, ComponentType<T2>.ID);
        public Filter AnyOf<T1, T2, T3>() 
            where T1 : class, IComponent, new() 
            where T2 : class, IComponent, new() 
            where T3 : class, IComponent, new() => AnyOf(ComponentType<T1>.ID, ComponentType<T2>.ID, ComponentType<T3>.ID);
        public Filter AnyOf<T1, T2, T3, T4>()
            where T1 : class, IComponent, new()
            where T2 : class, IComponent, new()
            where T3 : class, IComponent, new()
            where T4 : class, IComponent, new() => AnyOf(ComponentType<T1>.ID, ComponentType<T2>.ID, ComponentType<T3>.ID, ComponentType<T4>.ID);
        private Filter AnyOf(params byte[] types) 
        {
            HashSet<byte> Any = new HashSet<byte>(); // The null-coalescing operator ?? returns the value of its left-hand operand if it isn't null
            foreach (byte comptype in types)
            {
                Any.Add(comptype);
                _isCached = false;
            }
            return this;
        }

        public Filter AllOf<T1>() 
            where T1 : class, IComponent, new() => AllOf(ComponentType<T1>.ID);
        public Filter AllOf<T1, T2>() 
            where T1 : class, IComponent, new() 
            where T2 : class, IComponent, new() => AllOf(ComponentType<T1>.ID, ComponentType<T2>.ID);
        public Filter AllOf<T1, T2, T3>() 
            where T1 : class, IComponent, new() 
            where T2 : class, IComponent, new() 
            where T3 : class, IComponent, new() => AllOf(ComponentType<T1>.ID, ComponentType<T2>.ID, ComponentType<T3>.ID);
        public Filter AllOf<T1, T2, T3, T4>() 
            where T1 : class, IComponent, new() 
            where T2 : class, IComponent, new() 
            where T3 : class, IComponent, new() 
            where T4 : class, IComponent, new() => AllOf(ComponentType<T1>.ID, ComponentType<T2>.ID, ComponentType<T3>.ID, ComponentType<T4>.ID);
        private Filter AllOf(params byte[] types)
        {
            All = All ?? new HashSet<byte>();
            foreach (byte type in types)
            {
                All.Add(type);
                _isCached = false;
            }
            return this;
        }

        /// <summary>
        /// None of the component types in this array can exist in the archetype
        /// </summary>
        /// <returns>Current filter</returns>
        public Filter NoneOf<T1>() 
            where T1 : class, IComponent, new() => NoneOf(ComponentType<T1>.ID);
        public Filter NoneOf<T1, T2>()
            where T1 : class, IComponent, new()
            where T2 : class, IComponent, new() => NoneOf(ComponentType<T1>.ID, ComponentType<T2>.ID);
        public Filter NoneOf<T1, T2, T3>()
            where T1 : class, IComponent, new()
            where T2 : class, IComponent, new()
            where T3 : class, IComponent, new() => NoneOf(ComponentType<T1>.ID, ComponentType<T2>.ID, ComponentType<T3>.ID);
        public Filter NoneOf<T1, T2, T3, T4>()
            where T1 : class, IComponent, new()
            where T2 : class, IComponent, new()
            where T3 : class, IComponent, new() 
            where T4 : class, IComponent, new() => NoneOf(ComponentType<T1>.ID, ComponentType<T2>.ID, ComponentType<T3>.ID, ComponentType<T4>.ID);

        private Filter NoneOf(params byte[] types)
        {
            None = None ?? new HashSet<byte>();
            foreach (byte type in types)
            {
                None.Add(type);
                _isCached = false;
            }

            return this;
        }

        public Filter Clone()
        {
            Filter filter = new Filter();
            if (Any != null) filter.Any = new HashSet<byte>(Any);
            if (All != null) filter.All = new HashSet<byte>(All);
            if (None != null) filter.None = new HashSet<byte>(None);
            return filter;
        }

        public bool Equals(Filter other)
        {
            if (other == null || other.GetHashCode() != GetHashCode() || other.GetType() != GetType()) return false;
            if (other.All != null && All != null && !other.All.SetEquals(All)) return false;
            if (other.Any != null && Any != null && !other.Any.SetEquals(Any)) return false;

            return other.None == null || None == null || other.None.SetEquals(None); 
        }

        private int _hash;
        private bool _isCached;

        public override int GetHashCode()
        {
            if (_isCached)
                return _hash;

            int hash = GetType().GetHashCode();
            hash = CalculateHash(hash, All, 3, 53);
            hash = CalculateHash(hash, Any, 307, 367);
            hash = CalculateHash(hash, None, 647, 683);

            _hash = hash;
            _isCached = true;

            return _hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CalculateHash(int hash, HashSet<byte> indices, int i1, int i2)
        {
            if (indices == null)
                return hash;

            byte[] indicesArray = indices.ToArray();
            Array.Sort(indicesArray);

            hash = indicesArray.Aggregate(hash, (current, index) => current ^ index * i1);
            hash ^= indices.Count * i2;
            return hash;
        }
    }
}