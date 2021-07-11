using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using Graphics.Shaders;
using Graphics.Rendering;

namespace Graphics.Entities
{
    class Camera : FrustumObserver
    {
        public UniformBlock Block = UniformBlock.For<CameraData>(0);
        private float FOV, nearZ, farZ;
        
        /// <summary>
        /// initiates with perspective projection matrix 
        /// </summary>
        /// <param name="FOV">the angle of visibility in degrees must be greater than 0 and less than 180</param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="DepthNear">must be greater than 0</param>
        /// <param name="DepthFar">larger values will render more objects</param>
        public Camera(float FOV, int Width, int Height, float DepthNear, float DepthFar): base(new InverseTransform3D())
        {
            nearZ = DepthNear; 
            farZ = DepthFar;
            this.FOV = FOV / 180 * MathF.PI;

            if (this.FOV != 0)
                Projection = Matrix4.CreatePerspectiveFieldOfView(this.FOV, Width / Height, nearZ, farZ);
            else
                Projection = Matrix4.CreateOrthographic((int)Width, (int)Height, nearZ, farZ);

            Transform.Set_Transform += SetBlock;
            
            Block.Set(new CameraData(Projection, Transform.Matrix, new Vector2(Width, Height))); // set data in uniform block
        }

        public void Resize(Vector2i Size)
        {
            if (FOV != 0)
                Projection = Matrix4.CreatePerspectiveFieldOfView(FOV, (float)Size.X / Size.Y, nearZ, farZ);
            else
                Projection = Matrix4.CreateOrthographic(Size.X, Size.Y, nearZ, farZ);
            Block.Set(0, Projection);
            Block.Set(144, (Vector2)Size); // set data in uniform block
        }
        private void SetBlock(Matrix4 Matrix)
        {
            Block.Set(64, Matrix); // set camera matrix in uniform block
            Block.Set(128, Transform.Position); // set position in uniform block
        }
    }
}
