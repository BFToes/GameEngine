using Graphics.Shaders;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Drawing;

namespace Graphics
{
    abstract class FrameBuffer
    {
        public ShaderProgram PostProcess;

        public Color4 RefreshCol = Color.Purple;
        public Vector2i Size
        {
            get => size;
            set => PrivateResize(value);
        }

        // seperated events so that updating texture attachment events dont get removed
        public Action<Vector2i> Resize;
        private Action<Vector2i> PrivateResize; 

        private Vector2i size;
        
        protected int VAO; // vertex array object
        protected int VBO; // vertex buffer
        protected int FBO; // frame buffer object

        public FrameBuffer(int Width, int Height)
        {
            Resize = (newSize) => { };
            PrivateResize = (newSize) => 
            { 
                size = newSize; 
                Resize(newSize); 
            };
            Size = new Vector2i(Width, Height);
            
            #region vertex buffer/array object setup
            // setup array
            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);
            // vector2 position
            GL.VertexArrayAttribBinding(VAO, 0, 0); // generates a new attribute binding to location in vertex buffer array
            GL.EnableVertexArrayAttrib(VAO, 0); // enables the attribute binding to location
            GL.VertexArrayAttribFormat(VAO, 0, 2, VertexAttribType.Float, false, 0);
            // vector2 FragUV
            GL.VertexArrayAttribBinding(VAO, 1, 0);
            GL.EnableVertexArrayAttrib(VAO, 1);
            GL.VertexArrayAttribFormat(VAO, 1, 2, VertexAttribType.Float, false, 8);
            // setup buffer
            VBO = GL.GenBuffer(); // generates and binds vertex buffer object
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO); // uses this buffer
            GL.VertexArrayVertexBuffer(VAO, 0, VBO, IntPtr.Zero, new Vertex2D().SizeInBytes); // assigns vertice data

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, 4 * new Vertex2D().SizeInBytes, new float[16] { -1, -1, 0, 0, 1, -1, 1, 0, 1, 1, 1, 1, -1, 1, 0, 1 }, BufferUsageHint.StaticDraw);
            #endregion

            #region Start Setup Framebuffer
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            #endregion
        }
        /// <summary>
        /// assign new render buffer and bind to framebuffer object
        /// </summary>
        /// <param name="Storage"></param>
        /// <param name="Attachment"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <returns></returns>
        protected int NewRenderBufferAttachment(RenderbufferStorage Storage, FramebufferAttachment Attachment, int Width, int Height, bool AutoResize = true)
        {
            int RBO = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RBO);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, Storage, Width, Height);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, Attachment, RenderbufferTarget.Renderbuffer, RBO);

            if (AutoResize) 
                PrivateResize += (Size) => SetRenderBuffer(RBO, Storage, Size.X, Size.Y);


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
                PrivateResize += (Size) => SetTextureAttachment(Tex, Size.X, Size.Y, PixelInternalFormat, PixelFormat, PixelType);
            
            return Tex;
        }
        /// <summary>
        /// reassign sizes to texture
        /// </summary>
        /// <param name="Texture">the texture ID int</param>
        /// <param name="PixelFormat">the format of the each pixel eg rgb or DepthComponent</param>
        /// <param name="Width">the width of the image.</param>
        /// <param name="Height">the height of the image.</param>
        protected void SetTextureAttachment(int Texture, int Width, int Height, PixelInternalFormat PixelInternalFormat, PixelFormat PixelFormat, PixelType PixelType)
        {
            GL.BindTexture(TextureTarget.Texture2D, Texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat, Width, Height, 0, PixelFormat, PixelType, (float[])null);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
        /// <summary>
        /// sets this frame buffer to the render target
        /// </summary>
        public virtual void Render()
        {
            PostProcess.Use();
            GL.BindVertexArray(VAO);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
        }
    }
}
