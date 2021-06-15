using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace Graphics
{
    public abstract class AbstractTransform2D : ITransform
    {
        protected Matrix2 RotMatrix = Matrix2.Identity;
        private float rotation = 0;
        private Vector2 scale = Vector2.One;
        private Vector2 position = Vector2.Zero;
        private int z_index;

        public event Action<float> Set_Rotation = delegate { };
        public event Action<Vector2> Set_Position = delegate { };
        public event Action<Vector2> Set_Scale = delegate { };
        public event Action<int> Set_Z_Index = delegate { };
        public event Action<Matrix4> Set_Transform = delegate { };

        public Matrix4 Matrix { get; protected set; }
        public virtual float Rotation
        {
            set
            {
                rotation = value;
                Set_Rotation(rotation);
                Set_Transform(Matrix);
            }
            get => rotation;

        }
        public virtual Vector2 Scale
        {
            set
            {
                scale = value;
                Set_Scale(scale);
                Set_Transform(Matrix);
            }
            get => scale;
        }
        public virtual Vector2 Position
        {
            set
            {
                position = value;

                Set_Position(position);
                Set_Transform(Matrix);
            }
            get => position;
        }
        public virtual int Z_Index
        {
            get => z_index;
            set
            {
                z_index = value;
                Set_Z_Index(z_index);
                Set_Transform(Matrix);
            }
        }
    }
    public class Transform2D : AbstractTransform2D
    {
        public override float Rotation
        {
            set
            {
                float R = value % (2 * MathF.PI);
                RotMatrix = Matrix2.CreateRotation(R);
                throw new NotImplementedException("2D Matrix Calculation");
                base.Rotation = value;
            }
            get => base.Rotation;

        }
        public override Vector2 Scale
        {
            set
            {
                throw new NotImplementedException("2D Matrix Calculation");
                base.Scale = value;
            }
            get => base.Scale;
        }
        public override Vector2 Position
        {
            set
            {
                throw new NotImplementedException("2D Matrix Calculation");
                base.Position = value;
            }
            get => base.Position;
        }
        public override int Z_Index
        {
            set
            {
                throw new NotImplementedException("2D Matrix Calculation");
                base.Z_Index = value;
            }
            get => base.Z_Index;
        }


        public Transform2D() => Matrix = Matrix4.Identity;
        public Transform2D(Vector3 Rot, Vector3 Sca, Vector3 Pos)
        {
            Matrix4 Tr = Matrix4.CreateTranslation(Pos);
            Matrix4 Sc = Matrix4.CreateScale(Sca);
            Matrix4 Rt = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(Rot));
            Matrix = (Rt * Sc * Tr);
        }
        private static Matrix4 CalcMatrix(Vector2 Position, Vector2 Scale, Matrix2 RotMat, int Z_Index)
        {
            throw new NotImplementedException("2D Matrix Calculation");
        }
    }
    public class InverseTransform2D : AbstractTransform2D
    {
        public override float Rotation
        {
            set
            {
                float r = value % (2 * MathF.PI);
                //RotMatrix = Matrix3.CreateRotationX(r.X) * Matrix3.CreateRotationY(r.Y) * Matrix3.CreateRotationZ(r.Z);
                Matrix = CalcMatrix(Position, Scale, RotMatrix, Z_Index);
                base.Rotation = r;
            }
            get => base.Rotation;

        }
        public override Vector2 Scale
        {
            set
            {
                Matrix = CalcMatrix(Position, value, RotMatrix, Z_Index);
                base.Scale = value;
            }
            get => base.Scale;
        }
        public override Vector2 Position
        {
            set
            {
                Matrix = CalcMatrix(value, Scale, RotMatrix, Z_Index);
                base.Position = value;
            }
            get => base.Position;
        }
        public override int Z_Index 
        {
            set 
            {
                throw new NotImplementedException("2D Matrix Calculation");
                base.Z_Index = value; 
            }

            get => base.Z_Index;
        }

        public InverseTransform2D() => Matrix = Matrix4.Identity;
        public InverseTransform2D(Vector3 Rot, Vector3 Sca, Vector3 Pos)
        {
            Matrix4 Tr = Matrix4.CreateTranslation(Pos);
            Matrix4 Sc = Matrix4.CreateScale(Sca);
            Matrix4 Rt = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(Rot));
            Matrix = (Rt * Sc * Tr).Inverted();
        }
        private static Matrix4 CalcMatrix(Vector2 Position, Vector2 Scale, Matrix2 RotMat, int Z_Index)
        {
            Matrix4 Tmat = Matrix4.Identity;
            throw new NotImplementedException("2D Matrix Calculation");
            return Tmat.Inverted();
        }
    }
}
