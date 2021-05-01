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
     * - Depth Texture currently doesnt work as t breaks the depth testing. dont know why really.
     * - Can't find nice numbers for orthographics camera matrix
     * - 
     * 
     */
    class ViewPort
    {
        public readonly string ColorTexture;
        //public readonly string DepthTexture;

        public Color4 RefreshCol = Color.Green;
        public Camera Camera;
        public ShaderProgram Material;

        private int Tex; // color texture
        //private int Dep; // depth texture

        private int FBO; // frame buffer object
        private int RBO; // depth buffer object


        
        private List<IRenderObject> ObjectPool = new List<IRenderObject>();
        private Rectangle rect;

        public ViewPort(string VertexShader, string FragmentShader, int PositionX, int PositionY, int Width, int Height)
        {
            Camera = new Camera(50, Width, Height, 2, 512);
            rect = new Rectangle(PositionX, PositionY, Width, Height); // calls resize
            SetRenderBuffer(RBO, RenderbufferStorage.DepthComponent, rect.Width, rect.Height);
            SetTextureAttachment(Tex, PixelFormat.Rgb, rect.Width, rect.Height);
            //SetTextureAttachment(Dep, PixelFormat.DepthComponent, rect.Width, rect.Height);

            // set up frame buffer object
            FBO = GL.GenFramebuffer(); 
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

            // Generate and set new textures
            Tex = NewTextureAttachment(out ColorTexture); SetTextureAttachment(Tex, PixelFormat.Rgb, Width, Height); // set up color texture
            //Dep = NewTextureAttachment(out DepthTexture); SetTextureAttachment(Dep, PixelFormat.DepthComponent, Width, Height); // set up depth texture
            
            // assign textures to shader program
            Material = new ShaderProgram(VertexShader, FragmentShader);
            Material.Uniforms["ColorTex"] = () => ColorTexture;
            //Material.Uniforms["DepthTex"] = () => DepthTexture;

            // set up render buffer object
            RBO = GL.GenRenderbuffer(); SetRenderBuffer(RBO, RenderbufferStorage.DepthComponent, Width, Height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, RBO);

            
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Tex, 0);
            // trying to save depth component to a texture broke the rendering?
            //GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, Dep, 0);

            FramebufferErrorCode FrameStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (FrameStatus != FramebufferErrorCode.FramebufferComplete) throw new Exception(FrameStatus.ToString());

            // deselect openGL objects
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public Rectangle Rect
        {
            set
            {
                rect = value;
                // updates respective buffer sizes and textures to new dimensions of Rect
                SetRenderBuffer(RBO, RenderbufferStorage.DepthComponent, rect.Width, rect.Height);
                SetTextureAttachment(Tex, PixelFormat.Rgb, rect.Width, rect.Height);
                //SetTextureAttachment(Dep, PixelFormat.DepthComponent, rect.Width, rect.Height);

                Camera.Resize(new Vector2(rect.Width, rect.Height));
            }
            get => rect;
        }

        /// <summary>
        /// renders objects inside this viewport and updates the viewport textures
        /// </summary>
        public void OnRender()
        {
            // use this viewport
            GL.Viewport(this.Rect);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, FBO);

            // clear this viewport frame buffer
            GL.ClearColor(RefreshCol);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // render each child in this viewport
            foreach (IRenderObject RO in ObjectPool) RO.OnRender(); // render in Z index order, must init render list to iterate through

        }

        public Bitmap GetTexture()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            Bitmap bmp = new Bitmap(rect.Width, rect.Height);

            System.Drawing.Imaging.BitmapData data = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            ErrorCode ec = GL.GetError();
            GL.Flush();
            GL.ReadBuffer(ReadBufferMode.Back);
            GL.ReadPixels(0, 0, rect.Width, rect.Height, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
            ec = GL.GetError();
            
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
        public void Add(IRenderObject item)
        {
            ObjectPool.Add(item);
            item.Set_Z_Index += Sort;
        }
        /// <summary>
        ///  removes object from render list.
        /// </summary>
        /// <param name="item"></param>
        public void Remove(IRenderObject item)
        {
            item.Set_Z_Index -= Sort;
            ObjectPool.Remove(item);
        }
        /// <summary>
        /// sorts item by Z index
        /// </summary>
        /// <param name="_">unused value</param>
        void Sort(int _) => ObjectPool = ObjectPool.OrderBy(Object => Object.Z_index).ToList();
        #endregion

        #region RenderObject buffers/Texture initiation
        /// <summary>
        /// creates new viewport texture
        /// </summary>
        /// <param name="path">the texture path.</param>
        /// <returns></returns>
        private int NewTextureAttachment(out string path)
        {
            int Handle;
            GL.CreateTextures(TextureTarget.Texture2D, 1, out Handle); // creates texture2D on handle
            GL.BindTexture(TextureTarget.Texture2D, Handle);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            path = $"ViewportTexture/{TextureManager.VPTextures++}";
            TextureManager.Add(path, Handle);
            return Handle;
        }
        /// <summary>
        /// assign size and empty values to Texture
        /// </summary>
        /// <param name="Texture">the texture ID int</param>
        /// <param name="PixelFormat">the format of the each pixel eg rgb or DepthComponent</param>
        /// <param name="Width">the width of the image.</param>
        /// <param name="Height">the height of the image.</param>
        private void SetTextureAttachment(int Texture, PixelFormat PixelFormat, int Width, int Height)
        {
            GL.BindTexture(TextureTarget.Texture2D, Texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, Width, Height, 0, PixelFormat, PixelType.Float, (float[])null);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
        /// <summary>
        /// Sets an initiated render buffer.
        /// </summary>
        /// <param name="RBO">render buffer object ID integer.</param>
        /// <param name="Storage">the storage type of this buffer</param>
        /// <param name="Width">the width of the image</param>
        /// <param name="Height">the height of the image</param>
        private void SetRenderBuffer(int RBO, RenderbufferStorage Storage, int Width, int Height)
        {
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RBO);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, Storage, Width, Height);
        }
        #endregion

    }
}
