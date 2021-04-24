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

    public interface IViewPort
    {
        public Rectangle Rect { get; }
    }
    class ViewPort
    {
        public readonly string ColorTexture;
        public readonly string DepthTexture;

        public Color4 RefreshCol = Color.Green;
        public ICamera Camera;
        public ShaderProgram Material;

        private int Tex; // color texture
        private int Dep; // depth texture
        private int LFTex; // Last frame's color texture

        private int FBO; // frame buffer object
        private int RBO; // depth buffer object


        
        private List<IRenderObject> ObjectPool = new List<IRenderObject>();
        private Rectangle rect;

        public ViewPort(string VertexShader, string FragmentShader, int PositionX, int PositionY, int Width, int Height)
        {
            
            Rect = new Rectangle(PositionX, PositionY, Width, Height); // calls resize
            Camera = new Camera(50, new Vector2(2, 2), 1, 10);


            // set up frame buffer object
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

            string ColTexPath, DepTexPath;
            // Generate and set new textures
            Tex = NewTextureAttachment(out ColTexPath); SetTextureAttachment(Tex, PixelFormat.Rgb, Width, Height); // set up color texture
            Dep = NewTextureAttachment(out DepTexPath); SetTextureAttachment(Dep, PixelFormat.DepthComponent, Width, Height); // set up depth texture
            //LFTex = NewTextureAttachment(out ColorTexture); SetTextureAttachment(LFTex, PixelFormat.DepthComponent, Width, Height);
            
            Material = new ShaderProgram(VertexShader, FragmentShader);
            Material.Uniforms["ColorTex"] = () => ColTexPath;
            Material.Uniforms["DepthTex"] = () => DepTexPath;


            // set up render buffer object
            RBO = GL.GenRenderbuffer(); SetRenderBuffer(RBO, RenderbufferStorage.DepthComponent, Width, Height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.Depth, RenderbufferTarget.Renderbuffer, RBO);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.Depth, TextureTarget.Texture2D, Dep, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Tex, 0);
            
            FramebufferErrorCode FrameStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (FrameStatus != FramebufferErrorCode.FramebufferComplete) throw new Exception(FrameStatus.ToString());

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
                SetTextureAttachment(Dep, PixelFormat.DepthComponent, rect.Width, rect.Height);
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
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

            // clear this viewport frame buffer
            GL.ClearColor(RefreshCol);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // render each child in this viewport
            foreach (IRenderObject RO in ObjectPool) RO.OnRender(); // render in Z index order, must init render list to iterate through



            /*
            // copies this frame to last frame texture -> only needed if sampling texture within itself
            //GL.BindTexture(TextureTarget.Texture2D, LFTex);
            //GL.CopyTexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb, 0, 0, Rect.Width, Rect.Height, 0);
            */
            /*
            // Copies Depth buffer to default frame buffer -> I dont know if its needed
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.BlitFramebuffer(0, 0, Rect.Width, Rect.Height, 0, 0, Rect.Width, Rect.Height, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
            */
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


    }
}
