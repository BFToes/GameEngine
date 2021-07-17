using OpenTK.Mathematics;
using System;

namespace GameEngine.Entities.Culling
{
    interface ICullObserver<ObserverType> where ObserverType : IObserverShape
    {
        protected ObserverType Observer { get; }

        public bool Detects(ICullable<CullPoint> Entity);
        public bool Detects(ICullable<CullSphere> Entity);
        public bool Detects(ICullable<CullVolume> Entity);
    }

    interface IObserverShape
    {
        public bool Detects(CullPoint Point);
        public bool Detects(CullSphere Sphere);
        public bool Detects(CullVolume Volume);
    }
    sealed class FrustumObserver : IObserverShape
    {
        public Vector4[] Frustum = new Vector4[6];


        private void CalculatePlane(int Plane, Vector4 ColumnA, Vector4 ColumnB)
        {
            Frustum[Plane] = ColumnA + ColumnB;
            Frustum[Plane] /= new Vector3(Frustum[Plane]).Length;
        }

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
        public bool Detects(CullPoint Point)
        {
            for (int i = 0; i < 6; i++)
                if (Frustum[i].X * Point.pos.X + Frustum[i].Y * Point.pos.Y + Frustum[i].Z * Point.pos.Z + Frustum[i].W <= 0.0f)
                    return false;
            return true;
        }
        public bool Detects(CullSphere Sphere)
        {
            for (int p = 0; p < 6; p++)
                if (Frustum[p].X * Sphere.pos.X + Frustum[p].Y * Sphere.pos.Y + Frustum[p].Z * Sphere.pos.Z + Frustum[p].W < -Sphere.rad)
                    return false;
            return true;
        }
        public bool Detects(CullVolume Volume)
        {
            for (int i = 0; i < 6; i++)
            {
                if (Frustum[i].X * Volume.minPos.X + Frustum[i].Y * Volume.minPos.Y + Frustum[i].Z * Volume.minPos.Z + Frustum[i].W > 0) continue;
                if (Frustum[i].X * Volume.maxPos.X + Frustum[i].Y * Volume.minPos.Y + Frustum[i].Z * Volume.minPos.Z + Frustum[i].W > 0) continue;
                if (Frustum[i].X * Volume.minPos.X + Frustum[i].Y * Volume.maxPos.Y + Frustum[i].Z * Volume.minPos.Z + Frustum[i].W > 0) continue;
                if (Frustum[i].X * Volume.maxPos.X + Frustum[i].Y * Volume.maxPos.Y + Frustum[i].Z * Volume.minPos.Z + Frustum[i].W > 0) continue;
                if (Frustum[i].X * Volume.minPos.X + Frustum[i].Y * Volume.minPos.Y + Frustum[i].Z * Volume.maxPos.Z + Frustum[i].W > 0) continue;
                if (Frustum[i].X * Volume.maxPos.X + Frustum[i].Y * Volume.minPos.Y + Frustum[i].Z * Volume.maxPos.Z + Frustum[i].W > 0) continue;
                if (Frustum[i].X * Volume.minPos.X + Frustum[i].Y * Volume.maxPos.Y + Frustum[i].Z * Volume.maxPos.Z + Frustum[i].W > 0) continue;
                if (Frustum[i].X * Volume.maxPos.X + Frustum[i].Y * Volume.maxPos.Y + Frustum[i].Z * Volume.maxPos.Z + Frustum[i].W > 0) continue;
                return false;
            }
            return true;
        }
    }
    sealed class SphereObserver : IObserverShape
    {
        public Vector3 CullPosition;
        public float CullRadius;

        public void Update(Matrix4 Matrix)
        {
            CullPosition = new Vector3(Matrix.Row3);
            Vector3 Scale = Matrix.ExtractScale();
            CullRadius = MathF.Max(MathF.Max(Scale.X, Scale.Y), Scale.Z) * 1.4f;
        }

        public bool Detects(CullPoint Point) => (Point.pos - CullPosition).LengthSquared < CullRadius * CullRadius;
        public bool Detects(CullSphere Sphere) => (Sphere.pos - CullPosition).LengthSquared < MathF.Pow(Sphere.rad + CullRadius, 2);
        public bool Detects(CullVolume Volume)
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
}