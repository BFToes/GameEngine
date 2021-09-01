using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using GameEngine.Components;
using ECS;

namespace GameEngine
{

    public enum UniformBlockIndex
    {
        Camera = 0,
        Lights = 1,
    }

    class Scene : Context
    {
        public Camera camera;
        private GeometryBuffer gBuffer;

        private Vector2i _size;
        public Vector2i size
        {
            get => _size;
            set
            {
                _size = value;

                gBuffer.Size = _size;
                camera.Resize(_size);
            }
        }

        public Scene(int Width, int Height) : base()
        {
            _size = new Vector2i(Width, Height);
            camera = new Camera(this, Width, Height);
            gBuffer = new GeometryBuffer(Width, Height);
        }

        public void Render(int DrawTarget)
        {

        }

        public void Process(float delta) 
        { 

        }
        private class GeometryBuffer : FrameBuffer
        {
            public readonly int AlbedoTexture;   // colour texture
            public readonly int NormalTexture;   // normal texture
            public readonly int PositionTexture; // position texture
            public readonly int DepthBuffer;     // depth buffer
            public GeometryBuffer(int Width, int Height) : base(Width, Height)
            {
                // geometry-buffer textures
                PositionTexture = NewTextureAttachment(FramebufferAttachment.ColorAttachment0, Width, Height);
                NormalTexture = NewTextureAttachment(FramebufferAttachment.ColorAttachment1, Width, Height);
                AlbedoTexture = NewTextureAttachment(FramebufferAttachment.ColorAttachment2, Width, Height);
                DepthBuffer = NewRenderBufferAttachment(RenderbufferStorage.DepthComponent24, FramebufferAttachment.DepthAttachment, Width, Height);

                // this frame buffer draws to multiple textures at once
                GL.DrawBuffers(3, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2 });

                RefreshColour = new Color4(0, 0, 0, 0);
            }

            public override void Use()
            {
                // prepares openGL for rendering objects instead of lighting
                base.Use();
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.DepthMask(true);
                GL.Disable(EnableCap.StencilTest);
                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Back);
            }
        }
    }
}
