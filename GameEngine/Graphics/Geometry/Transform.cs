using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace Graphics
{
    interface ITransform
    {
        public Matrix4 Matrix { get; }
        public Vector3 Rotation { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Scale { get; set; }
    }

    public class Transform : ITransform
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
            }
            get => position;
        }

        public Transform() => Matrix = Matrix4.Identity;
        public Transform(Vector3 Rot, Vector3 Sca, Vector3 Pos)
        {
            Matrix4 Tr = Matrix4.CreateTranslation(Pos);
            Matrix4 Sc = Matrix4.CreateScale(Sca);
            Matrix4 Rt = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(Rot));
            Matrix = (Rt * Sc * Tr);
        }
    }
    public class TransformInvert : ITransform
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
                rotation = new Vector3(value.X % (2 * MathF.PI), value.Y % (2 * MathF.PI), value.Z % (2 * MathF.PI));
                rotmat = Matrix3.CreateRotationX(rotation.X) * Matrix3.CreateRotationY(rotation.Y) * Matrix3.CreateRotationZ(rotation.Z);
                Set(Position, Scale, rotmat);
            }
            get => rotation;

        }
        public Vector3 Scale
        {
            set
            {
                scale = value;
                Set(Position, Scale, rotmat);

            }
            get => scale;
        }
        public Vector3 Position
        {
            set
            {
                position = value;
                Set(Position, Scale, rotmat);
            }
            get => position;
        }

        public TransformInvert() => Matrix = Matrix4.Identity;
        public TransformInvert(Vector3 Rot, Vector3 Sca, Vector3 Pos)
        {
            Matrix4 Tr = Matrix4.CreateTranslation(Pos);
            Matrix4 Sc = Matrix4.CreateScale(Sca);
            Matrix4 Rt = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(Rot));
            Matrix = (Rt * Sc * Tr).Inverted();
        }
        protected virtual void Set(Vector3 Position, Vector3 Scale, Matrix3 RotMat)
        {
            Matrix4 Tmat = Matrix4.Identity;
            Tmat.M11 = scale.X * rotmat.M11; Tmat.M21 = scale.X * rotmat.M21; Tmat.M31 = scale.X * rotmat.M31; Tmat.M41 = Position.X;
            Tmat.M12 = scale.Y * rotmat.M12; Tmat.M22 = scale.Y * rotmat.M22; Tmat.M32 = scale.Y * rotmat.M32; Tmat.M42 = Position.Y;
            Tmat.M13 = scale.Z * rotmat.M13; Tmat.M23 = scale.Z * rotmat.M23; Tmat.M33 = scale.Z * rotmat.M33; Tmat.M43 = Position.Z;
            Matrix = Tmat.Inverted();
        }
    }
}
