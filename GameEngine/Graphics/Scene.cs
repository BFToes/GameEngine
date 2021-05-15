using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using Graphics.Shaders;
using System.Drawing;
using OpenTK.Mathematics;

namespace Graphics
{
    /* Thing To Do(Lighting): 
     * 1: attenuation ie limit effect over distance
     * 2: light volumes & tile based rendering
     * 3: forward render for shadows
     * https://learnopengl.com/Advanced-Lighting/Deferred-Shading
     * https://www.digipen.edu/sites/default/files/public/docs/theses/denis-ishmukhametov-master-of-science-in-computer-science-thesis-efficient-tile-based-deferred-shading-pipeline.pdf
     * https://cglearn.codelight.eu/pub/advanced-computer-graphics/deferred-rendering
     * 
     * 
     * Volume Lighting
     * https://ogldev.org/www/tutorial36/tutorial36.html
     * ^
     * newer OpenGL but more complicated and less explicit on what to do. still better than some.
     * 
     * Stencil Lighting
     * https://ogldev.org/www/tutorial40/tutorial40.html
     * https://www.angelfire.com/games5/duktroa/RealTimeShadowTutorial.htm
     * https://nehe.gamedev.net/tutorial/shadows/16010/
     * https://www.gamedev.net/articles/programming/graphics/the-theory-of-stencil-shadow-volumes-r1873/
     * ^
     * Uses old openGL with a fixed pipeline. also targetted at older hardware.
     * 
     * per instance data storage - allow for multiple materials
     * model importing
     * create mesh class
     * PBR materials
     * Render Textures
     * 
     */
    class Scene : FrameBuffer
    {
        public Camera Camera;
        public UniformBlock CameraBlock;
        public UniformBlock LightBlock;
        
        

        public readonly int AlbedoTexture; // colour texture
        public readonly int NormalTexture; // normal texture
        public readonly int PositionTexture; // position texture
        public readonly int DepthBuffer; // depth buffer

        public List<IRenderable> Objects = new List<IRenderable>();
        public List<UniformBlock> UniformBlocks = new List<UniformBlock>();
        public Scene(string VertexShader, string FragmentShader, int Width, int Height) : base(Width, Height)
        {
            #region Finish FrameBuffer Setup as geometry buffer
            // geometry-buffer textures
            PositionTexture = NewTextureAttachment(FramebufferAttachment.ColorAttachment0, Width, Height);
            NormalTexture = NewTextureAttachment(FramebufferAttachment.ColorAttachment1, Width, Height);
            AlbedoTexture = NewTextureAttachment(FramebufferAttachment.ColorAttachment2, Width, Height);

            DepthBuffer = NewRenderBufferAttachment(RenderbufferStorage.Depth24Stencil8, FramebufferAttachment.DepthStencilAttachment, Width, Height);
            
            // draws to multiple textures at once
            GL.DrawBuffers(3, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2 });

            FramebufferErrorCode FrameStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (FrameStatus != FramebufferErrorCode.FramebufferComplete) throw new Exception(FrameStatus.ToString());
            #endregion

            // set camera object
            Camera = new Camera(this, 50, Width, Height, 2, 512);
            CameraBlock = UniformBlock.For<CameraData>(0);
            UniformBlocks.Add(CameraBlock);
            
            // set up light block
            LightBlock = UniformBlock.For<LightData>(1, 32);
            UniformBlocks.Add(LightBlock);

            // set up shader program
            PostProcess = ShaderProgram.From(VertexShader, FragmentShader);

            PostProcess.SetUniformSampler2D("PositionTexture", PositionTexture);
            PostProcess.SetUniformSampler2D("NormalTexture", NormalTexture);
            PostProcess.SetUniformSampler2D("ColourTexture", AlbedoTexture);
            
            // set up light block
            PostProcess.SetUniform("LightCount", 0);
            PostProcess.SetUniformBlock("LightBlock", 1); // tell shader program to use binding index 1 for light block

            LightBlock.Set(0, new Vector4(40, 10, 0, 0), 0); // set light position in uniform block
            LightBlock.Set(16, new Vector4(1, 0, 1, 0), 0); // set light color
            PostProcess.SetUniform("LightCount", 1); // number of lights

            RefreshCol = Color.FromArgb(0, 0, 0, 0);
        }

        /// <summary>
        /// renders objects inside this viewport and updates the viewport textures
        /// </summary>
        public void Process()
        {
            // geometry shader pass
            GL.Viewport(0, 0, Size.X, Size.Y);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, FBO);
            GL.ClearColor(RefreshCol);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            foreach (UniformBlock UB in UniformBlocks) UB.Bind();
            foreach (IRenderable RO in Objects) RO.Render();
            
            // final post processing pass which currently adds lights is called when the frame buffer is rendered
        }

        #region RenderObject Management
        public void Add(IRenderable item) => Objects.Add(item);
        public void Remove(IRenderable item) => Objects.Remove(item);
        #endregion
    }
}


