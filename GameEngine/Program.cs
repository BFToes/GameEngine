using System;
using ECS;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Threading;
using System.Linq;


namespace GameEngine
{
    /* NOTES:
     * - Inheritance does not work with components. will be registered as a new component with no relation to the base. this maybe more useful.
     * - Need to adapt existing entities into new component system. 
     * - should try and work out threading at some point.
     */
    /*
    // Render components
    public sealed class CullComponent : IComponent { } // Requires - RenderComponent
    public sealed class CullObserverComponent : IComponent { }
    public sealed class CameraComponent : IComponent { } // Requires - CullObserverComponent
    public sealed class RenderComponent : IComponent { } // Requires - TransformComponent, MeshComponent, LODGroupComponent
    public sealed class LODGroupComponent : IComponent { } // Requires - RenderComponent
    public sealed class MaterialComponent : IComponent { }
    public sealed class MeshComponent : IComponent { }
    // Light components
    public sealed class PointLightComponent : IComponent { } // Requires - TransformComponent, CullObserverComponent
    public sealed class DirectionLightComponent : IComponent { } // Requires - CullObserverComponent
    public sealed class OccluderComponent : IComponent { } // Requires - TransformComponent, MeshComponent, CullComponent
    // animation components
    public sealed class AnimationComponent : IComponent { }
    public sealed class ArticulatedJointComponent : IComponent { }
    public sealed class SkinMeshComponent : IComponent { }
    // collider component
    public sealed class ColliderComponent : IComponent { }
    public sealed class RayCastComponent : IComponent { }

    */

    public struct TransformComponent : IComponent
    {
        private bool _dirtyFlag; 
        
        private Vector3 _position;
        private Quaternion _rotation;
        private Vector3 _scale;

        public Matrix4 Matrix { get; private set; }
        public Vector3 Position 
        {
            get => _position;
            set
            {
                _dirtyFlag = true;
                _position = value;
            }
        }
        public Quaternion Rotation 
        {
            get => _rotation;
            set
            {
                _dirtyFlag = true;
                _rotation = value;
            }
        }
        public Vector3 Scale 
        {
            get => _scale;
            set
            {
                _dirtyFlag = true;
                _scale = value;
            }
        }

        public TransformComponent(Vector3 Scale, Quaternion Rotation, Vector3 Position)
        {
            _dirtyFlag = false;
            _scale = Scale;
            _rotation = Rotation;
            _position = Position;
            Matrix = Matrix4.Identity;
        }

        private static Matrix4 CalculateTransform(Vector3 Position, Quaternion Rotation, Vector3 Scale)
        {
            Matrix4 RotationMatrix = Matrix4.CreateFromQuaternion(Rotation);
            Matrix4 ScaleMatrix = Matrix4.CreateScale(Scale);
            Matrix4 TranslationMatrix = Matrix4.CreateTranslation(Position);
            return RotationMatrix * ScaleMatrix * TranslationMatrix;
        }
        private static Matrix4 CalculateTransform(Matrix4 Base, Vector3 Position, Quaternion Rotation, Vector3 Scale)
        {
            Matrix4 RotationMatrix = Matrix4.CreateFromQuaternion(Rotation);
            Matrix4 ScaleMatrix = Matrix4.CreateScale(Scale);
            Matrix4 TranslationMatrix = Matrix4.CreateTranslation(Position);
            return RotationMatrix * ScaleMatrix * TranslationMatrix * Base;
        }

        
    }

    public struct C0 : IComponent { public long data { get; set; } }
    public struct C1 : IComponent { public long data { get; set; } }
    public struct C2 : IComponent { public long data { get; set; } }
    public struct C3 : IComponent { public long data { get; set; } }
    public struct C4 : IComponent { public long data { get; set; } }
    public struct C5 : IComponent { public long data { get; set; } }
    public struct C6 : IComponent { public long data { get; set; } }
    public struct C7 : IComponent { public long data { get; set; } }
    public struct C8 : IComponent { public long data { get; set; } }

    public class thing : Entity
    {
        public ref C1 C1 => ref GetComponent<C1>();

        public thing() : base(ComponentManager.ID<C0, C1, C2>()) 
        {
            //AddComponent<C3>();
            //RemoveComponent<C2>();
        }
    }

    public class system :  Behaviour
    {
        public system() : base(new byte[] { 1, 4 }, new byte[] { }, new byte[] { })
        { }

        public void Count()
        {
            int count = 0;
            foreach (Archetype A in archetypes)
            {
                Archetype.Pool<Entity> E = (Archetype.Pool<Entity>)A.entities;
                Archetype.Pool<C1> C1s = (Archetype.Pool<C1>)A.components[1];
                Archetype.Pool<C4> C4s = (Archetype.Pool<C4>)A.components[4];

                for (int i = 0; i < A.Length; i++)
                {
                    Console.WriteLine($"{count += 1}  {E[i]}   {C1s[i].data}   {C4s[i].data}");
                }
            }
        }
    }

    class Program
    {
        static public void Main(string[] args)
        {
            ComponentManager.ID<C0>();
            ComponentManager.ID<C1>();
            ComponentManager.ID<C2>();
            ComponentManager.ID<C3>();
            ComponentManager.ID<C4>();
            ComponentManager.ID<C5>();
            ComponentManager.ID<C6>();
            ComponentManager.ID<C7>();
            ComponentManager.ID<C8>();



            Random rng = new Random();
            List<Entity> list = new List<Entity>();
            
            Console.WriteLine("begin");

            var test = new system();
            
            for (int i = 0; i < 2000; i++)
                list.Add(new thing());

            Console.WriteLine("1");

            foreach (thing thing in list)
                thing.ApplyComponents(new ComponentSet(new byte[] { (byte)rng.Next(0, 2), (byte)rng.Next(2, 4), (byte)rng.Next(4, 6), (byte)rng.Next(6, 8) }));

            Console.WriteLine("2");

            foreach (thing thing in list)
                if (!thing.HasComponent<C1>())
                    thing.AddComponent<C1>();


            Console.WriteLine("3");

            var search = Archetype.FindApplicable(new Query(new byte[] { 1, 4 }, new byte[] { }, new byte[] { })).ToList();

            Console.WriteLine("4");

            test.Count();
        
            Console.WriteLine("5");

            Console.ReadLine();
        }
    }
}