using Graphics.Entities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Graphics.Rendering
{
    interface CullShape
    {
        public bool InView(Observer Observer);
    }

    interface Observer
    {
        /// <summary>
        /// point is observed by observer
        /// </summary>
        public bool IntersectPoint(Vector3 Position);
        /// <summary>
        /// volume, defined by an axis aligned cube scaled around Position, is observed by observer
        /// </summary>
        public bool IntersectVolume(Vector3 Position, Vector3 Scale);
        /// <summary>
        /// sphere is is observed by observer
        /// </summary>
        public bool IntersectSphere(Vector3 Position, float Scale);
    }

    abstract class FrustumObserver : SpatialEntity<AbstractTransform3D>, Observer
    {
        private float[,] _frustum = new float[6, 4];
        private enum ClippingPlane { Left, Right, Bottom, Top, Front, Back }

        protected Matrix4 Projection;

        public FrustumObserver(AbstractTransform3D Transform) : base(Transform)
        {
            Transform.Set_Transform += CalculateFrustum;
        }

        private void CalculateFrustum(Matrix4 View)
        {
            Matrix4 M = View * Projection;

            CalculatePlane((int)ClippingPlane.Left, M.Column3, M.Column0);
            CalculatePlane((int)ClippingPlane.Right, M.Column3, -M.Column0);
            CalculatePlane((int)ClippingPlane.Bottom, M.Column3, M.Column1);
            CalculatePlane((int)ClippingPlane.Top, M.Column3, -M.Column1);
            CalculatePlane((int)ClippingPlane.Front, M.Column3, M.Column2);
            CalculatePlane((int)ClippingPlane.Back, M.Column3, -M.Column2);
        }
        private void CalculatePlane(int Plane, Vector4 ColumnA, Vector4 ColumnB)
        {
            _frustum[Plane, 0] = ColumnA.X + ColumnB.X;
            _frustum[Plane, 1] = ColumnA.Y + ColumnB.Y;
            _frustum[Plane, 2] = ColumnA.Z + ColumnB.Z;
            _frustum[Plane, 3] = ColumnA.W + ColumnB.W;

            float magnitude = MathF.Sqrt((_frustum[Plane, 0] * _frustum[Plane, 0]) + (_frustum[Plane, 1] * _frustum[Plane, 1]) + (_frustum[Plane, 2] * _frustum[Plane, 2]));
            _frustum[Plane, 0] /= magnitude;
            _frustum[Plane, 1] /= magnitude;
            _frustum[Plane, 2] /= magnitude;
            _frustum[Plane, 3] /= magnitude;
        }

        #region Observer Intersect Function
        public bool IntersectPoint(Vector3 Position)
        {
            for (int i = 0; i < 6; i++)
                if (_frustum[i, 0] * Position.X + _frustum[i, 1] * Position.Y + _frustum[i, 2] * Position.Z + _frustum[i, 3] <= 0.0f)
                    return false;
            return true;
        }
        public bool IntersectSphere(Vector3 Position, float Scale)
        {
            for (int p = 0; p < 6; p++)
                if (_frustum[p, 0] * Position.X + _frustum[p, 1] * Position.Y + _frustum[p, 2] * Position.Z + _frustum[p, 3] < -Scale) 
                    return false;
            return true;
        }
        public bool IntersectVolume(Vector3 Position, Vector3 Scale)
        {
            for (int i = 0; i < 6; i++)
            {
                if (_frustum[i, 0] * (Position.X - Scale.X) + _frustum[i, 1] * (Position.Y - Scale.Y) + _frustum[i, 2] * (Position.Z - Scale.Z) + _frustum[i, 3] > 0) continue;
                if (_frustum[i, 0] * (Position.X + Scale.X) + _frustum[i, 1] * (Position.Y - Scale.Y) + _frustum[i, 2] * (Position.Z - Scale.Z) + _frustum[i, 3] > 0) continue;
                if (_frustum[i, 0] * (Position.X - Scale.X) + _frustum[i, 1] * (Position.Y + Scale.Y) + _frustum[i, 2] * (Position.Z - Scale.Z) + _frustum[i, 3] > 0) continue;
                if (_frustum[i, 0] * (Position.X + Scale.X) + _frustum[i, 1] * (Position.Y + Scale.Y) + _frustum[i, 2] * (Position.Z - Scale.Z) + _frustum[i, 3] > 0) continue;
                if (_frustum[i, 0] * (Position.X - Scale.X) + _frustum[i, 1] * (Position.Y - Scale.Y) + _frustum[i, 2] * (Position.Z + Scale.Z) + _frustum[i, 3] > 0) continue;
                if (_frustum[i, 0] * (Position.X + Scale.X) + _frustum[i, 1] * (Position.Y - Scale.Y) + _frustum[i, 2] * (Position.Z + Scale.Z) + _frustum[i, 3] > 0) continue;
                if (_frustum[i, 0] * (Position.X - Scale.X) + _frustum[i, 1] * (Position.Y + Scale.Y) + _frustum[i, 2] * (Position.Z + Scale.Z) + _frustum[i, 3] > 0) continue;
                if (_frustum[i, 0] * (Position.X + Scale.X) + _frustum[i, 1] * (Position.Y + Scale.Y) + _frustum[i, 2] * (Position.Z + Scale.Z) + _frustum[i, 3] > 0) continue;
                return false;
            }
            return true;
        }
        #endregion
    }

    abstract class SphereObserver : SpatialEntity<AbstractTransform3D>, Observer
    {
        protected abstract float ObserverRadius { get; }

        public SphereObserver(AbstractTransform3D Transform) : base(Transform) { }
        #region Observer Intersect Functions
        public bool IntersectPoint(Vector3 Position) => (Position - WorldPosition).LengthSquared < ObserverRadius * ObserverRadius;
        public bool IntersectSphere(Vector3 Position, float Radius) => (Position - WorldPosition).LengthSquared < MathF.Pow(Radius + ObserverRadius, 2);
        public bool IntersectVolume(Vector3 Position, Vector3 Scale)
        {
            Vector3 MinPos = Position - Scale;
            Vector3 MaxPos = Position + Scale;
            float distSQ = ObserverRadius * ObserverRadius;
            
            if (WorldPosition.X < MinPos.X) 
                distSQ -= MathF.Pow(WorldPosition.X - MinPos.X, 2);
            else if (WorldPosition.X > MaxPos.X) 
                distSQ -= MathF.Pow(WorldPosition.X - MaxPos.X, 2);
            
            if (WorldPosition.Y < MinPos.Y) 
                distSQ -= MathF.Pow(WorldPosition.Y - MinPos.Y, 2);
            else if (WorldPosition.Y > MaxPos.Y) 
                distSQ -= MathF.Pow(WorldPosition.Y - MaxPos.Y, 2);
            
            if (WorldPosition.Z < MinPos.Z) 
                distSQ -= MathF.Pow(WorldPosition.Z - MinPos.Z, 2);
            else if (WorldPosition.Z > MaxPos.Z) 
                distSQ -= MathF.Pow(WorldPosition.Z - MaxPos.Z, 2);

            return distSQ > 0;
        }
        #endregion
    }
}
