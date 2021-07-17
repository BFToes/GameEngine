using OpenTK.Mathematics;
using System;

namespace GameEngine.Geometry.Transform
{
    public interface ITransform
    {
        public Matrix4 Matrix { get; }
        public event Action<Matrix4> Set_Transform;
        public void Extract(Matrix4 Matrix);
    }
    public abstract class AbstractTransform3D : ITransform
    {
        protected Matrix3 RotMatrix = Matrix3.Identity;
        private Vector3 rotation = Vector3.Zero;
        private Vector3 scale = Vector3.One;
        private Vector3 position = Vector3.Zero;

        public event Action<Vector3> Set_Rotation = delegate{ };
        public event Action<Vector3> Set_Position = delegate{ };
        public event Action<Vector3> Set_Scale = delegate{ };
        public event Action<Matrix4> Set_Transform = delegate{ };

        public Matrix4 Matrix { get; protected set; }
        public virtual Vector3 Rotation
        {
            set
            {
                rotation = value;
                Set_Rotation(rotation);
                Set_Transform(Matrix);
            }
            get => rotation;

        }
        public virtual Vector3 Scale
        {
            set
            {
                scale = value;
                Set_Scale(scale);
                Set_Transform(Matrix);
            }
            get => scale;
        }
        public virtual Vector3 Position
        {
            set
            {
                position = value;

                Set_Position(position);
                Set_Transform(Matrix);
            }
            get => position;
        }

        public virtual void Extract(Matrix4 Matrix)
        {
            scale = Matrix.ExtractScale();
            position = Matrix.ExtractTranslation();
            Quaternion Q = Matrix.ExtractRotation();
            rotation = Q.ToEulerAngles();
            RotMatrix = Matrix3.CreateFromQuaternion(Q);
        }
    }
    public class Transform3D : AbstractTransform3D
    {
        public override Vector3 Rotation
        {
            set
            {
                value = new Vector3(value.X % 2 * MathF.PI, value.Y % 2 * MathF.PI, value.Z % 2 * MathF.PI);
                RotMatrix = Matrix3.CreateFromQuaternion(Quaternion.FromEulerAngles(value));
                Matrix = CalcMatrix(Position, Scale, RotMatrix);
                base.Rotation = value;
            }
            get => base.Rotation;

        }
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

        public Transform3D() => Matrix = Matrix4.Identity;
        public Transform3D(Vector3 Rot, Vector3 Sca, Vector3 Pos)
        {
            Matrix4 Tr = Matrix4.CreateTranslation(Pos);
            Matrix4 Sc = Matrix4.CreateScale(Sca);
            Matrix4 Rt = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(Rot));
            Matrix = (Rt * Sc * Tr);
        }
        private static Matrix4 CalcMatrix(Vector3 Position, Vector3 Scale, Matrix3 RotMat)
        {
            Matrix4 Tmat = Matrix4.Identity;
            Tmat.M11 = Scale.X * RotMat.M11; Tmat.M21 = Scale.X * RotMat.M21; Tmat.M31 = Scale.X * RotMat.M31; Tmat.M41 = Position.X;
            Tmat.M12 = Scale.Y * RotMat.M12; Tmat.M22 = Scale.Y * RotMat.M22; Tmat.M32 = Scale.Y * RotMat.M32; Tmat.M42 = Position.Y;
            Tmat.M13 = Scale.Z * RotMat.M13; Tmat.M23 = Scale.Z * RotMat.M23; Tmat.M33 = Scale.Z * RotMat.M33; Tmat.M43 = Position.Z;
            return Tmat;
        }
    }
    public class InverseTransform3D : AbstractTransform3D
    {
        public override Vector3 Rotation
        {
            set
            {
                Vector3 r = new Vector3(value.X % (2 * MathF.PI), value.Y % (2 * MathF.PI), value.Z % (2 * MathF.PI));
                RotMatrix = Matrix3.CreateRotationX(r.X) * Matrix3.CreateRotationY(r.Y) * Matrix3.CreateRotationZ(r.Z);
                Matrix = CalcMatrix(Position, Scale, RotMatrix);
                base.Rotation = r;
            }
            get => base.Rotation;

        }
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
                Matrix = CalcMatrix(value, Scale, RotMatrix);
                base.Position = value;
            }
            get => base.Position;
        }

        public InverseTransform3D() => Matrix = Matrix4.Identity;
        public InverseTransform3D(Vector3 Rot, Vector3 Sca, Vector3 Pos)
        {
            Matrix4 Tr = Matrix4.CreateTranslation(Pos);
            Matrix4 Sc = Matrix4.CreateScale(Sca);
            Matrix4 Rt = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(Rot));
            Matrix = (Rt * Sc * Tr).Inverted();
        }
        private static Matrix4 CalcMatrix(Vector3 Position, Vector3 Scale, Matrix3 RotMat)
        {
            Matrix4 Tmat = Matrix4.Identity;
            Tmat.M11 = Scale.X * RotMat.M11; Tmat.M21 = Scale.X * RotMat.M21; Tmat.M31 = Scale.X * RotMat.M31; Tmat.M41 = Position.X;
            Tmat.M12 = Scale.Y * RotMat.M12; Tmat.M22 = Scale.Y * RotMat.M22; Tmat.M32 = Scale.Y * RotMat.M32; Tmat.M42 = Position.Y;
            Tmat.M13 = Scale.Z * RotMat.M13; Tmat.M23 = Scale.Z * RotMat.M23; Tmat.M33 = Scale.Z * RotMat.M33; Tmat.M43 = Position.Z;
            return Tmat.Inverted();
        }
        public override void Extract(Matrix4 Matrix)
        {
            throw new NotImplementedException();
        }
    }
    public class TransformAligned3D : AbstractTransform3D
    {
        public override Vector3 Scale
        {
            set
            {
                Matrix = CalcMatrix(Position, value);
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
        private static Matrix4 CalcMatrix(Vector3 Position, Vector3 Scale)
        {
            Matrix4 Tmat = Matrix4.Identity;
            Tmat.M11 = Scale.X; Tmat.M21 = 0; Tmat.M31 = 0; Tmat.M41 = Position.X;
            Tmat.M12 = 0; Tmat.M22 = Scale.Y; Tmat.M32 = 0; Tmat.M42 = Position.Y;
            Tmat.M13 = 0; Tmat.M23 = 0; Tmat.M33 = Scale.Z; Tmat.M43 = Position.Z;
            return Tmat;
        }
    }
}
