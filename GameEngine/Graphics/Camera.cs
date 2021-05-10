using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using Graphics.Shaders;

namespace Graphics
{
    class Camera : TransformInvert
    {
        public UniformBuffer<CameraData> UniformBlock;
        private float nearZ;
        private float farZ;
        public float fov { get; private set; }
        public Matrix4 ProjMat { get; private set; }

        /// <summary>
        /// initiates with perspective projection matrix 
        /// </summary>
        /// <param name="FOV">the angle of visibility in degrees must be greater than 0 and less than 180</param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="DepthNear">must be greater than 0</param>
        /// <param name="DepthFar">larger values will render more objects</param>
        public Camera(float FOV, float Width, float Height, float DepthNear, float DepthFar)
        {
            UniformBlock = new UniformBuffer<CameraData>();
            nearZ = DepthNear; farZ = DepthFar;
            fov = FOV / 180 * MathF.PI;
            ProjMat = Matrix4.CreatePerspectiveFieldOfView(fov, Width / Height, DepthNear, DepthFar);
        }
        /// <summary>
        /// initiates with orthographic projection matrix 
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="DepthNear">must be greater than or equal to 0</param>
        /// <param name="DepthFar">larger values will render more objects</param>
        public Camera(float Width, float Height, float DepthNear, float DepthFar)
        {
            UniformBlock = new UniformBuffer<CameraData>();
            nearZ = DepthNear; farZ = DepthFar;
            ProjMat = Matrix4.CreateOrthographic(Width, Height, nearZ, farZ);
        }
        public void Resize(Vector2i Size)
        {

            if (fov != 0)
            {
                Vector2 NormSize = ((Vector2)Size).Normalized();
                ProjMat = Matrix4.CreatePerspectiveFieldOfView(fov, NormSize.X / NormSize.Y, nearZ, farZ);
            }
            else
            {
                ProjMat = Matrix4.CreateOrthographic(Size.X, Size.Y, nearZ, farZ);
            }
            UniformBlock.Set(0, 64, ProjMat); // set data in uniform block
        }
        protected override void Set(Vector3 Position, Vector3 Scale, Matrix3 RotMat)
        {
            base.Set(Position, Scale, RotMat);
            UniformBlock.Set(64, 64, Matrix); // set data in uniform block
        }

    }
}
