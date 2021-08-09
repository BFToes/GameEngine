using System;
using ECS;
using System.Threading;

namespace GameEngine
{
    /* NOTES:
     * - Inheritance does not work with components. will be registered as a new component with no relation to the base.
     * 
     */
    class Program
    {
        class EntityA : Entity
        {
            public EntityA(EntityContext World) : base(World)
            {
                //AddComponent<ComponentA>();
                AddComponent<ComponentB>();
                AddComponent<ComponentC>();
                AddComponent<ComponentD>();
                AddComponent<ComponentE>();
            }
        }

        class EntityB : Entity
        {
            public EntityB(EntityContext World) : base(World)
            {
                AddComponent<ComponentA>();
                AddComponent<ComponentB>();
                AddComponent<ComponentC>();
            }
        }


        class ComponentA : IComponent 
        {
            public int A;
        }
        class ComponentB : IComponent 
        {
            public int B;
        }
        class ComponentC : IComponent 
        {
            public int C;
        }
        class ComponentD : IComponent 
        {
            public int D;
        }
        class ComponentE : IComponent
        {
            public int E, F, G;
        }
        [Serializable]
        class Scene : EntityContext
        {
            public Scene()
            {
                AddBehaviour(B1);
            }
            public void AddBehaviour2() => AddBehaviour(B2);
            public void ExecuteBehaviour1() => B1.Execute();
            public void ExecuteBehaviour2() => B2.Execute();


            public Behaviour B1 = new TestBehaviour1();
            public class TestBehaviour1 : TypedBehaviour<ComponentA>
            {
                public TestBehaviour1() : base() { }
                public override void Function(Entity E, ComponentA C1)
                {
                    Console.WriteLine($"Behaviour Acting on Entity with components A, B, C, D, E");
                }
            }
            public Behaviour B2 = new TestBehaviour2();
            public class TestBehaviour2 : TypedBehaviour<ComponentA, ComponentB, ComponentC>
            {
                public TestBehaviour2() : base() { }
                public override void Function(Entity E, ComponentA C1, ComponentB C2, ComponentC C3)
                {
                    Console.WriteLine($"Behaviour Acting on Entity with components A, B, C");
                }
            }
        }

        static void Main(string[] _)
        {

            Scene Wor = new Scene();
            for (int i = 0; i < 3; i++)
                new EntityA(Wor);
            for (int i = 0; i < 2; i++)
                new EntityB(Wor);
            
            Console.Write($"Behaviour 1 EntityCount : {Wor.B1.EntityCount}\nBehaviour 2 EntityCount : {Wor.B2.EntityCount}\n");
            Console.ReadLine();
        }
    }
}