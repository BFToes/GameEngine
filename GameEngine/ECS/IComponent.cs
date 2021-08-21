using System;
using System.Collections;
using System.Collections.Generic;

namespace ECS
{
    /// <summary>
    /// A module of data that can attach to entities to provide functionality. 
    /// All data relating to an <see cref="Entity"/> is stored through an <see cref="IComponent"/>.
    /// </summary>
    public interface IComponent : Archetype.IPoolable { }
    



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
            if (_count == byte.MaxValue)
                throw new Exception();

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
    /// a 256 bit number that represents a unique of <see cref="byte"/>s. 
    /// </summary>
    internal struct ComponentSet : IComparable<ComponentSet>
    {
        // 256 bits = 32 bytes
        private readonly ulong[] bits;

        public ComponentSet(byte[] IDs)
        {
            // doesnt need to be sorted
            bits = new ulong[4];
            for (byte j = 0; j < IDs.Length; j++)
                bits[IDs[j] / 64] = bits[IDs[j] / 64] | (1ul << (IDs[j] % 64));
        }

        #region Comparison operators
        public static bool operator >(ComponentSet ID_A, ComponentSet ID_B)
        {
            if (ID_A.bits[3] != ID_B.bits[3]) return ID_A.bits[3] > ID_B.bits[3];
            if (ID_A.bits[2] != ID_B.bits[2]) return ID_A.bits[2] > ID_B.bits[2];
            if (ID_A.bits[1] != ID_B.bits[1]) return ID_A.bits[1] > ID_B.bits[1];
            if (ID_A.bits[0] != ID_B.bits[0]) return ID_A.bits[0] > ID_B.bits[0];
            return false;
        }
        public static bool operator <(ComponentSet ID_A, ComponentSet ID_B)
        {
            if (ID_A.bits[3] != ID_B.bits[3]) return ID_A.bits[3] < ID_B.bits[3];
            if (ID_A.bits[2] != ID_B.bits[2]) return ID_A.bits[2] < ID_B.bits[2];
            if (ID_A.bits[1] != ID_B.bits[1]) return ID_A.bits[1] < ID_B.bits[1];
            if (ID_A.bits[0] != ID_B.bits[0]) return ID_A.bits[0] < ID_B.bits[0];
            return false;
        }
        public static bool operator >=(ComponentSet ID_A, ComponentSet ID_B)
        {
            if (ID_A.bits[3] != ID_B.bits[3]) return ID_A.bits[3] > ID_B.bits[3];
            if (ID_A.bits[2] != ID_B.bits[2]) return ID_A.bits[2] > ID_B.bits[2];
            if (ID_A.bits[1] != ID_B.bits[1]) return ID_A.bits[1] > ID_B.bits[1];
            if (ID_A.bits[0] != ID_B.bits[0]) return ID_A.bits[0] > ID_B.bits[0];
            return true;
        }
        public static bool operator <=(ComponentSet ID_A, ComponentSet ID_B)
        {
            if (ID_A.bits[3] != ID_B.bits[3]) return ID_A.bits[3] < ID_B.bits[3];
            if (ID_A.bits[2] != ID_B.bits[2]) return ID_A.bits[2] < ID_B.bits[2];
            if (ID_A.bits[1] != ID_B.bits[1]) return ID_A.bits[1] < ID_B.bits[1];
            if (ID_A.bits[0] != ID_B.bits[0]) return ID_A.bits[0] < ID_B.bits[0];
            return true;
        }
        public static bool operator ==(ComponentSet ID_A, ComponentSet ID_B)
        {
            return (ID_A.bits[0] == ID_B.bits[0]) &&
                   (ID_A.bits[1] == ID_B.bits[2]) &&
                   (ID_A.bits[2] == ID_B.bits[2]) &&
                   (ID_A.bits[3] == ID_B.bits[3]);
        }
        public static bool operator !=(ComponentSet ID_A, ComponentSet ID_B)
        {
            return (ID_A.bits[0] != ID_B.bits[0]) ||
                   (ID_A.bits[1] != ID_B.bits[2]) ||
                   (ID_A.bits[2] != ID_B.bits[2]) ||
                   (ID_A.bits[3] != ID_B.bits[3]);
        }

        public override bool Equals(object obj)
        {
            return (obj is ComponentSet && (ComponentSet)obj == this);
        }
        public override int GetHashCode()
        {
            throw new Exception("Cannot Hash SetIDs");
            // The Set ID could store all 256 different components and in that case
            // it would need every single bit therefore the 32 bit compression of 
            // the hash would lose data and therefore.
        }

        public int CompareTo(ComponentSet that)
        {
            if (this.bits[0] != that.bits[0]) return this.bits[0].CompareTo(that.bits[0]);
            if (this.bits[1] != that.bits[2]) return this.bits[1].CompareTo(that.bits[1]);
            if (this.bits[2] != that.bits[2]) return this.bits[2].CompareTo(that.bits[2]);
            return this.bits[3].CompareTo(that.bits[3]);
        }
        #endregion

        #region Any/All/None Operations
        public bool Overlaps(ComponentSet Mask)
        {
            // Must have atleast 1 but from Mask
            return ((Mask.bits[0] & bits[0]) > 0) &&
                   ((Mask.bits[1] & bits[1]) > 0) &&
                   ((Mask.bits[2] & bits[2]) > 0) &&
                   ((Mask.bits[3] & bits[3]) > 0);

        }
        public bool Contains(ComponentSet Mask)
        {
            return ((Mask.bits[0] & bits[0]) == Mask.bits[0]) &&
                   ((Mask.bits[1] & bits[1]) == Mask.bits[1]) &&
                   ((Mask.bits[2] & bits[2]) == Mask.bits[2]) &&
                   ((Mask.bits[3] & bits[3]) == Mask.bits[3]);
        }
        
        public bool Contains(byte CompID) 
        {
            return (bits[CompID / 64] & (1ul << (CompID % 64))) > 0;
        }
        #endregion

        public override string ToString()
        {
            return $"{BitConverter.ToString(BitConverter.GetBytes(bits[0]))}   " +
                   $"{BitConverter.ToString(BitConverter.GetBytes(bits[1]))}   " +
                   $"{BitConverter.ToString(BitConverter.GetBytes(bits[2]))}   " +
                   $"{BitConverter.ToString(BitConverter.GetBytes(bits[3]))}";
        }
    }




    public static class ListExtensions 
    {
        internal static int Search(this List<Archetype> list, ComponentSet compSet)
        {        
            int index = 0;
            int lower = 0;
            int upper = list.Count - 1;

            while (lower < upper)
            {
                index = (upper + lower + 1) / 2;
                
                int diff = list[index]._compSet.CompareTo(compSet);
                if (diff > 0) upper = index - 1;
                else if (diff < 0) lower = index + 1;
                else return index;
            }
            return ~index;
        }
        internal static int Search(this List<Archetype.Group> list, ComponentSet compSet)
        {        
            int index = 0;
            int lower = 0;
            int upper = list.Count - 1;

            while (lower < upper)
            {
                index = (upper + lower + 1) / 2;
                
                int diff = list[index]._allFilter.CompareTo(compSet);
                if (diff > 0) upper = index - 1;
                else if (diff < 0) lower = index + 1;
                else return index;
            }
            return ~index;
        }
    }
}
