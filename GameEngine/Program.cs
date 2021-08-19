using System;
using ECS;
using System.Threading;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ListExtensions;

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

    public struct RenderComponent : IComponent { }

    public struct Component0 : IComponent { public string data { get; set; } }
    public struct Component1 : IComponent { public string data { get; set; } }
    public struct Component2 : IComponent { public string data { get; set; } }
    public struct Component3 : IComponent { public string data { get; set; } }
    public struct Component4 : IComponent { public string data { get; set; } }
    public struct Component5 : IComponent { public string data { get; set; } }
    public struct Component6 : IComponent { public string data { get; set; } }
    public struct Component7 : IComponent { public string data { get; set; } }
    public struct Component8 : IComponent { public string data { get; set; } }

    public class thing : Entity
    {
        public ref Component1 C1 => ref GetComponent<Component1>();

        public thing() : base(Archetype.Get(ComponentManager.ID<Component0, Component1, Component2>())) 
        {
            RemoveComponent<Component2>();
            AddComponent<TransformComponent>();
            AddComponent<Component3>();

            RemoveComponent<TransformComponent>();
        }
    }

    class Program
    {
        static public void Main(string[] args)
        {
            Random rnd = new Random();
            byte[] comps = ComponentManager.ID<Component0, Component1, Component2, Component3>();
            List<Entity> list = new List<Entity>();
            
            Console.WriteLine("begin");

            for (int i = 0; i < 200000; i++)
            {
                var Entity = new thing();
                Entity.SetComponents(comps.Where(x => rnd.Next(256) > 128).OrderBy(x => rnd.Next()).ToArray());
                list.Add(Entity);
            }

            Console.WriteLine("finished constructing in random order");

            foreach (thing thing in list)
                if (!thing.Has<Component1>())
                    thing.AddComponent<Component1>();

            foreach (thing thing in list)
                thing.C1.data = rnd.Next().ToString();

            Console.WriteLine("finished adding random data");

            Console.ReadLine();
        }
    }
}