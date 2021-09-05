using System;
using ECS;
using GameEngine.Components;
using OpenTK.Mathematics;
namespace GameEngine
{
    class Program
    {
        class TestObject : Entity 
        {
            ref Transform transform => ref GetComponent<Transform>();
            ref Hierarchy hierarchy => ref GetComponent<Hierarchy>();

            public TestObject(Scene scene) : base(scene, ComponentManager.ID<Transform, Hierarchy>()) 
            {
                transform = new Transform(Vector3.Zero, Vector3.Zero, Vector3.One);
                hierarchy = new Hierarchy(null);
            }
        }



        static void Main(string[] args)
        {
            var window = RenderWindow.New(null, false, 600, 600);
            new TestObject(window.scene);
            window.Run();           
        }
    }
}