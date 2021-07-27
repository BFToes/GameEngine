using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace GameEngine.ECS
{
    /*
    public abstract class SpatialComponent : IComponent
    {
        public Matrix4 GlobalTransform
        {
            get => _globalTransform;
            set => SetGlobalTransform(_globalTransform = value);
        }
        public abstract Matrix4 LocalTransform 
        { 
            get; 
        }
        public bool InheritTransform
        {
            get => !(_inheritTransform && ParentTransform is null);
            set => _inheritTransform = value;
        }

        public event Action<Matrix4> SetGlobalTransform = delegate { };
        public abstract event Action<Matrix4> SetLocalTransform;

        private SpatialComponent ParentTransform;
        private Matrix4 _globalTransform;
        private bool _inheritTransform = true;

        public SpatialComponent()
        {
            SetLocalTransform += UpdateGlobalTransform;
        }
        public void UpdateGlobalTransform(Matrix4 _)
        {
            if (InheritTransform) GlobalTransform = LocalTransform * ParentTransform.GlobalTransform;
            else GlobalTransform = LocalTransform; 
        }
        private void OnSetParent(Entity Parent)
        {
            if (!(ParentTransform is null)) ParentTransform.SetGlobalTransform -= UpdateGlobalTransform;

            if (Parent is null) ParentTransform = null;
            else ParentTransform = Parent.GetComponent<SpatialComponent>();

            if (!(ParentTransform is null)) ParentTransform.SetGlobalTransform += UpdateGlobalTransform;

            UpdateGlobalTransform(Matrix4.Zero);
        }
        internal override void Added()
        {
            entity.SetParent += OnSetParent;
        }
    }
    public sealed class Spatial3DComponent : SpatialComponent
    {
        private Matrix4 _mat = Matrix4.Identity;
        private Matrix3 RotMatrix = Matrix3.Identity;
        private Vector3 rotation = Vector3.Zero;
        private Vector3 scale = Vector3.One;
        private Vector3 position = Vector3.Zero;

        public event Action<Vector3> SetRotation = delegate { };
        public event Action<Vector3> SetPosition = delegate { };
        public event Action<Vector3> SetScale = delegate { };
        public override event Action<Matrix4> SetLocalTransform = delegate { };

        public override Matrix4 LocalTransform => _mat;
        public Vector3 Rotation
        {
            set
            {
                value = new Vector3(value.X % 2 * MathF.PI, value.Y % 2 * MathF.PI, value.Z % 2 * MathF.PI);
                RotMatrix = Matrix3.CreateFromQuaternion(Quaternion.FromEulerAngles(value));
                _mat.M11 = Scale.X * RotMatrix.M11; _mat.M21 = Scale.X * RotMatrix.M21; _mat.M31 = Scale.X * RotMatrix.M31; _mat.M41 = Position.X;
                _mat.M12 = Scale.Y * RotMatrix.M12; _mat.M22 = Scale.Y * RotMatrix.M22; _mat.M32 = Scale.Y * RotMatrix.M32; _mat.M42 = Position.Y;
                _mat.M13 = Scale.Z * RotMatrix.M13; _mat.M23 = Scale.Z * RotMatrix.M23; _mat.M33 = Scale.Z * RotMatrix.M33; _mat.M43 = Position.Z;
                rotation = value;

                SetRotation(rotation);
                SetLocalTransform(LocalTransform);
            }
            get => rotation;

        }
        public Vector3 Position
        {
            set
            {
                _mat.M41 = value.X; _mat.M42 = value.Y; _mat.M43 = value.Z;
                position = value;

                SetPosition(position);
                SetLocalTransform(LocalTransform);
            }
            get => position;
        }
        public Vector3 Scale
        {
            set
            {
                _mat.M11 = value.X * RotMatrix.M11; _mat.M21 = value.X * RotMatrix.M21; _mat.M31 = value.X * RotMatrix.M31;
                _mat.M12 = value.Y * RotMatrix.M12; _mat.M22 = value.Y * RotMatrix.M22; _mat.M32 = value.Y * RotMatrix.M32;
                _mat.M13 = value.Z * RotMatrix.M13; _mat.M23 = value.Z * RotMatrix.M23; _mat.M33 = value.Z * RotMatrix.M33;
                scale = value;

                SetScale(scale);
                SetLocalTransform(LocalTransform);
            }
            get => scale;
        }
    }
    public sealed class Spatial2DComponent : SpatialComponent
    {
        private Matrix4 _mat = Matrix4.Identity;
        private float rotation = 0;
        private Vector2 scale = Vector2.One;
        private Vector2 position = Vector2.Zero;

        public event Action<float> SetRotation = delegate { };
        public event Action<Vector2> SetPosition = delegate { };
        public event Action<Vector2> SetScale = delegate { };
        public override event Action<Matrix4> SetLocalTransform = delegate { };

        public override Matrix4 LocalTransform => _mat;
        public float Rotation
        {
            set
            {
                value = value % 2 * MathF.PI;
                Matrix2 RotMatrix = Matrix2.CreateRotation(value);
                _mat.M11 = Scale.X * RotMatrix.M11; _mat.M21 = Scale.X * RotMatrix.M21; _mat.M31 = 0; _mat.M41 = Position.X;
                _mat.M12 = Scale.Y * RotMatrix.M12; _mat.M22 = Scale.Y * RotMatrix.M22; _mat.M32 = 0; _mat.M42 = Position.Y;
                rotation = value;

                SetRotation(rotation);
                SetLocalTransform(LocalTransform);
            }
            get => rotation;

        }
        public Vector2 Position
        {
            set
            {
                _mat.M41 = value.X; _mat.M42 = value.Y; 
                position = value;

                SetPosition(position);
                SetLocalTransform(LocalTransform);
            }
            get => position;
        }
        public Vector2 Scale
        {
            set
            {
                Matrix2 RotMatrix = Matrix2.CreateRotation(rotation);
                _mat.M11 = value.X * RotMatrix.M11; _mat.M21 = value.X * RotMatrix.M21; _mat.M31 = 0; _mat.M41 = Position.X;
                _mat.M12 = value.Y * RotMatrix.M12; _mat.M22 = value.Y * RotMatrix.M22; _mat.M32 = 0; _mat.M42 = Position.Y;
                scale = value;

                SetScale(scale);
                SetLocalTransform(LocalTransform);
            }
            get => scale;
        }
    }
    */
}
