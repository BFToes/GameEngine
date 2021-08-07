using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common; // mouse event
using OpenTK.Windowing.GraphicsLibraryFramework; // mouse button
using System;

using GameEngine.Rendering;
using GameEngine.Resources;
//using GameEngine.Entities;
using Assimp;
//using GameEngine.ECS;
//using GameEngine.ECS.System;
using ECS;

namespace GameEngine
{
    class Program
    {
        class EntityA : Entity
        {
            public EntityA(EntityContext World) : base(World)
            {
                AddA();
                AddB();
                AddC();
                AddD();
                AddE();
                //RemoveC();
                //RemoveA();
                //RemoveD();
                //RemoveE();
                //RemoveB();
                //AddC();
                //AddA();
                //AddB();
                //AddE();
                //AddD();
            }
            public void RemoveA() => RemoveComponent<ComponentA>();
            public void RemoveB() => RemoveComponent<ComponentB>();
            public void RemoveC() => RemoveComponent<ComponentC>();
            public void RemoveD() => RemoveComponent<ComponentD>();
            public void RemoveE() => RemoveComponent<ComponentE>();

            public void AddA() => AddComponent<ComponentA>();
            public void AddB() => AddComponent<ComponentB>();
            public void AddC() => AddComponent<ComponentC>();
            public void AddD() => AddComponent<ComponentD>();
            public void AddE() => AddComponent<ComponentE>();
        }

        class EntityB : Entity
        {
            public EntityB(EntityContext World) : base(World)
            {
                AddA();
                AddB();
                AddC();
                AddD();
                AddE();
                //RemoveC();
                //RemoveA();
                RemoveD();
                RemoveE();
                //RemoveB();
                //AddC();
                //AddA();
                //AddB();
                //AddE();
                //AddD();
            }
            public void RemoveA() => RemoveComponent<ComponentA>();
            public void RemoveB() => RemoveComponent<ComponentB>();
            public void RemoveC() => RemoveComponent<ComponentC>();
            public void RemoveD() => RemoveComponent<ComponentD>();
            public void RemoveE() => RemoveComponent<ComponentE>();

            public void AddA() => AddComponent<ComponentA>();
            public void AddB() => AddComponent<ComponentB>();
            public void AddC() => AddComponent<ComponentC>();
            public void AddD() => AddComponent<ComponentD>();
            public void AddE() => AddComponent<ComponentE>();


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
        class Scene : EntityContext
        {
            public Scene()
            {
                AddBehaviour(B1);
                AddBehaviour(B2);
            }

            public void ExecuteBehaviour1() => B1.Execute();
            public void ExecuteBehaviour2() => B2.Execute();


            public BaseBehaviour B1 = new TestBehaviour1();
            public class TestBehaviour1 : TypedBehaviour<ComponentA, ComponentB, ComponentC, ComponentD, ComponentE>
            {
                public TestBehaviour1() : base() { }
                public override void Function(Entity E, ComponentA C1, ComponentB C2, ComponentC C3, ComponentD C4, ComponentE C5)
                {
                    Console.WriteLine($"Behaviour Acting on Entity with components A, B, C, D, E");
                }
            }
            public BaseBehaviour B2 = new TestBehaviour2();
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
            Wor.Debug();
            Console.ReadLine();

        }
    }



}


