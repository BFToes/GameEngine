using System;
using System.Collections.Generic;
using System.Text;
using GameEngine.Components;
using ECS;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;
using GameEngine.Resources;

namespace GameEngine
{
    class Camera : Entity
    {
        public ref Transform Transform => ref GetComponent<Transform>();

        private Matrix4 _projection;
        private UniformBlock Block = UniformBlock.For<Camera.UniformBlockData>((int)UniformBlockIndex.Camera);
        private readonly float FOV, nearZ, farZ;
        private readonly Vector2i Size;
        public Camera(Context context, int width, int height, float fov = 0.9f, float depthNear = 0.125f, float depthFar = 512f) : base(context, ComponentManager.ID<Transform, CullObserver, Hierarchy>())
        {
            this.FOV = fov;
            this.nearZ = depthNear;
            this.farZ = depthFar;
            this.Size = new Vector2i(width, height);
            Resize(Size);

            Transform = new Transform(Vector3.Zero, Vector3.Zero, Vector3.One);
        }
        internal void Resize(Vector2i Size)
        {
            if (FOV != 0)
                _projection = Matrix4.CreatePerspectiveFieldOfView(FOV, Size.X / Size.Y, nearZ, farZ);
            else
                _projection = Matrix4.CreateOrthographic(Size.X, Size.Y, nearZ, farZ);
        }
        public void Use()
        {
            Block.Set(new UniformBlockData(_projection, Transform.matrix.Inverted(), Transform.WorldPosition, Size));
            Block.Bind();
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct UniformBlockData
        {
            [FieldOffset(0)]
            public Matrix4 Projection; // + 64
            [FieldOffset(64)]
            public Matrix4 Transform; // + 64
            [FieldOffset(128)]
            public Vector3 WorldPosition; // + 16
            [FieldOffset(144)]
            public Vector2 ScreenSize; // + 8

            public UniformBlockData(Matrix4 Projection, Matrix4 Transform, Vector3 WorldPosition, Vector2 ScreenSize)
            {
                this.Projection = Projection;
                this.Transform = Transform;
                this.WorldPosition = WorldPosition;
                this.ScreenSize = ScreenSize;
            }
        }



    }
}
