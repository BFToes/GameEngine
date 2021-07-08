using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace Graphics
{
    public class TransformAligned3D : AbstractTransform3D
    {
        public override Vector3 Scale
        {
            set
            {
                Matrix = CalcMatrix(Position, value, RotMatrix);
                base.Scale = value;
            }
            get => base.Scale;
        }
        public override Vector3 Position
        {
            set
            {
                Matrix4 Tmat = Matrix;
                Tmat.M41 = value.X; Tmat.M42 = value.Y; Tmat.M43 = value.Z; Matrix = Tmat;

                base.Position = value;
            }
            get => base.Position;
        }
        public override Vector3 Rotation
        { 
            get => throw new Exception("Axis Aligned means no rotation dumbass");
            set => throw new Exception("Axis Aligned means no rotation dumbass");
        } // pretend this isnt here

        public TransformAligned3D() => Matrix = Matrix4.Identity;
        private static Matrix4 CalcMatrix(Vector3 Position, Vector3 Scale, Matrix3 RotMat)
        {
            Matrix4 Tmat = Matrix4.Identity;
            Tmat.M11 = Scale.X; Tmat.M21 = 0;       Tmat.M31 = 0;       Tmat.M41 = Position.X;
            Tmat.M12 = 0;       Tmat.M22 = Scale.Y; Tmat.M32 = 0;       Tmat.M42 = Position.Y;
            Tmat.M13 = 0;       Tmat.M23 = 0;       Tmat.M33 = Scale.Z; Tmat.M43 = Position.Z;
            return Tmat;
        }
    }
}
