using ECS.Pool;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ECS
{
    /// <summary>
    /// a filter for components where a black list and white list can be defined
    /// </summary>
    public sealed class Filter
    {
        private HashSet<Type> All;
        private HashSet<Type> Any;
        private HashSet<Type> None;
        
        #region Static Constructors
        /// <summary>
        /// creates a <see cref="Filter"/> from Component types
        /// </summary>
        public static Filter FromType<T1>() 
            where T1 : IComponent, new()
        {
            return new Filter(new Type[] {
                typeof(T1)
            });
        }
        /// <inheritdoc cref="FromType"/>
        public static Filter FromType<T1, T2>() 
            where T1 : IComponent, new()
            where T2 : IComponent, new()
        {
            return new Filter(new Type[] {
                typeof(T1),
                typeof(T2)
            });
        }
        /// <inheritdoc cref="FromType"/>
        public static Filter FromType<T1, T2, T3>() 
            where T1 : IComponent, new()
            where T2 : IComponent, new()
            where T3 : IComponent, new()
        {
            return new Filter(new Type[] {
                typeof(T1),
                typeof(T2),
                typeof(T3)
            });
        }
        /// <inheritdoc cref="FromType"/>
        public static Filter FromType<T1, T2, T3, T4>() 
            where T1 : IComponent, new()
            where T2 : IComponent, new()
            where T3 : IComponent, new()
            where T4 : IComponent, new()
        {
            return new Filter(new Type[] {
                typeof(T1),
                typeof(T2),
                typeof(T3),
                typeof(T4)
            });
        }
        /// <inheritdoc cref="FromType"/>
        public static Filter FromType<T1, T2, T3, T4, T5>() 
            where T1 : IComponent, new()
            where T2 : IComponent, new()
            where T3 : IComponent, new()
            where T4 : IComponent, new()
            where T5 : IComponent, new()
        {
            return new Filter(new Type[] {
                typeof(T1),
                typeof(T2),
                typeof(T3),
                typeof(T4),
                typeof(T5),
            });
        }
        /// <inheritdoc cref="FromType"/>
        public static Filter FromType<T1, T2, T3, T4, T5, T6>() 
            where T1 : IComponent, new()
            where T2 : IComponent, new()
            where T3 : IComponent, new()
            where T4 : IComponent, new()
            where T5 : IComponent, new()
            where T6 : IComponent, new()
        {
            return new Filter(new Type[] {
                typeof(T1),
                typeof(T2),
                typeof(T3),
                typeof(T4),
                typeof(T5),
                typeof(T6),
            });
        }
        /// <inheritdoc cref="FromType"/>
        public static Filter FromType<T1, T2, T3, T4, T5, T6, T7>() 
            where T1 : IComponent, new()
            where T2 : IComponent, new()
            where T3 : IComponent, new()
            where T4 : IComponent, new()
            where T5 : IComponent, new()
            where T6 : IComponent, new()
            where T7 : IComponent, new()
        {
            return new Filter(new Type[] {
                typeof(T1),
                typeof(T2),
                typeof(T3),
                typeof(T4),
                typeof(T5),
                typeof(T6),
                typeof(T7),
            });
        }
        /// <inheritdoc cref="FromType"/>
        public static Filter FromType<T1, T2, T3, T4, T5, T6, T7, T8>() 
            where T1 : IComponent, new()
            where T2 : IComponent, new()
            where T3 : IComponent, new()
            where T4 : IComponent, new()
            where T5 : IComponent, new()
            where T6 : IComponent, new()
            where T7 : IComponent, new()
            where T8 : IComponent, new()
        {
            return new Filter(new Type[] {
                typeof(T1),
                typeof(T2),
                typeof(T3),
                typeof(T4),
                typeof(T5),
                typeof(T6),
                typeof(T7),
                typeof(T8),
            });
        }
        #endregion

        public Filter(Type[] All = null, Type[] Any = null, Type[] None = null)
        {
            this.All = All != null ? new HashSet<Type>(All) : null;
            this.Any = Any != null ? new HashSet<Type>(Any) : null;
            this.None = None != null ? new HashSet<Type>(None) : null;
        }
        public bool Check(IEnumerable<Type> Items) => (All?.IsSubsetOf(Items) ?? false) && (!None?.Overlaps(Items) ?? false) && (Any?.Overlaps(Items) ?? false);
    }

    /// <summary>
    /// A collection of <see cref="Archetype"/>s which fulfils a <see cref="Filter"/> condition. 
    /// Used to perform logic over a filtered collection of <see cref="Entity"/>.
    /// </summary>
    public abstract class Behaviour
    {
        public readonly Filter _filter;
        protected List<Archetype> Archetypes;

        public int Length
        {
            get
            {
                int count = 0;
                foreach (Archetype A in Archetypes)
                    count += A.Length;
                return count;
            }
        }
        public int ArchetypeCount => Archetypes.Count;

        public Behaviour(Filter Filter)
        {
            this._filter = Filter;
            this.Archetypes = new List<Archetype>();
        }

        internal void AddArchetype(Archetype Archetype)
        {
            Archetypes.Add(Archetype);
        }
        internal void RemoveArchetype(Archetype Archetype)
        {
            Archetypes.Remove(Archetype);
        }
    }
    /// <inheritdoc cref="Behaviour"/>
    public sealed class Behaviour<T1> : Behaviour
        where T1 : IComponent, new()
    {
        public delegate void FunctionDelegate(T1 C1);
        public readonly FunctionDelegate Function;
        public Behaviour(FunctionDelegate Function) : base(Filter.FromType<T1>()) 
        { 
            this.Function = Function; 
        }

        public void Update()
        {
            foreach (Archetype A in Archetypes)
            {
                BundlePool<T1> CompPool1 = A.GetPool<T1>();

                for (int i = 0; i < A.Length; i++)
                    Task.Run(() => Function(CompPool1[i]));
                Task.WaitAll();
            }
        }
    }
    /// <inheritdoc cref="Behaviour"/>
    public sealed class Behaviour<T1, T2> : Behaviour
        where T1 : IComponent, new()
        where T2 : IComponent, new()
    {
        public delegate void FunctionDelegate(T1 C1, T2 C2);
        public readonly FunctionDelegate Function;
        public Behaviour(FunctionDelegate Function) : base(Filter.FromType<T1>())
        {
            this.Function = Function;
        }

        public void Update()
        {
            foreach (Archetype A in Archetypes)
            {
                BundlePool<T1> CompPool1 = A.GetPool<T1>();
                BundlePool<T2> CompPool2 = A.GetPool<T2>();


                for (int i = 0; i < A.Length; i++)
                    Function(CompPool1[i], CompPool2[i]);
                Task.WaitAll();
            }
        }
    }
    /// <inheritdoc cref="Behaviour"/>
    public sealed class Behaviour<T1, T2, T3> : Behaviour
        where T1 : IComponent, new()
        where T2 : IComponent, new()
        where T3 : IComponent, new()
    {
        public delegate void FunctionDelegate(T1 C1, T2 C2, T3 C3);
        public readonly FunctionDelegate Function;
        public Behaviour(FunctionDelegate Function) : base(Filter.FromType<T1, T2, T3>())
        {
            this.Function = Function;
        }

        public void Update()
        {
            foreach (Archetype A in Archetypes)
            {
                BundlePool<T1> CompPool1 = A.GetPool<T1>();
                BundlePool<T2> CompPool2 = A.GetPool<T2>();
                BundlePool<T3> CompPool3 = A.GetPool<T3>();

                for (int i = 0; i < A.Length; i++)
                    Task.Run(() => Function(CompPool1[i], CompPool2[i], CompPool3[i]));
                Task.WaitAll();
            }
        }
    }
    /// <inheritdoc cref="Behaviour"/>
    public sealed class Behaviour<T1, T2, T3, T4> : Behaviour
        where T1 : IComponent, new()
        where T2 : IComponent, new()
        where T3 : IComponent, new()
        where T4 : IComponent, new()
    {
        public delegate void FunctionDelegate(T1 C1, T2 C2, T3 C3, T4 C4);
        public readonly FunctionDelegate Function;
        public Behaviour(FunctionDelegate Function) : base(Filter.FromType<T1, T2, T3, T4>())
        {
            this.Function = Function;
        }

        public void Update()
        {
            foreach (Archetype A in Archetypes)
            {
                BundlePool<T1> CompPool1 = A.GetPool<T1>();
                BundlePool<T2> CompPool2 = A.GetPool<T2>();
                BundlePool<T3> CompPool3 = A.GetPool<T3>();
                BundlePool<T4> CompPool4 = A.GetPool<T4>();

                for (int i = 0; i < A.Length; i++)
                    Task.Run(() => Function(CompPool1[i], CompPool2[i], CompPool3[i], CompPool4[i]));
                Task.WaitAll();
            }
        }
    }
    /// <inheritdoc cref="Behaviour"/>
    public sealed class Behaviour<T1, T2, T3, T4, T5> : Behaviour
        where T1 : IComponent, new()
        where T2 : IComponent, new()
        where T3 : IComponent, new()
        where T4 : IComponent, new()
        where T5 : IComponent, new()
    {
        public delegate void FunctionDelegate(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5);
        public readonly FunctionDelegate Function;
        public Behaviour(FunctionDelegate Function) : base(Filter.FromType<T1, T2, T3, T4, T5>())
        {
            this.Function = Function;
        }

        public void Update()
        {
            foreach (Archetype A in Archetypes)
            {
                BundlePool<T1> CompPool1 = A.GetPool<T1>();
                BundlePool<T2> CompPool2 = A.GetPool<T2>();
                BundlePool<T3> CompPool3 = A.GetPool<T3>();
                BundlePool<T4> CompPool4 = A.GetPool<T4>();
                BundlePool<T5> CompPool5 = A.GetPool<T5>();

                for (int i = 0; i < A.Length; i++)
                    Task.Run(() => Function(CompPool1[i], CompPool2[i], CompPool3[i], CompPool4[i], CompPool5[i]));
                Task.WaitAll();
            }
        }
    }
    /// <inheritdoc cref="Behaviour"/>
    public sealed class Behaviour<T1, T2, T3, T4, T5, T6> : Behaviour
        where T1 : IComponent, new()
        where T2 : IComponent, new()
        where T3 : IComponent, new()
        where T4 : IComponent, new()
        where T5 : IComponent, new()
        where T6 : IComponent, new()
    {
        public delegate void FunctionDelegate(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6);
        public readonly FunctionDelegate Function;
        public Behaviour(FunctionDelegate Function) : base(Filter.FromType<T1, T2, T3, T4, T5, T6>())
        {
            this.Function = Function;
        }

        public void Update()
        {
            foreach (Archetype A in Archetypes)
            {
                BundlePool<T1> CompPool1 = A.GetPool<T1>();
                BundlePool<T2> CompPool2 = A.GetPool<T2>();
                BundlePool<T3> CompPool3 = A.GetPool<T3>();
                BundlePool<T4> CompPool4 = A.GetPool<T4>();
                BundlePool<T5> CompPool5 = A.GetPool<T5>();
                BundlePool<T6> CompPool6 = A.GetPool<T6>();

                for (int i = 0; i < A.Length; i++)
                    Task.Run(() => Function(CompPool1[i], CompPool2[i], CompPool3[i], CompPool4[i], CompPool5[i], CompPool6[i]));
                Task.WaitAll();
            }
        }
    }
    /// <inheritdoc cref="Behaviour"/>
    public sealed class Behaviour<T1, T2, T3, T4, T5, T6, T7> : Behaviour
        where T1 : IComponent, new()
        where T2 : IComponent, new()
        where T3 : IComponent, new()
        where T4 : IComponent, new()
        where T5 : IComponent, new()
        where T6 : IComponent, new()
        where T7 : IComponent, new()
    {
        public delegate void FunctionDelegate(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7);
        public readonly FunctionDelegate Function;
        public Behaviour(FunctionDelegate Function) : base(Filter.FromType<T1, T2, T3, T4, T5, T6, T7>())
        {
            this.Function = Function;
        }

        public void Update()
        {
            foreach (Archetype A in Archetypes)
            {
                BundlePool<T1> CompPool1 = A.GetPool<T1>();
                BundlePool<T2> CompPool2 = A.GetPool<T2>();
                BundlePool<T3> CompPool3 = A.GetPool<T3>();
                BundlePool<T4> CompPool4 = A.GetPool<T4>();
                BundlePool<T5> CompPool5 = A.GetPool<T5>();
                BundlePool<T6> CompPool6 = A.GetPool<T6>();
                BundlePool<T7> CompPool7 = A.GetPool<T7>();

                for (int i = 0; i < A.Length; i++)
                    Task.Run(() => Function(CompPool1[i], CompPool2[i], CompPool3[i], CompPool4[i], CompPool5[i], CompPool6[i], CompPool7[i]));
                Task.WaitAll();
            }
        }
    }
    /// <inheritdoc cref="Behaviour"/>
    public sealed class Behaviour<T1, T2, T3, T4, T5, T6, T7, T8> : Behaviour
        where T1 : IComponent, new()
        where T2 : IComponent, new()
        where T3 : IComponent, new()
        where T4 : IComponent, new()
        where T5 : IComponent, new()
        where T6 : IComponent, new()
        where T7 : IComponent, new()
        where T8 : IComponent, new()
    {

        public delegate void FunctionDelegate(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7, T8 C8);
        public readonly FunctionDelegate Function;
        public Behaviour(FunctionDelegate Function) : base(Filter.FromType<T1, T2, T3, T4, T5, T6, T7, T8>())
        {
            this.Function = Function;
        }

        public void Update()
        {
            foreach (Archetype A in Archetypes)
            {
                BundlePool<T1> CompPool1 = A.GetPool<T1>();
                BundlePool<T2> CompPool2 = A.GetPool<T2>();
                BundlePool<T3> CompPool3 = A.GetPool<T3>();
                BundlePool<T4> CompPool4 = A.GetPool<T4>();
                BundlePool<T5> CompPool5 = A.GetPool<T5>();
                BundlePool<T6> CompPool6 = A.GetPool<T6>();
                BundlePool<T7> CompPool7 = A.GetPool<T7>();
                BundlePool<T8> CompPool8 = A.GetPool<T8>();

                for (int i = 0; i < A.Length; i++)
                    Task.Run(() => Function(CompPool1[i], CompPool2[i], CompPool3[i], CompPool4[i], CompPool5[i], CompPool6[i], CompPool7[i], CompPool8[i]));
                Task.WaitAll();
            }
        }
    }
}
