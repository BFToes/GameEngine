using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Graphics.Rendering.Culling
{
    public interface ICullable<CullType> where CullType : ICullShape
    {
        public CullType CullShape { get; }
    }
    public interface ICullShape  { }
    struct CullPoint : ICullShape
    {
        public Vector3 pos;

        public void Extract(Matrix4 Matrix)
        {
            pos = new Vector3(Matrix.Row3);
        }
    }
    struct CullVolume : ICullShape
    {
        public Vector3 minPos;
        public Vector3 maxPos;

        public void Extract(Matrix4 Matrix)
        {
            Vector3 Position = new Vector3(Matrix.Row3);
            Vector3 Scale = Matrix.ExtractScale();
            minPos = Position - Scale;
            maxPos = Position + Scale;
        }
    }
    struct CullSphere : ICullShape
    {
        public Vector3 pos;
        public float rad;

        public void Extract(Matrix4 Matrix)
        {
            CullSphere Shape = new CullSphere();
            pos = new Vector3(Matrix.Row3);
            Vector3 Scale = Matrix.ExtractScale();
            rad = MathF.Max(MathF.Max(Scale.X, Scale.Y), Scale.Z);
        }
    }
}
