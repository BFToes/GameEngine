using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
namespace ECS
{
    /// <summary>
    /// A collection of <see cref="Archetype"/>s containing a specification of components.
    /// Used to perform logic over a filtered collection of <see cref="Entity"/>.
    /// </summary>
    public abstract class Behaviour
    {
        public static List<Behaviour> All;

        static Behaviour() {
            All = new List<Behaviour>();
        }


        protected List<Archetype> archetypes;
        private readonly byte[] _filter;
        public int ArchetypeCount => archetypes.Count;
        
        public Behaviour(params byte[] Filter)
        {
            this._filter = Filter;
            this.archetypes = new List<Archetype>();
        }
        public void AddArchetype() 
        {

        }
        public void TestFunction<T1, T2>(Entity Entity, T1 C1, T2 C2)
            where T1 : IComponent, new()
            where T2 : IComponent, new() 
        {

        }

    }


    public class Behaviour<T1> : Behaviour
        where T1 : IComponent, new()
    {
        public delegate void BehaviourFunc(ref Entity E, ref T1 C1);
        private readonly BehaviourFunc _function;
        
        
        public Behaviour(BehaviourFunc Function) : base(ComponentManager.ID<T1>())
        {
            this._function = Function;
        }
        
        public virtual void Update()
        {
            foreach (Archetype A in archetypes)
            {
                var En = A.GetPool();
                var C1 = A.GetPool<T1>();
                for (int i = 0; i < A.Length; i++)
                    Task.Run(() => _function(ref En[i], ref C1[i]));
                Task.WaitAll();
            }
        }
    }

    public class SortedBehaviour<T1> : Behaviour<T1>
        where T1 : IComponent, new()
    {
        public delegate int  BehaviourSort(Entity E, T1 C1);
        private readonly BehaviourSort _sortFunc;
        private List<KeyValuePair<int, int>> _sortedIndexes;
        
        private bool ReSortFlag = true;

        public SortedBehaviour(BehaviourFunc Function, BehaviourSort SortFunc) : base(Function)
        {
            this._sortFunc = SortFunc;
        }
        public override void Update()
        {
            // sorty stuff
            
        }
        public virtual void Sort() 
        {
            if (!ReSortFlag) return;
            _sortedIndexes = archetypes
                .SelectMany((A, i1) => Enumerable.Range(0, archetypes[i1].Length)
                .Select(i2 => new KeyValuePair<int, int>(i1, i2)))
                .OrderBy((i) => _sortFunc(archetypes[i.Key].GetPool()[i.Value], 
                                            archetypes[i.Key].GetPool<T1>()[i.Value]))
                .ToList();

        }
    }
}
