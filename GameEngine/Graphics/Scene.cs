using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using Graphics.Shaders;

namespace Graphics
{
    class Scene : FrameBufferObject
    {
        public Camera Camera;
        public UniformBlock CameraBlock;
        
        public ShaderProgram Material;
        

        public readonly int AlbedoTexture; // colour texture
        public readonly int NormalTexture; // normal texture
        public readonly int PositionTexture; // position texture
        public readonly int DepthBuffer; // depth buffer

        public List<IRenderable> Objects = new List<IRenderable>();
        public List<UniformBlock> UniformBlocks = new List<UniformBlock>();
        public Scene(string VertexShader, string FragmentShader, int Width, int Height) : base(Width, Height)
        {
            // geometry-buffer textures
            PositionTexture = NewTextureAttachment(PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, FramebufferAttachment.ColorAttachment0, Width, Height);
            NormalTexture = NewTextureAttachment(PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, FramebufferAttachment.ColorAttachment1, Width, Height);
            AlbedoTexture = NewTextureAttachment(PixelInternalFormat.Rgba, PixelFormat.Rgba, PixelType.UnsignedByte, FramebufferAttachment.ColorAttachment2, Width, Height);
            // draws to multiple textures at once
            GL.DrawBuffers(3, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2 });
            DepthBuffer = NewRenderBufferAttachment(RenderbufferStorage.DepthComponent24, FramebufferAttachment.DepthAttachment, Width, Height);

            //FramebufferErrorCode FrameStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            //if (FrameStatus != FramebufferErrorCode.FramebufferComplete) throw new Exception(FrameStatus.ToString());

            // set camera object
            Camera = new Camera(this, 50, Width, Height, 2, 512);
            CameraBlock = UniformBlock.For<CameraData>(0);
            UniformBlocks.Add(CameraBlock);


            // set up shader program
            Material = new ShaderProgram(VertexShader, FragmentShader);

            Material.SetUniformSampler2D("PositionTexture", PositionTexture);
            Material.SetUniformSampler2D("NormalTexture", NormalTexture);
            Material.SetUniformSampler2D("ColourTexture", AlbedoTexture);
            Material.SetUniform("ViewPos", Camera.Position);
        }

        /// <summary>
        /// renders objects inside this viewport and updates the viewport textures
        /// </summary>
        public void Process()
        {
            // geometry shader pass
            this.UseFrameBuffer();
            foreach (UniformBlock UB in UniformBlocks) UB.Bind();
            foreach (IRenderable RO in Objects) RO.Render();

        }
        #region RenderObject Management
        public void Add(IRenderable item) => Objects.Add(item);
        public void Remove(IRenderable item) => Objects.Remove(item);
        #endregion
    }
}


