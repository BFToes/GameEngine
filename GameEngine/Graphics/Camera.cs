using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Graphics
{
    interface ICamera
    {
        public float Zoom { get; set; }
        public Matrix4 ProjMat { get; }
        public void Resize(Vector2 ScreenSize);
    }

    class Camera : ICamera
    {
        private float nearZ;
        private float farZ;
        private float fov;
        public ITransform Transform;
        public Matrix4 ProjMat { get; private set; }
        public float Zoom 
        { 
            get => throw new NotImplementedException(); 
            set => throw new NotImplementedException(); 
        }
        /// <summary>
        /// initiates with perspective projection matrix 
        /// </summary>
        /// <param name="FOV">the angle of visibility in degrees must be greater than 0 and less than 180</param>
        /// <param name="ScreenSize"></param>
        /// <param name="DepthNear">must be greater than 0</param>
        /// <param name="DepthFar">larger values will render more objects</param>
        public Camera(float FOV, float Width, float Height, float DepthNear, float DepthFar)
        {
            nearZ = DepthNear; farZ = DepthFar;
            fov = FOV / 180 * MathF.PI;
            ProjMat = Matrix4.CreatePerspectiveFieldOfView(fov, Width / Height, DepthNear, DepthFar);
        }
        /// <summary>
        /// initiates with orthographic projection matrix 
        /// </summary>
        /// <param name="ScreenSize"></param>
        /// <param name="DepthNear">must be greater than or equal to 0</param>
        /// <param name="DepthFar">larger values will render more objects</param>
        public Camera(float Width, float Height, float DepthNear, float DepthFar)
        {
            nearZ = DepthNear; farZ = DepthFar;
            ProjMat = Matrix4.CreateOrthographic(Width, Height, nearZ, farZ);          

        }
        public void Resize(Vector2 ScreenSize)
        {

            if (fov != 0)
            {
                ScreenSize.Normalize();
                ProjMat = Matrix4.CreatePerspectiveFieldOfView(fov, ScreenSize.X / ScreenSize.Y, nearZ, farZ);
            }
            else
            {
                ProjMat = Matrix4.CreateOrthographic(ScreenSize.X, ScreenSize.Y, nearZ, farZ);
            }
        }
    }
}
