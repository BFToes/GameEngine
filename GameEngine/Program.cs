using System;
using ECS;
using System.Threading;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;
using ECS.Pool;

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

    public sealed class TransformComponent : IComponent 
    {
        int IPoolItem.ID { get; set; }

        private bool DirtyFlag = false;

        private Vector3 _position = Vector3.Zero;
        private Quaternion _rotation = Quaternion.Identity;
        private Vector3 _scale = Vector3.One;

        public Matrix4 Matrix { get; private set; }
        public Vector3 Position 
        {
            get => _position;
            set
            {
                DirtyFlag = true;
                _position = value;
            }
        }
        public Quaternion Rotation 
        {
            get => _rotation;
            set
            {
                DirtyFlag = true;
                _rotation = value;
            }
        }
        public Vector3 Scale 
        {
            get => _scale;
            set
            {
                DirtyFlag = true;
                _scale = value;
            }
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

        public sealed class UpdateSystem : Behaviour
        {
            public UpdateSystem() : base(Filter.FromType<TransformComponent>()) { }
            
            public void Update()
            {
                foreach(Archetype A in Archetypes)
                {
                    //ComponentPool<TransformComponent> CompPool1 = A.GetComponentPool<TransformComponent>();
                    
                }


            }
        }
    }

    public class scene : EntityContext
    {
        //private readonly Behaviour<PointLightComponent> LightSystem;
        //private readonly Behaviour<OccluderComponent, MeshComponent> OccluderSystem;
        //private readonly Behaviour<RenderComponent, CullComponent> RenderCullSystem;
        //private readonly TransformComponent.UpdateSystem TransformSystem = new TransformComponent.UpdateSystem();


    }
    public class thing : Entity
    {
        public thing(EntityContext C) : base(C) { }
    }
    class Program
    {
        static public void Main(string[] args)
        {
            var world = new scene();
            var thing1 = new thing(world);
            var thing2 = new thing(world);
            var thing3 = new thing(world);
            var thing4 = new thing(world);
            Console.ReadLine();
        }
    }
    

}