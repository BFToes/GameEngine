using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;
using Graphics.Shaders;
using OpenTK.Windowing.Common;

namespace Graphics
{
    /* Notes:
     * - Z index sorting obselete unless object transparent
     * 
     */

    enum RenderPriority
    {
        Lighting,
        Objects,
        TransparentObjects
    }

    class Scene : FrameBufferObject
    {

        public Camera Camera;
        public ShaderProgram Material;

        private int ColourTexture; // colour texture
        private int NormalTexture; // normal texture
        private int PositionTexture; // position texture
        private int DepthBuffer; // depth buffer

        public List<IRenderLight> Lights = new List<IRenderLight>();
        public List<IRenderObject> Objects = new List<IRenderObject>();

        public Scene(string VertexShader, string FragmentShader, int Width, int Height) : base(Width, Height)
        {
            Camera = new Camera(50, Width, Height, 2, 512);
            Resize += (Size) => Camera.Resize(Size);

            // g-buffer textures
            PositionTexture = NewTextureAttachment(PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, FramebufferAttachment.ColorAttachment0, Width, Height);
            NormalTexture = NewTextureAttachment(PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, FramebufferAttachment.ColorAttachment1, Width, Height);
            ColourTexture = NewTextureAttachment(PixelInternalFormat.Rgba, PixelFormat.Rgba, PixelType.UnsignedByte, FramebufferAttachment.ColorAttachment2, Width, Height);
            // draws to multiple textures at once
            GL.DrawBuffers(3, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2 });
            
            DepthBuffer = NewRenderBufferAttachment(RenderbufferStorage.DepthComponent24, FramebufferAttachment.DepthAttachment, Width, Height);

            FramebufferErrorCode FrameStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (FrameStatus != FramebufferErrorCode.FramebufferComplete) throw new Exception(FrameStatus.ToString());

            // assign textures to shader program
            Material = new ShaderProgram(VertexShader, FragmentShader);
            Material.Uniforms["PositionTexture"] = () => PositionTexture;
            Material.Uniforms["NormalTexture"] = () => NormalTexture;
            Material.Uniforms["ColourTexture"] = () => ColourTexture;

        }

        /// <summary>
        /// renders objects inside this viewport and updates the viewport textures
        /// </summary>
        public void OnRender()
        {

            GL.CullFace(CullFace);
            GL.DepthFunc(DepthFunc);

            // use this viewport
            GL.Viewport(0, 0, Size.X, Size.Y);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, FBO);

            GL.ClearColor(RefreshCol);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, PositionTexture);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, NormalTexture);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, ColourTexture);

            // render each child in this viewport
            foreach (IRenderable RO in Objects) RO.OnRender(); // render in Z index order, must init render list to iterate through

        }
        public Bitmap GetTexture()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            Bitmap bmp = new Bitmap(size.X, size.Y);

            System.Drawing.Imaging.BitmapData data = bmp.LockBits(new Rectangle(0, 0, size.X, size.Y), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            
            GL.Flush();
            GL.ReadBuffer(ReadBufferMode.Back);
            GL.ReadPixels(0, 0, size.X, size.Y, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);

            bmp.UnlockBits(data);
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            return bmp;
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


