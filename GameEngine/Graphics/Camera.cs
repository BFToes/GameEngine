using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Graphics
{
    interface ICamera : ITransform
    {
        public float Zoom { get; set; }
        public Matrix4 ProjMat { get; }
        public float AspectRatio { get; set; }
    }

    class Camera : ICamera
    {
        public Matrix4 Matrix { get; private set; }
        public Matrix4 ProjMat { get; }
        public float Zoom 
        { 
            get => throw new NotImplementedException(); 
            set => throw new NotImplementedException(); 
        }
        public float AspectRatio 
        { 
            get => throw new NotImplementedException(); 
            set => throw new NotImplementedException(); 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FOV">the angle of visibility in degrees</param>
        /// <param name="ScreenSize"></param>
        /// <param name="DepthNear"> must be greater than 0</param>
        /// <param name="DepthFar"></param>
        public Camera(float FOV, Vector2 ScreenSize, float DepthNear, float DepthFar)
        {
            FOV = FOV / 180 * MathF.PI;
            ScreenSize.Normalize();
            float AspectRatio = ScreenSize.Y / ScreenSize.X;
            ProjMat = Matrix4.CreatePerspectiveFieldOfView(FOV, AspectRatio, DepthNear, DepthFar);
        }
        public Camera(Vector2 ScreenSize, float DepthNear, float DepthFar)
        {
            ProjMat = Matrix4.CreateOrthographic(ScreenSize.X, ScreenSize.Y, DepthNear, DepthFar);
        }
    }
}
