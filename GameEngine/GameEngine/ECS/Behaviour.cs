using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ListExtensions;
namespace ECS
{
    public abstract partial class Entity
    {
        /// <summary>
        /// A collection of <see cref="Archetype"/>s. All containing an Archetype.
        /// Used to perform logic over a filtered collection of <see cref="Entity"/>.
        /// </summary>
        public abstract class Behaviour
        {
            protected List<Entity.Archetype> archetypes;
            private readonly byte[] _filter;
            public int Count
            {
                get
                {
                    int count = 0;
                    foreach (Entity.Archetype A in archetypes)
                        count += A.Length;
                    return count;
                }
            }
            public int ArchetypeCount => archetypes.Count;

            public Behaviour(params byte[] Filter)
            {
                this._filter = Filter;
                this.archetypes = new List<Archetype>();
            }

            public virtual void AddIfApplicable(Archetype A)
            {
                if (A.GetComponentIDs().Contains(_filter))
                    archetypes.Add(A);
            }
        }

        /// <inheritdoc cref="Behaviour"/>
        public sealed class Behaviour<T1> : Behaviour
            where T1 : IComponent, new()
        {
            public delegate void FunctionDelegate(Entity E, T1 C1);
            public readonly FunctionDelegate Function;
            public Behaviour(FunctionDelegate Function) : base(ComponentManager.ID<T1>())
            {
                this.Function = Function;
            }

            public void Update()
            {
                foreach (Archetype A in archetypes)
                {
                    Archetype.Pool<Entity> P0 = A.GetPool<T1>(out var P1);

                    for (int i = 0; i < A.Length; i++)
                        Task.Run(() => Function(P0[i], P1[i]));
                    Task.WaitAll();
                }
            }
        }
        /// <inheritdoc cref="Behaviour"/>
        public sealed class Behaviour<T1, T2> : Behaviour
            where T1 : IComponent, new()
            where T2 : IComponent, new()
        {
            public delegate void FunctionDelegate(Entity E, T1 C1, T2 C2);
            public readonly FunctionDelegate Function;
            public Behaviour(FunctionDelegate Function) : base(ComponentManager.ID<T1>(), ComponentManager.ID<T2>())
            {
                this.Function = Function;
            }

            public void Update()
            {
                foreach (Archetype A in archetypes)
                {
                    var P0 = A.GetPools<T1, T2>(out var P1, out var P2);
                    for (int i = 0; i < A.Length; i++)
                        Task.Run(() => Function(P0[i], P1[i], P2[i]));
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
            public delegate void FunctionDelegate(Entity E, T1 C1, T2 C2, T3 C3);
            public readonly FunctionDelegate Function;
            public Behaviour(FunctionDelegate Function) : base(ComponentManager.ID<T1>(), ComponentManager.ID<T2>(), ComponentManager.ID<T3>())
            {
                this.Function = Function;
            }

            public void Update()
            {
                foreach (Archetype A in archetypes)
                {
                    var P0 = A.GetPools<T1, T2, T3>(out var P1, out var P2, out var P3);

                    for (int i = 0; i < A.Length; i++)
                        Task.Run(() => Function(P0[i], P1[i], P2[i], P3[i]));
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
            public delegate void FunctionDelegate(Entity E, T1 C1, T2 C2, T3 C3, T4 C4);
            public readonly FunctionDelegate Function;
            public Behaviour(FunctionDelegate Function) : base(ComponentManager.ID<T1>(), ComponentManager.ID<T2>(), ComponentManager.ID<T3>(), ComponentManager.ID<T4>())
            {
                this.Function = Function;
            }

            public void Update()
            {
                foreach (Archetype A in archetypes)
                {
                    var P0 = A.GetPools<T1, T2, T3, T4>(out var P1, out var P2, out var P3, out var P4);

                    for (int i = 0; i < A.Length; i++)
                        Task.Run(() => Function(P0[i], P1[i], P2[i], P3[i], P4[i]));
                    Task.WaitAll();
                }
            }
        }
    }
}
