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

        public readonly int ColourTexture; // colour texture
        public readonly int NormalTexture; // normal texture
        public readonly int PositionTexture; // position texture
        public readonly int DepthBuffer; // depth buffer


        public List<IRenderLight> Lights = new List<IRenderLight>();
        public List<IRenderObject> Objects = new List<IRenderObject>();

        public Scene(string VertexShader, string FragmentShader, int Width, int Height) : base(Width, Height)
        {
            
            Camera = new Camera(50, Width, Height, 2, 512);
            Resize += (Size) => Camera.Resize(Size);

            // geometry-buffer textures
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
            Material.Uniforms["ViewPos"] = () => Camera.Position;
        }

        /// <summary>
        /// renders objects inside this viewport and updates the viewport textures
        /// </summary>
        public void Render()
        {
            // geometry shader pass
            this.RenderTo();
            foreach (IRenderObject RO in Objects) RO.Render();
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
        private class GeometryFrameBuffer : FrameBufferObject
        {

            
            public GeometryFrameBuffer(int Width, int Height) : base(Width, Height)
            {

            }
        }
    }

    
}


