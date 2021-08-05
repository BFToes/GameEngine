﻿using System;
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
            this.Whitelist = new HashSet<T>(Whitelist);
            this.Blacklist = new HashSet<T>(Blacklist);
        }
        public bool Check(params T[] Items) =>
            (Whitelist.Count == 0 || (Whitelist.Count > 0 && Whitelist.IsSubsetOf(Items))) &&
            (Blacklist.Count == 0 || (Blacklist.Count > 0 && !Blacklist.Overlaps(Items)));
    }

    public class BehaviourFunction : Behaviour
    {
        public delegate void BehaviourExecute(Entity Entity);
        private readonly BehaviourExecute Function;
        public BehaviourFunction(BehaviourExecute Function) : base(new Filter<byte>()) => this.Function = Function;

        public override void Execute()
        {
            foreach (Archetype A in Archetypes)
                for (int i = 0; i < A.EntityCount; i++)
                    Function(A[i]);
        }
    }
    public class BehaviourFunction<T1> : Behaviour
        where T1 : IComponent, new()
    {
        public delegate void BehaviourExecute(Entity Entity, T1 Component1);
        private readonly BehaviourExecute Function;
        public BehaviourFunction(BehaviourExecute Function) : base(CreateFilter<T1>()) => this.Function = Function;

        public override void Execute()
        {
            foreach (Archetype A in Archetypes)
            {
                ComponentPool<T1> CompPool1 = A.GetComponentPool<T1>();

                for (int i = 0; i < A.EntityCount; i++)
                    Function(A[i], (T1)CompPool1[i]);
            }
        }
    }
    public class BehaviourFunction<T1, T2> : Behaviour
        where T1 : IComponent, new()
        where T2 : IComponent, new()
    {
        public delegate void BehaviourExecute(Entity Entity, T1 Component1, T2 Component2);
        private readonly BehaviourExecute Function;
        public BehaviourFunction(BehaviourExecute Function) : base(CreateFilter<T1>()) => this.Function = Function;

        public override void Execute()
        {
            foreach (Archetype A in Archetypes)
            {
                ComponentPool<T1> CompPool1 = A.GetComponentPool<T1>();
                ComponentPool<T2> CompPool2 = A.GetComponentPool<T2>();

                for (int i = 0; i < A.EntityCount; i++)
                    Function(A[i], (T1)CompPool1[i], (T2)CompPool2[i]);
            }
        }
    }
    public class BehaviourFunction<T1, T2, T3> : Behaviour
        where T1 : IComponent, new()
        where T2 : IComponent, new()
        where T3 : IComponent, new()
    {
        public delegate void BehaviourExecute(Entity Entity, T1 Component1, T2 Component2, T3 Component3);
        private readonly BehaviourExecute Function;
        public BehaviourFunction(BehaviourExecute Function) : base(CreateFilter<T1, T2, T3>()) => this.Function = Function;

        public override void Execute()
        {
            foreach (Archetype A in Archetypes)
            {
                ComponentPool<T1> CompPool1 = A.GetComponentPool<T1>();
                ComponentPool<T2> CompPool2 = A.GetComponentPool<T2>();
                ComponentPool<T3> CompPool3 = A.GetComponentPool<T3>();

                for (int i = 0; i < A.EntityCount; i++)
                    Function(A[i], (T1)CompPool1[i], (T2)CompPool2[i], (T3)CompPool3[i]);
            }
        }
    }
    public class BehaviourFunction<T1, T2, T3, T4> : Behaviour
        where T1 : IComponent, new()
        where T2 : IComponent, new()
        where T3 : IComponent, new()
        where T4 : IComponent, new()
    {
        public delegate void BehaviourExecute(Entity Entity, T1 Component1, T2 Component2, T3 Component3, T4 Component4);
        private readonly BehaviourExecute Function;
        public BehaviourFunction(BehaviourExecute Function) : base(CreateFilter<T1, T2, T3, T4>()) => this.Function = Function;

        public override void Execute()
        {
            foreach (Archetype A in Archetypes)
            {
                ComponentPool<T1> CompPool1 = A.GetComponentPool<T1>();
                ComponentPool<T2> CompPool2 = A.GetComponentPool<T2>();
                ComponentPool<T3> CompPool3 = A.GetComponentPool<T3>();
                ComponentPool<T4> CompPool4 = A.GetComponentPool<T4>();

                for (int i = 0; i < A.EntityCount; i++)
                    Function(A[i], (T1)CompPool1[i], (T2)CompPool2[i], (T3)CompPool3[i], (T4)CompPool4[i]);
            }
        }
    }
    public class BehaviourFunction<T1, T2, T3, T4, T5> : Behaviour
        where T1 : IComponent, new()
        where T2 : IComponent, new()
        where T3 : IComponent, new()
        where T4 : IComponent, new()
        where T5 : IComponent, new()
    {
        public delegate void BehaviourExecute(Entity Entity, T1 Component1, T2 Component2, T3 Component3, T4 Component4, T5 Component5);
        private readonly BehaviourExecute Function;
        public BehaviourFunction(BehaviourExecute Function) : base(CreateFilter<T1, T2, T3, T4, T5>()) => this.Function = Function;

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
                    Function(A[i], (T1)CompPool1[i], (T2)CompPool2[i], (T3)CompPool3[i], (T4)CompPool4[i], (T5)CompPool5[i]);
            }
        }
    }
    public class BehaviourFunction<T1, T2, T3, T4, T5, T6> : Behaviour
        where T1 : IComponent, new()
        where T2 : IComponent, new()
        where T3 : IComponent, new()
        where T4 : IComponent, new()
        where T5 : IComponent, new()
        where T6 : IComponent, new()
    {
        public delegate void BehaviourExecute(Entity Entity, T1 Component1, T2 Component2, T3 Component3, T4 Component4, T5 Component5, T6 Component6);
        private readonly BehaviourExecute Function;
        public BehaviourFunction(BehaviourExecute Function) : base(CreateFilter<T1, T2, T3, T4, T5, T6>()) => this.Function = Function;

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
                    Function(A[i], (T1)CompPool1[i], (T2)CompPool2[i], (T3)CompPool3[i], (T4)CompPool4[i], (T5)CompPool5[i], (T6)CompPool6[i]);
            }
        }
    }
    public class BehaviourFunction<T1, T2, T3, T4, T5, T6, T7> : Behaviour
        where T1 : IComponent, new()
        where T2 : IComponent, new()
        where T3 : IComponent, new()
        where T4 : IComponent, new()
        where T5 : IComponent, new()
        where T6 : IComponent, new()
        where T7 : IComponent, new()
    {
        public delegate void BehaviourExecute(Entity Entity, T1 Component1, T2 Component2, T3 Component3, T4 Component4, T5 Component5, T6 Component6, T7 Component7);
        private readonly BehaviourExecute Function;
        public BehaviourFunction(BehaviourExecute Function) : base(CreateFilter<T1, T2, T3, T4, T5, T6, T7>()) => this.Function = Function;

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
                    Function(A[i], (T1)CompPool1[i], (T2)CompPool2[i], (T3)CompPool3[i], (T4)CompPool4[i], (T5)CompPool5[i], (T6)CompPool6[i], (T7)CompPool7[i]);
            }
        }
    }
    public class BehaviourFunction<T1, T2, T3, T4, T5, T6, T7, T8> : Behaviour
        where T1 : IComponent, new()
        where T2 : IComponent, new()
        where T3 : IComponent, new()
        where T4 : IComponent, new()
        where T5 : IComponent, new()
        where T6 : IComponent, new()
        where T7 : IComponent, new()
        where T8 : IComponent, new()
    {
        public delegate void BehaviourExecute(Entity Entity, T1 Component1, T2 Component2, T3 Component3, T4 Component4, T5 Component5, T6 Component6, T7 Component7, T8 Component8);
        private readonly BehaviourExecute Function;
        public BehaviourFunction(BehaviourExecute Function) : base(CreateFilter<T1, T2, T3, T4, T5, T6, T7, T8>()) => this.Function = Function;

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
                    Function(A[i], (T1)CompPool1[i], (T2)CompPool2[i], (T3)CompPool3[i], (T4)CompPool4[i], (T5)CompPool5[i], (T6)CompPool6[i], (T7)CompPool7[i], (T8)CompPool8[i]);
            }
        }
    }
    
    /// <summary>
    /// A collection of <see cref="Archetype"/>s which fulfils a <see cref="Filter{T}"/> condition. 
    /// Used to perform logic over filtered selection of <see cref="Entity"/>.
    /// </summary>
    public abstract class Behaviour
    {
        public readonly Filter<byte> Filter;
        protected EntityContext Manager;
        protected List<Archetype> Archetypes;

        #region Filter Constructors
        internal static Filter<byte> CreateFilter<T1>()
            where T1 : IComponent, new()
        {
            return new Filter<byte>(new byte[] { 
                ComponentType<T1>.ID 
            });
        }
        internal static Filter<byte> CreateFilter<T1, T2>()
            where T1 : IComponent, new()
            where T2 : IComponent, new()
        {
            return new Filter<byte>(new byte[] { 
                ComponentType<T1>.ID, 
                ComponentType<T2>.ID 
            });
        }
        internal static Filter<byte> CreateFilter<T1, T2, T3>()
            where T1 : IComponent, new()
            where T2 : IComponent, new()
            where T3 : IComponent, new()
        {
            return new Filter<byte>(new byte[] { 
                ComponentType<T1>.ID, 
                ComponentType<T2>.ID, 
                ComponentType<T3>.ID 
            });
        }
        internal static Filter<byte> CreateFilter<T1, T2, T3, T4>()
            where T1 : IComponent, new()
            where T2 : IComponent, new()
            where T3 : IComponent, new()
            where T4 : IComponent, new()
        {
            return new Filter<byte>(new byte[] {
                ComponentType<T1>.ID,
                ComponentType<T2>.ID,
                ComponentType<T3>.ID,
                ComponentType<T4>.ID
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
                ComponentType<T1>.ID,
                ComponentType<T2>.ID,
                ComponentType<T3>.ID,
                ComponentType<T4>.ID,
                ComponentType<T5>.ID,
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
                ComponentType<T1>.ID,
                ComponentType<T2>.ID,
                ComponentType<T3>.ID,
                ComponentType<T4>.ID,
                ComponentType<T5>.ID,
                ComponentType<T6>.ID,
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
                ComponentType<T1>.ID,
                ComponentType<T2>.ID,
                ComponentType<T3>.ID,
                ComponentType<T4>.ID,
                ComponentType<T5>.ID,
                ComponentType<T6>.ID,
                ComponentType<T7>.ID,
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
                ComponentType<T1>.ID,
                ComponentType<T2>.ID,
                ComponentType<T3>.ID,
                ComponentType<T4>.ID,
                ComponentType<T5>.ID,
                ComponentType<T6>.ID,
                ComponentType<T7>.ID,
                ComponentType<T8>.ID,
            });
        }
        #endregion

        public Behaviour(Filter<byte> Filter)
        {
            this.Filter = Filter;
        }
        public void Add(Archetype Archetype) => Archetypes.Add(Archetype);
        public void Remove(Archetype Archetype) => Archetypes.Remove(Archetype);
        public abstract void Execute();
    }


}