using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Drawing;
using System;


namespace GameEngine
{
    abstract class FrameBuffer
    {
        public Color4 RefreshColour = Color.Purple;
        public int RefreshStencil = 0;

        public Vector2i Size
        {
            get => size;
            set => setSize(value);
        }
        private Action<Vector2i> setSize;
        private Vector2i size;

        private int FBO; // frame buffer object handle

        public FrameBuffer(int Width, int Height)
        {
            setSize = (newSize) => size = newSize;
            Size = new Vector2i(Width, Height);

            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
        }

        #region Buffer Attachment
        /// <summary>
        /// assign new render buffer and bind to framebuffer object
        /// </summary>
        protected int NewRenderBufferAttachment(RenderbufferStorage Storage, FramebufferAttachment Attachment, int Width, int Height, bool AutoResize = true)
        {
            int RBO = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RBO);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, Storage, Width, Height);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, Attachment, RenderbufferTarget.Renderbuffer, RBO);

            if (AutoResize)
                setSize += (Size) => SetRenderBuffer(RBO, Storage, Size.X, Size.Y);

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
        #endregion

        #region Texture Attachment
        /// <summary>
        /// creates new viewport texture
        /// </summary>
        /// <param name="path">the texture path.</param>
        /// <returns></returns>
        protected int NewTextureAttachment(FramebufferAttachment Attachment, int Width, int Height, bool AutoResize = true, PixelInternalFormat PixelInternalFormat = PixelInternalFormat.Rgba16f, PixelFormat PixelFormat = PixelFormat.Rgba, PixelType PixelType = PixelType.Float, TextureMinFilter MinFilter = TextureMinFilter.Nearest, TextureMagFilter MagFilter = TextureMagFilter.Nearest)
        {
            int Tex = GL.GenTexture(); // generate texture
            GL.BindTexture(TextureTarget.Texture2D, Tex); // bind to current texture
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat, Width, Height, 0, PixelFormat, PixelType, (float[])null); // set texture to empty texture of width and height
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)MinFilter); // set texture minimize function
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)MagFilter); // set texture magnify function
            GL.BindTexture(TextureTarget.Texture2D, 0); // unbind from texture 

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, Attachment, TextureTarget.Texture2D, Tex, 0);

            if (AutoResize) // if Resize
                setSize += (Size) => SetTextureAttachment(Tex, Size.X, Size.Y, PixelInternalFormat, PixelFormat, PixelType);

            return Tex;
        }
        /// <summary>
        /// reassign sizes to texture
        /// </summary>
        /// <param name="Texture">the texture ID int</param>
        /// <param name="Width">the width of the image.</param>
        /// <param name="Height">the height of the image.</param>
        protected void SetTextureAttachment(int Texture, int Width, int Height, PixelInternalFormat PixelInternalFormat, PixelFormat PixelFormat, PixelType PixelType)
        {
            GL.BindTexture(TextureTarget.Texture2D, Texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat, Width, Height, 0, PixelFormat, PixelType, (float[])null);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
        #endregion

        #region Texture Cube Attachment
        /// <summary>
        /// creates new texture cube attachment
        /// </summary>
        protected int NewTextureCubeAttachment(int Width, int Height, bool AutoResize = false, FramebufferAttachment Attachment = FramebufferAttachment.DepthAttachment, PixelInternalFormat PixelInternalFormat = PixelInternalFormat.DepthComponent, PixelFormat PixelFormat = PixelFormat.DepthComponent, PixelType PixelType = PixelType.Float, TextureMinFilter MinFilter = TextureMinFilter.Nearest, TextureMagFilter MagFilter = TextureMagFilter.Nearest, TextureWrapMode WrapMode = TextureWrapMode.ClampToEdge)
        {
            int TextureCube = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, TextureCube);

            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, PixelInternalFormat, Width, Height, 0, PixelFormat, PixelType, (float[])null);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeX, 0, PixelInternalFormat, Width, Height, 0, PixelFormat, PixelType, (float[])null);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveY, 0, PixelInternalFormat, Width, Height, 0, PixelFormat, PixelType, (float[])null);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeY, 0, PixelInternalFormat, Width, Height, 0, PixelFormat, PixelType, (float[])null);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveZ, 0, PixelInternalFormat, Width, Height, 0, PixelFormat, PixelType, (float[])null);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeZ, 0, PixelInternalFormat, Width, Height, 0, PixelFormat, PixelType, (float[])null);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)MagFilter);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)MinFilter);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)WrapMode);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)WrapMode);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)WrapMode);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, Attachment, TextureCube, 0);

            if (AutoResize)
                setSize += (Size) => SetTextureCubeAttachment(TextureCube, Size.X, Size.Y, PixelInternalFormat, PixelFormat, PixelType);

            return TextureCube;
        }
        /// <summary>
        /// reassigns sizes to textures in cube
        /// </summary>
        protected void SetTextureCubeAttachment(int TextureCube, int Width, int Height, PixelInternalFormat PixelInternalFormat, PixelFormat PixelFormat, PixelType PixelType)
        {
            GL.BindTexture(TextureTarget.TextureCubeMap, TextureCube);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, PixelInternalFormat, Width, Height, 0, PixelFormat, PixelType, (float[])null);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeX, 0, PixelInternalFormat, Width, Height, 0, PixelFormat, PixelType, (float[])null);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveY, 0, PixelInternalFormat, Width, Height, 0, PixelFormat, PixelType, (float[])null);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeY, 0, PixelInternalFormat, Width, Height, 0, PixelFormat, PixelType, (float[])null);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveZ, 0, PixelInternalFormat, Width, Height, 0, PixelFormat, PixelType, (float[])null);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeZ, 0, PixelInternalFormat, Width, Height, 0, PixelFormat, PixelType, (float[])null);
            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
        }
        #endregion

        /// <summary>
        /// sets this frame buffer to the render target
        /// </summary>
        public virtual void Use()
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, FBO);
            GL.Viewport(0, 0, Size.X, Size.Y);
            GL.ClearColor(RefreshColour);
            GL.ClearStencil(RefreshStencil);
        }

        /// <summary>
        /// throws exception if framebuffer has loaded incorrectly. must be used immediately after construction
        /// </summary>
        public virtual void CheckFrameBufferStatus()
        {
            FramebufferErrorCode FrameStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (FrameStatus != FramebufferErrorCode.FramebufferComplete) throw new Exception(FrameStatus.ToString());
        }

        public static implicit operator int(FrameBuffer FB) => FB.FBO;
    }
}
