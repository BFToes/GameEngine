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

        class ComponentA : IComponent 
        {
            public int A = 0;
        }
        class ComponentB : IComponent 
        {
            public int B = 1;
        }
        class ComponentC : IComponent 
        {
            public int C = 2;
        }
        class ComponentD : IComponent 
        {
            public int D = 3;
        }
        class ComponentE : IComponent
        {
            public int E = 4;
            public int F = 5;
            public int G = 6;
        }
        class Scene : EntityContext
        {
            public Scene()
            {
                
            }
        }

        static void Main(string[] _)
        {

            Scene Wor = new Scene();
            for (int i = 0; i < 1; i++)
                new EntityA(Wor);

            Console.ReadLine();

        }
    }



}


