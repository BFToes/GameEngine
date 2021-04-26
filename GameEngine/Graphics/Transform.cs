using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace Graphics
{
    interface ITransform
    {
        public Matrix4 Matrix { get; }
    }
    public class Transform2D : ITransform
    {
        private float rotation = 0;
        private Vector2 scale = Vector2.One;
        private Vector2 position = Vector2.Zero;
        public Matrix4 Matrix { get; private set; }
        public float Rotation 
        {
            set
            {
                rotation = value % 2 * MathF.PI;
                Matrix2 Mat = Matrix2.CreateRotation(rotation);
                Matrix4 Tmat = Matrix;
                Tmat.M11 = scale.X * Mat.M11; Tmat.M12 = scale.X * Mat.M12;
                Tmat.M21 = scale.X * Mat.M21; Tmat.M22 = scale.X * Mat.M22;
                Matrix = Tmat;
            }
            get => rotation;
            
        }
        public Vector2 Scale
        {
            set
            {
                scale = value;
                Matrix2 Mat = Matrix2.CreateRotation(rotation);
                Matrix4 Tmat = Matrix;
                Tmat.M11 = scale.X * Mat.M11; Tmat.M12 = scale.X * Mat.M12;
                Tmat.M21 = scale.X * Mat.M21; Tmat.M22 = scale.X * Mat.M22;
                Matrix = Tmat;
            }
            get => scale;
        }
        public Vector2 Position
        {
            set
            {
                position = value;
                Matrix4 Tmat = Matrix;
                Tmat.M14 = Position.X;
                Tmat.M24 = Position.Y;
                Matrix = Tmat;
            }
            get => position;
        }

        public Transform2D() => Matrix = Matrix4.Identity;
    }
    public class Transform3D : ITransform
    {
        private Matrix3 rotmat = Matrix3.Identity;
        private Vector3 rotation = Vector3.Zero;
        private Vector3 scale = Vector3.One;
        private Vector3 position = Vector3.Zero;

        public Matrix4 Matrix { get; private set; }
        public Vector3 Rotation
        {
            set
            {

                
                rotation = new Vector3(value.X % 2 * MathF.PI, value.Y % 2 * MathF.PI, value.Z % 2 * MathF.PI);
                rotmat = Matrix3.CreateFromQuaternion(Quaternion.FromEulerAngles(rotation));
                Matrix4 Tmat = Matrix;
                Tmat.M11 = scale.X * rotmat.M11; Tmat.M21 = scale.X * rotmat.M21; Tmat.M31 = scale.X * rotmat.M31;
                Tmat.M12 = scale.Y * rotmat.M12; Tmat.M22 = scale.Y * rotmat.M22; Tmat.M32 = scale.Y * rotmat.M32;
                Tmat.M13 = scale.Z * rotmat.M13; Tmat.M23 = scale.Z * rotmat.M23; Tmat.M33 = scale.Z * rotmat.M33;
                Matrix = Tmat;

                //Set(rotation, scale, position);
            }
            get => rotation;

        }
        public Vector3 Scale
        {
            set
            {
                scale = value;
                Matrix4 Tmat = Matrix;
                Tmat.M11 = scale.X * rotmat.M11; Tmat.M21 = scale.X * rotmat.M21; Tmat.M31 = scale.X * rotmat.M31;
                Tmat.M12 = scale.Y * rotmat.M12; Tmat.M22 = scale.Y * rotmat.M22; Tmat.M32 = scale.Y * rotmat.M32;
                Tmat.M13 = scale.Z * rotmat.M13; Tmat.M23 = scale.Z * rotmat.M23; Tmat.M33 = scale.Z * rotmat.M33;
                Matrix = Tmat;
                //Set(rotation, scale, position); ;
            }
            get => scale;
        }
        public Vector3 Position
        {
            set
            {
                position = value;
                Matrix4 Tmat = Matrix;
                Tmat.M41 = Position.X;
                Tmat.M42 = Position.Y;
                Tmat.M43 = Position.Z;
                Matrix = Tmat;
                //Set(rotation, scale, position);
            }
            get => position;
        }

        public Transform3D() => Matrix = Matrix4.Identity;
        public Transform3D(Vector3 Rot, Vector3 Sca, Vector3 Pos) => this.Set(Rot, Sca, Pos);
        private void Set(Vector3 Rot, Vector3 Sca, Vector3 Pos)
        {
            Matrix4 Tr = Matrix4.CreateTranslation(Pos);
            Matrix4 Sc = Matrix4.CreateScale(Sca);
            Matrix4 Rt = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(Rot));
            Matrix = (Rt * Sc * Tr);
        }
    }
}
