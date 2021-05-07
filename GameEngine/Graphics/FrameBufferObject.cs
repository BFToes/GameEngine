using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Graphics
{
    abstract class FrameBufferObject
    {
        public Color4 RefreshCol = Color.Purple;
        public CullFaceMode CullFace = CullFaceMode.Back;
        public DepthFunction DepthFunc = DepthFunction.Less;
        public Action<Vector2i> Resize;
        public Vector2i Size
        {
            get => size;
            set => Resize(value);
        }
        private Vector2i size;
        private int FBO;
        public FrameBufferObject(int Width, int Height)
        {
            Resize = (newSize) => size = newSize;
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
        }
        /// <summary>
        /// assign new render buffer and bind to framebuffer object
        /// </summary>
        /// <param name="Storage"></param>
        /// <param name="Attachment"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <returns></returns>
        protected int NewRenderBufferAttachment(RenderbufferStorage Storage, FramebufferAttachment Attachment, int Width, int Height)
        {
            int RBO = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RBO);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, Storage, Width, Height);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, Attachment, RenderbufferTarget.Renderbuffer, RBO);

            Resize += (Size) => SetRenderBuffer(RBO, Storage, Size.X, Size.Y);
            return RBO;
        }
        /// <summary>
        /// reassigns size to render buffer
        /// </summary>
        /// <param name="RBO">render buffer object ID integer.</param>
        /// <param name="Storage">the storage type of this buffer</param>
        /// <param name="Width">the width of the image</param>
        /// <param name="Height">the height of the image</param>
        protected void SetRenderBuffer(int RBO, RenderbufferStorage Storage, int Width, int Height)
        {
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RBO);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, Storage, Width, Height);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
        }
        /// <summary>
        /// creates new viewport texture
        /// </summary>
        /// <param name="path">the texture path.</param>
        /// <returns></returns>
        protected int NewTextureAttachment(PixelInternalFormat PixelInternalFormat, PixelFormat PixelFormat, PixelType PixelType, FramebufferAttachment Attachment, int Width, int Height)
        {
            
            int Tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, Tex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat, Width, Height, 0, PixelFormat, PixelType, (float[])null);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, Attachment, TextureTarget.Texture2D, Tex, 0);

            Resize += (Size) => SetTextureAttachment(Tex, PixelInternalFormat, PixelFormat, PixelType, Attachment, Size.X, Size.Y);
            return Tex;
        }
        /// <summary>
        /// reassign sizes to texture
        /// </summary>
        /// <param name="Texture">the texture ID int</param>
        /// <param name="PixelFormat">the format of the each pixel eg rgb or DepthComponent</param>
        /// <param name="Width">the width of the image.</param>
        /// <param name="Height">the height of the image.</param>
        protected void SetTextureAttachment(int Texture, PixelInternalFormat PixelInternalFormat, PixelFormat PixelFormat, PixelType PixelType, FramebufferAttachment Attachment, int Width, int Height)
        {
            GL.BindTexture(TextureTarget.Texture2D, Texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat, Width, Height, 0, PixelFormat, PixelType, (float[])null);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        /// <summary>
        /// sets this frame buffer to the render target
        /// </summary>
        public void RenderToThis()
        {
            GL.CullFace(CullFace);
            GL.DepthFunc(DepthFunc);
            // geometry shader pass
            GL.Viewport(0, 0, Size.X, Size.Y);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, FBO);
            GL.ClearColor(RefreshCol);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }
    
    }
}
