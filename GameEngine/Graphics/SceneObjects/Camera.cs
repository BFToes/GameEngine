using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using Graphics.Shaders;

namespace Graphics.SceneObjects
{
    class Camera : SpatialEntity<AbstractTransform3D>
    {
        public UniformBlock Block;
        public float fov { get; private set; }
        public Matrix4 ProjMat { get; private set; }
        private float nearZ;
        private float farZ;

        /// <summary>
        /// initiates with perspective projection matrix 
        /// </summary>
        /// <param name="FOV">the angle of visibility in degrees must be greater than 0 and less than 180</param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="DepthNear">must be greater than 0</param>
        /// <param name="DepthFar">larger values will render more objects</param>
        public Camera(float FOV, float Width, float Height, float DepthNear, float DepthFar) : base(new InverseTransform3D())
        {
            nearZ = DepthNear; 
            farZ = DepthFar;
            fov = FOV / 180 * MathF.PI;
            
            if (fov != 0) 
                ProjMat = Matrix4.CreatePerspectiveFieldOfView(fov, Width / Height, nearZ, farZ);
            else 
                ProjMat = Matrix4.CreateOrthographic((int)Width, (int)Height, nearZ, farZ);
            
            Transform.Set_Transform += SetBlock;
            
            Block = UniformBlock.For<CameraData>(0);
            Block.Set(new CameraData(ProjMat, Transform.Matrix, new Vector2(Width, Height))); // set data in uniform block
            CameraData C = Block.Get<CameraData>();
        }
        public void Resize(Vector2i Size)
        {
            if (fov != 0)
                ProjMat = Matrix4.CreatePerspectiveFieldOfView(fov, (float)Size.X / (float)Size.Y, nearZ, farZ);
            else
                ProjMat = Matrix4.CreateOrthographic(Size.X, Size.Y, nearZ, farZ);
            Block.Set(0, ProjMat);
            Block.Set(144, (Vector2)Size); // set data in uniform block
        }
        private void SetBlock(Matrix4 Matrix)
        {
            Block.Set(64, Matrix); // set camera matrix in uniform block
            Block.Set(128, Transform.Position); // set position in uniform block
        }
    }
}
