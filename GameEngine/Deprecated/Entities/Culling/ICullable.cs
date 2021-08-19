using OpenTK.Mathematics;
using System;

namespace GameEngine.Entities.Culling
{
    [Obsolete]
    /// <summary>
    /// Can check if Culling object is inside itself
    /// </summary>
    /// <typeparam name="CullType"></typeparam>
    interface ICullObserver<CullType> where CullType : IObserverShape
    {
        protected CullType Observer { get; }

        public bool Detects(ICullable<Sphere> Entity);
        public bool Detects(ICullable<Box> Entity);
    }
    [Obsolete]
    /// <summary>
    /// can be found within cull observers and is an observer itself
    /// </summary>
    /// <typeparam name="CullType"></typeparam>
    interface ICullable<CullType> : ICullObserver<CullType> where CullType : ICullShape
    {
        public CullType CullShape { get; }
    }
    [Obsolete]
    interface IObserverShape 
    {
        public bool Intersect(Sphere Sphere);
        public bool Intersect(Box Volume);
    }
    [Obsolete]
    interface ICullShape : IObserverShape { }

    [Obsolete]
    /// <summary>
    /// An axis aligned bounding box 
    /// </summary>
    class Box : ICullShape, IObserverShape
    {
        public Vector3 minPos = -Vector3.One;
        public Vector3 maxPos = Vector3.One;

        public Box(Vector3 minPos, Vector3 maxPos)
        {
            this.minPos = minPos;
            this.maxPos = maxPos;
        }

        public bool Intersect(Sphere Sphere)
        {
            float distSQ = Sphere.CullRadius * Sphere.CullRadius;

            if (Sphere.CullPosition.X < minPos.X) distSQ -= MathF.Pow(Sphere.CullPosition.X - minPos.X, 2);
            else if (Sphere.CullPosition.X > maxPos.X) distSQ -= MathF.Pow(Sphere.CullPosition.X - maxPos.X, 2);

            if (Sphere.CullPosition.Y < minPos.Y) distSQ -= MathF.Pow(Sphere.CullPosition.Y - minPos.Y, 2);
            else if (Sphere.CullPosition.Y > maxPos.Y) distSQ -= MathF.Pow(Sphere.CullPosition.Y - maxPos.Y, 2);

            if (Sphere.CullPosition.Z < minPos.Z) distSQ -= MathF.Pow(Sphere.CullPosition.Z - minPos.Z, 2);
            else if (Sphere.CullPosition.Z > maxPos.Z) distSQ -= MathF.Pow(Sphere.CullPosition.Z - maxPos.Z, 2);

            return distSQ > 0;
        }
        public bool Intersect(Box Volume)
        {
            if (maxPos.X < Volume.minPos.X) return false;
            if (maxPos.Y < Volume.minPos.Y) return false;
            if (maxPos.Z < Volume.minPos.Z) return false;
            if (minPos.X > Volume.maxPos.X) return false;
            if (minPos.Y > Volume.maxPos.Y) return false;
            if (minPos.Z > Volume.maxPos.Z) return false;
            return true;
        }

        public void Extract(Matrix4 Matrix)
        {
            Vector3 Position = new Vector3(Matrix.Row3);
            Vector3 Scale = Matrix.ExtractScale();
            minPos = Position - Scale;
            maxPos = Position + Scale;
        }

        public static Box operator |(Box Box1, Box Box2) 
        {
            Vector3 Max = new Vector3(
                MathF.Max(Box1.maxPos.X, Box2.maxPos.X), 
                MathF.Max(Box1.maxPos.Y, Box2.maxPos.Y), 
                MathF.Max(Box1.maxPos.Z, Box2.maxPos.Z));
            Vector3 Min = new Vector3(
                MathF.Min(Box1.minPos.X, Box2.minPos.X),
                MathF.Min(Box1.minPos.Y, Box2.minPos.Y),
                MathF.Min(Box1.minPos.Z, Box2.minPos.Z));

            return new Box(Min, Max);
        }
        public static Box operator &(Box Box1, Box Box2)
        {
            Vector3 Max = new Vector3(
                MathF.Min(Box1.maxPos.X, Box2.maxPos.X),
                MathF.Min(Box1.maxPos.Y, Box2.maxPos.Y),
                MathF.Min(Box1.maxPos.Z, Box2.maxPos.Z));
            Vector3 Min = new Vector3(
                MathF.Max(Box1.minPos.X, Box2.minPos.X),
                MathF.Max(Box1.minPos.Y, Box2.minPos.Y),
                MathF.Max(Box1.minPos.Z, Box2.minPos.Z));

            return new Box(Min, Max);
        }
    }
    [Obsolete]
    /// <summary>
    /// A Sphere
    /// </summary>
    class Sphere : ICullShape, IObserverShape
    {
        public Vector3 CullPosition = Vector3.Zero;
        public float CullRadius = 1;

        public Sphere() { }
        public Sphere(Vector3 Position, float Radius)
        {
            CullPosition = Position;
            CullRadius = Radius;
        }

        public void Update(Matrix4 Matrix)
        {
            CullPosition = new Vector3(Matrix.Row3);
            Vector3 Scale = Matrix.ExtractScale();
            CullRadius = MathF.Max(MathF.Max(Scale.X, Scale.Y), Scale.Z) * 1.4f;
        }
        public bool Intersect(Sphere Sphere) => (Sphere.CullPosition - CullPosition).LengthSquared < MathF.Pow(Sphere.CullRadius + CullRadius, 2);
        public bool Intersect(Box Volume)
        {
            float distSQ = CullRadius * CullRadius;

            if (CullPosition.X < Volume.minPos.X) distSQ -= MathF.Pow(CullPosition.X - Volume.minPos.X, 2);
            else if (CullPosition.X > Volume.maxPos.X) distSQ -= MathF.Pow(CullPosition.X - Volume.maxPos.X, 2);

            if (CullPosition.Y < Volume.minPos.Y) distSQ -= MathF.Pow(CullPosition.Y - Volume.minPos.Y, 2);
            else if (CullPosition.Y > Volume.maxPos.Y) distSQ -= MathF.Pow(CullPosition.Y - Volume.maxPos.Y, 2);

            if (CullPosition.Z < Volume.minPos.Z) distSQ -= MathF.Pow(CullPosition.Z - Volume.minPos.Z, 2);
            else if (CullPosition.Z > Volume.maxPos.Z) distSQ -= MathF.Pow(CullPosition.Z - Volume.maxPos.Z, 2);

            return distSQ > 0;
        }
    }
    [Obsolete]
    /// <summary>
    /// A view frustum built from a projection matrix
    /// </summary>
    class Frustum : IObserverShape
    {
        private Vector4[] frustum = new Vector4[6];

        public void Update(Matrix4 View, Matrix4 Proj)
        {
            Matrix4 ViewProj = View * Proj;
            CalculatePlane(0, ViewProj.Column3, ViewProj.Column0);
            CalculatePlane(1, ViewProj.Column3, -ViewProj.Column0);
            CalculatePlane(2, ViewProj.Column3, ViewProj.Column1);
            CalculatePlane(3, ViewProj.Column3, -ViewProj.Column1);
            CalculatePlane(4, ViewProj.Column3, ViewProj.Column2);
            CalculatePlane(5, ViewProj.Column3, -ViewProj.Column2);
        }
        private void CalculatePlane(int Plane, Vector4 ColumnA, Vector4 ColumnB)
        {
            frustum[Plane] = ColumnA + ColumnB;
            frustum[Plane] /= new Vector3(frustum[Plane]).Length;
        }

        public bool Intersect(Sphere Sphere)
        {
            for (int p = 0; p < 6; p++)
                if (frustum[p].X * Sphere.CullPosition.X + frustum[p].Y * Sphere.CullPosition.Y + frustum[p].Z * Sphere.CullPosition.Z + frustum[p].W < -Sphere.CullRadius)
                    return false;
            return true;
        }
        public bool Intersect(Box Volume)
        {
            for (int i = 0; i < 6; i++)
            {
                if (frustum[i].X * Volume.minPos.X + frustum[i].Y * Volume.minPos.Y + frustum[i].Z * Volume.minPos.Z + frustum[i].W > 0) continue;
                if (frustum[i].X * Volume.maxPos.X + frustum[i].Y * Volume.minPos.Y + frustum[i].Z * Volume.minPos.Z + frustum[i].W > 0) continue;
                if (frustum[i].X * Volume.minPos.X + frustum[i].Y * Volume.maxPos.Y + frustum[i].Z * Volume.minPos.Z + frustum[i].W > 0) continue;
                if (frustum[i].X * Volume.maxPos.X + frustum[i].Y * Volume.maxPos.Y + frustum[i].Z * Volume.minPos.Z + frustum[i].W > 0) continue;
                if (frustum[i].X * Volume.minPos.X + frustum[i].Y * Volume.minPos.Y + frustum[i].Z * Volume.maxPos.Z + frustum[i].W > 0) continue;
                if (frustum[i].X * Volume.maxPos.X + frustum[i].Y * Volume.minPos.Y + frustum[i].Z * Volume.maxPos.Z + frustum[i].W > 0) continue;
                if (frustum[i].X * Volume.minPos.X + frustum[i].Y * Volume.maxPos.Y + frustum[i].Z * Volume.maxPos.Z + frustum[i].W > 0) continue;
                if (frustum[i].X * Volume.maxPos.X + frustum[i].Y * Volume.maxPos.Y + frustum[i].Z * Volume.maxPos.Z + frustum[i].W > 0) continue;
                return false;
            }
            return true;
        }
    }

}
