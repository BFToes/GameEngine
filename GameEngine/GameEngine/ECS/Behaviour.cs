using System;
using System.Collections.Generic;
using System.Text;

namespace ECS
{
    public class Filter<T>
    {
        private readonly HashSet<T> Whitelist;
        private readonly HashSet<T> Blacklist;

        public Filter(T[] Whitelist = default, T[] Blacklist = default)
        {
            this.Whitelist = Whitelist != null ? new HashSet<T>(Whitelist) : null;
            this.Blacklist = Blacklist != null ? new HashSet<T>(Blacklist) : new HashSet<T>();
        }
        public bool Check(params T[] Items) =>
            (Whitelist.Count == 0 || (Whitelist.Count > 0 && Whitelist.IsSubsetOf(Items))) &&
            (Blacklist.Count == 0 || (Blacklist.Count > 0 && !Blacklist.Overlaps(Items)));
    }

    public abstract class TypedBehaviour : Behaviour
    {
        public TypedBehaviour() : base(new Filter<byte>()){ }

        public override void Execute()
        {
            foreach (Archetype A in Archetypes)
                for (int i = 0; i < A.EntityCount; i++)
                    Function(A[i]);
        }
        public abstract void Function(Entity E);
    }
    public abstract class TypedBehaviour<T1> : Behaviour
        where T1 : IComponent, new()
    {
        public TypedBehaviour() : base(CreateFilter<T1>()) { }

        public override void Execute()
        {
            foreach (Archetype A in Archetypes)
            {
                ComponentPool<T1> CompPool1 = A.GetComponentPool<T1>();

                for (int i = 0; i < A.EntityCount; i++)
                    Function(A[i], CompPool1[i]);
            }
        }
        public abstract void Function(Entity E, T1 Component1); 
    }
    public abstract class TypedBehaviour<T1, T2> : Behaviour
        where T1 : IComponent, new()
        where T2 : IComponent, new()
    {
        public TypedBehaviour() : base(CreateFilter<T1>()) { }

        public override void Execute()
        {
            foreach (Archetype A in Archetypes)
            {
                ComponentPool<T1> CompPool1 = A.GetComponentPool<T1>();
                ComponentPool<T2> CompPool2 = A.GetComponentPool<T2>();

                for (int i = 0; i < A.EntityCount; i++)
                    Function(A[i], CompPool1[i], CompPool2[i]);
            }
        }
        public abstract void Function(Entity E, T1 C1, T2 C2);
    }
    public abstract class TypedBehaviour<T1, T2, T3> : Behaviour
        where T1 : IComponent, new()
        where T2 : IComponent, new()
        where T3 : IComponent, new()
    {
        public TypedBehaviour() : base(CreateFilter<T1, T2, T3>()) { }

        public override void Execute()
        {
            foreach (Archetype A in Archetypes)
            {
                ComponentPool<T1> CompPool1 = A.GetComponentPool<T1>();
                ComponentPool<T2> CompPool2 = A.GetComponentPool<T2>();
                ComponentPool<T3> CompPool3 = A.GetComponentPool<T3>();

                for (int i = 0; i < A.EntityCount; i++)
                    Function(A[i], CompPool1[i], CompPool2[i], CompPool3[i]);
            }
        }
        public abstract void Function(Entity E, T1 C1, T2 C2, T3 C3);
    }
    public abstract class TypedBehaviour<T1, T2, T3, T4> : Behaviour
        where T1 : IComponent, new()
        where T2 : IComponent, new()
        where T3 : IComponent, new()
        where T4 : IComponent, new()
    {
        public TypedBehaviour() : base(CreateFilter<T1, T2, T3, T4>()) { }

        public override void Execute()
        {
            foreach (Archetype A in Archetypes)
            {
                ComponentPool<T1> CompPool1 = A.GetComponentPool<T1>();
                ComponentPool<T2> CompPool2 = A.GetComponentPool<T2>();
                ComponentPool<T3> CompPool3 = A.GetComponentPool<T3>();
                ComponentPool<T4> CompPool4 = A.GetComponentPool<T4>();

                for (int i = 0; i < A.EntityCount; i++)
                    Function(A[i], CompPool1[i], CompPool2[i], CompPool3[i], CompPool4[i]);
            }
        }
        public abstract void Function(Entity E, T1 C1, T2 C2, T3 C3, T4 C4);
    }
    public abstract class TypedBehaviour<T1, T2, T3, T4, T5> : Behaviour
        where T1 : IComponent, new()
        where T2 : IComponent, new()
        where T3 : IComponent, new()
        where T4 : IComponent, new()
        where T5 : IComponent, new()
    {
        public TypedBehaviour() : base(CreateFilter<T1, T2, T3, T4, T5>()){ }

        public override void Execute()
        {
            foreach (Archetype A in Archetypes)
            {
                ComponentPool<T1> CompPool1 = A.GetComponentPool<T1>();
                ComponentPool<T2> CompPool2 = A.GetComponentPool<T2>();
                ComponentPool<T3> CompPool3 = A.GetComponentPool<T3>();
                ComponentPool<T4> CompPool4 = A.GetComponentPool<T4>();
                ComponentPool<T5> CompPool5 = A.GetComponentPool<T5>();

                for (int i = 0; i < A.EntityCount; i++)
                    Function(A[i], CompPool1[i], CompPool2[i], CompPool3[i], CompPool4[i], CompPool5[i]);
            }
        }
        public abstract void Function(Entity E, T1 C1, T2 C2, T3 C3, T4 C4, T5 C5);
    }
    public abstract class TypedBehaviour<T1, T2, T3, T4, T5, T6> : Behaviour
        where T1 : IComponent, new()
        where T2 : IComponent, new()
        where T3 : IComponent, new()
        where T4 : IComponent, new()
        where T5 : IComponent, new()
        where T6 : IComponent, new()
    {
        public TypedBehaviour() : base(CreateFilter<T1, T2, T3, T4, T5, T6>()){ }

        public override void Execute()
        {
            foreach (Archetype A in Archetypes)
            {
                ComponentPool<T1> CompPool1 = A.GetComponentPool<T1>();
                ComponentPool<T2> CompPool2 = A.GetComponentPool<T2>();
                ComponentPool<T3> CompPool3 = A.GetComponentPool<T3>();
                ComponentPool<T4> CompPool4 = A.GetComponentPool<T4>();
                ComponentPool<T5> CompPool5 = A.GetComponentPool<T5>();
                ComponentPool<T6> CompPool6 = A.GetComponentPool<T6>();

                for (int i = 0; i < A.EntityCount; i++)
                    Function(A[i], CompPool1[i], CompPool2[i], CompPool3[i], CompPool4[i], CompPool5[i], CompPool6[i]);
            }
        }
        public abstract void Function(Entity E, T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6);
    }
    public abstract class TypedBehaviour<T1, T2, T3, T4, T5, T6, T7> : Behaviour
        where T1 : IComponent, new()
        where T2 : IComponent, new()
        where T3 : IComponent, new()
        where T4 : IComponent, new()
        where T5 : IComponent, new()
        where T6 : IComponent, new()
        where T7 : IComponent, new()
    {
        public TypedBehaviour() : base(CreateFilter<T1, T2, T3, T4, T5, T6, T7>()){ }

        public override void Execute()
        {
            foreach (Archetype A in Archetypes)
            {
                ComponentPool<T1> CompPool1 = A.GetComponentPool<T1>();
                ComponentPool<T2> CompPool2 = A.GetComponentPool<T2>();
                ComponentPool<T3> CompPool3 = A.GetComponentPool<T3>();
                ComponentPool<T4> CompPool4 = A.GetComponentPool<T4>();
                ComponentPool<T5> CompPool5 = A.GetComponentPool<T5>();
                ComponentPool<T6> CompPool6 = A.GetComponentPool<T6>();
                ComponentPool<T7> CompPool7 = A.GetComponentPool<T7>();

                for (int i = 0; i < A.EntityCount; i++)
                    Function(A[i], CompPool1[i], CompPool2[i], CompPool3[i], CompPool4[i], CompPool5[i], CompPool6[i], CompPool7[i]);
            }
        }
        public abstract void Function(Entity E, T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7);
    }
    public abstract class TypedBehaviour<T1, T2, T3, T4, T5, T6, T7, T8> : Behaviour
        where T1 : IComponent, new()
        where T2 : IComponent, new()
        where T3 : IComponent, new()
        where T4 : IComponent, new()
        where T5 : IComponent, new()
        where T6 : IComponent, new()
        where T7 : IComponent, new()
        where T8 : IComponent, new()
    {
        public TypedBehaviour() : base(CreateFilter<T1, T2, T3, T4, T5, T6, T7, T8>()){ }

        public override void Execute()
        {
            foreach (Archetype A in Archetypes)
            {
                ComponentPool<T1> CompPool1 = A.GetComponentPool<T1>();
                ComponentPool<T2> CompPool2 = A.GetComponentPool<T2>();
                ComponentPool<T3> CompPool3 = A.GetComponentPool<T3>();
                ComponentPool<T4> CompPool4 = A.GetComponentPool<T4>();
                ComponentPool<T5> CompPool5 = A.GetComponentPool<T5>();
                ComponentPool<T6> CompPool6 = A.GetComponentPool<T6>();
                ComponentPool<T7> CompPool7 = A.GetComponentPool<T7>();
                ComponentPool<T8> CompPool8 = A.GetComponentPool<T8>();

                for (int i = 0; i < A.EntityCount; i++)
                    Function(A[i], CompPool1[i], CompPool2[i], CompPool3[i], CompPool4[i], CompPool5[i], CompPool6[i], CompPool7[i], CompPool8[i]);
            }
        }
        public abstract void Function(Entity E, T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7, T8 C8);
    }

    /// <summary>
    /// A collection of <see cref="Archetype"/>s which fulfils a <see cref="Filter{T}"/> condition. 
    /// Used to perform logic over filtered selection of <see cref="Entity"/>.
    /// </summary>
    public abstract class Behaviour
    {
        public readonly Filter<byte> Filter;
        protected List<Archetype> Archetypes;

        public int EntityCount
        {
            get
            {
                int count = 0;
                foreach(Archetype A in Archetypes)
                    count += A.EntityCount;
                return count;
            }
        }
        public int ArchetypeCount => Archetypes.Count;

        #region Filter Constructors
        internal static Filter<byte> CreateFilter<T1>()
            where T1 : IComponent, new()
        {
            return new Filter<byte>(new byte[] {
                ComponentManager.ID<T1>()
            });
        }
        internal static Filter<byte> CreateFilter<T1, T2>()
            where T1 : IComponent, new()
            where T2 : IComponent, new()
        {
            return new Filter<byte>(new byte[] {
                ComponentManager.ID<T1>(),
                ComponentManager.ID<T2>()
            });
        }
        internal static Filter<byte> CreateFilter<T1, T2, T3>()
            where T1 : IComponent, new()
            where T2 : IComponent, new()
            where T3 : IComponent, new()
        {
            return new Filter<byte>(new byte[] {
                ComponentManager.ID<T1>(),
                ComponentManager.ID<T2>(),
                ComponentManager.ID<T3>()
            });
        }
        internal static Filter<byte> CreateFilter<T1, T2, T3, T4>()
            where T1 : IComponent, new()
            where T2 : IComponent, new()
            where T3 : IComponent, new()
            where T4 : IComponent, new()
        {
            return new Filter<byte>(new byte[] {
                ComponentManager.ID<T1>(),
                ComponentManager.ID<T2>(),
                ComponentManager.ID<T3>(),
                ComponentManager.ID<T4>()
            });
        }
        internal static Filter<byte> CreateFilter<T1, T2, T3, T4, T5>()
            where T1 : IComponent, new()
            where T2 : IComponent, new()
            where T3 : IComponent, new()
            where T4 : IComponent, new()
            where T5 : IComponent, new()
        {
            return new Filter<byte>(new byte[] {
                ComponentManager.ID<T1>(),
                ComponentManager.ID<T2>(),
                ComponentManager.ID<T3>(),
                ComponentManager.ID<T4>(),
                ComponentManager.ID<T5>(),
            });
        }
        internal static Filter<byte> CreateFilter<T1, T2, T3, T4, T5, T6>()
            where T1 : IComponent, new()
            where T2 : IComponent, new()
            where T3 : IComponent, new()
            where T4 : IComponent, new()
            where T5 : IComponent, new()
            where T6 : IComponent, new()
        {
            return new Filter<byte>(new byte[] {
                ComponentManager.ID<T1>(),
                ComponentManager.ID<T2>(),
                ComponentManager.ID<T3>(),
                ComponentManager.ID<T4>(),
                ComponentManager.ID<T5>(),
                ComponentManager.ID<T6>(),
            });
        }
        internal static Filter<byte> CreateFilter<T1, T2, T3, T4, T5, T6, T7>()
            where T1 : IComponent, new()
            where T2 : IComponent, new()
            where T3 : IComponent, new()
            where T4 : IComponent, new()
            where T5 : IComponent, new()
            where T6 : IComponent, new()
            where T7 : IComponent, new()
        {
            return new Filter<byte>(new byte[]
            {
                ComponentManager.ID<T1>(),
                ComponentManager.ID<T2>(),
                ComponentManager.ID<T3>(),
                ComponentManager.ID<T4>(),
                ComponentManager.ID<T5>(),
                ComponentManager.ID<T6>(),
                ComponentManager.ID<T7>(),
            });
        }
        internal static Filter<byte> CreateFilter<T1, T2, T3, T4, T5, T6, T7, T8>()
            where T1 : IComponent, new()
            where T2 : IComponent, new()
            where T3 : IComponent, new()
            where T4 : IComponent, new()
            where T5 : IComponent, new()
            where T6 : IComponent, new()
            where T7 : IComponent, new()
            where T8 : IComponent, new()
        {
            return new Filter<byte>(new byte[]
            {
                ComponentManager.ID<T1>(),
                ComponentManager.ID<T2>(),
                ComponentManager.ID<T3>(),
                ComponentManager.ID<T4>(),
                ComponentManager.ID<T5>(),
                ComponentManager.ID<T6>(),
                ComponentManager.ID<T7>(),
                ComponentManager.ID<T8>(),
            });
        }
        #endregion

        public Behaviour(Filter<byte> Filter)
        {
            this.Filter = Filter;
            this.Archetypes = new List<Archetype>();
        }

        internal void AddArchetype(Archetype Archetype)
        {
            Archetypes.Add(Archetype);
            Archetype.Destroy += RemoveArchetype;
        }
        private void RemoveArchetype(Archetype Archetype)
        {
            Archetypes.Remove(Archetype);
            Archetype.Destroy -= RemoveArchetype;
        }
        public abstract void Execute();
    }
}
