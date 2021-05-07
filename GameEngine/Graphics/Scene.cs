using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using Graphics.Shaders;

namespace Graphics
{
    /* Notes:
     * - Z index sorting obselete unless object transparent
     * - Currently not complete
     */
    class Scene : FrameBufferObject
    {
        public Camera Camera;
        public ShaderProgram Material;

        public readonly int ColourTexture; // colour texture
        public readonly int NormalTexture; // normal texture
        public readonly int PositionTexture; // position texture
        public readonly int DepthBuffer; // depth buffer


        public List<IRenderLight> Lights = new List<IRenderLight>();
        public List<IRenderObject> Objects = new List<IRenderObject>();

        public Scene(string VertexShader, string FragmentShader, int Width, int Height) : base(Width, Height)
        {
            // geometry-buffer textures
            PositionTexture = NewTextureAttachment(PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, FramebufferAttachment.ColorAttachment0, Width, Height);
            NormalTexture = NewTextureAttachment(PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, FramebufferAttachment.ColorAttachment1, Width, Height);
            ColourTexture = NewTextureAttachment(PixelInternalFormat.Rgba, PixelFormat.Rgba, PixelType.UnsignedByte, FramebufferAttachment.ColorAttachment2, Width, Height);
            // draws to multiple textures at once
            GL.DrawBuffers(3, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2 });
            DepthBuffer = NewRenderBufferAttachment(RenderbufferStorage.DepthComponent24, FramebufferAttachment.DepthAttachment, Width, Height);

            FramebufferErrorCode FrameStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (FrameStatus != FramebufferErrorCode.FramebufferComplete) throw new Exception(FrameStatus.ToString());

            // set camera object
            Camera = new Camera(50, Width, Height, 2, 512);
            Resize += (Size) => Camera.Resize(Size);

            // assign textures to shader program
            Material = new ShaderProgram(VertexShader, FragmentShader);
            //Material.SetUniformSampler2D("PositionTexture", PositionTexture);
            //Material.SetUniformSampler2D("NormalTexture", NormalTexture);
            Material.SetUniformSampler2D("ColourTexture", ColourTexture);
            //Material.SetUniform("ViewPos", Camera.Position);
        }

        /// <summary>
        /// renders objects inside this viewport and updates the viewport textures
        /// </summary>
        public void Render()
        {
            // geometry shader pass
            this.RenderToThis();
            foreach (IRenderObject RO in Objects) RO.Render();
        }
        #region RenderObject Management
        /// <summary>
        /// adds object to render list.
        /// </summary>
        /// <param name="item"></param>
        public void Add(IRenderObject item) => Objects.Add(item);
        public void Add(IRenderLight item) => Lights.Add(item);

        public void Remove(IRenderObject item) => Objects.Remove(item);
        public void Remove(IRenderLight item) => Lights.Remove(item);

        #endregion
    }

    
}


