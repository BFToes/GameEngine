using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using Graphics.Shaders;
using System.Drawing;
using OpenTK.Mathematics;

namespace Graphics
{
    /* Volume Lighting
     * https://ogldev.org/www/tutorial36/tutorial36.html
     * ^
     * newer OpenGL but more complicated and less explicit on what to do. still better than some.
     * 
     * Stencil shadows
     * https://ogldev.org/www/tutorial40/tutorial40.html
     * https://www.angelfire.com/games5/duktroa/RealTimeShadowTutorial.htm
     * https://nehe.gamedev.net/tutorial/shadows/16010/
     * https://www.gamedev.net/articles/programming/graphics/the-theory-of-stencil-shadow-volumes-r1873/
     * 
     * IDISPOSABLE YOU PIECE OF UTTER SHIT..
     * per instance vertex data
     * model importing
     * PBR materials
     * 
     */
    class Scene
    {
        public Camera Camera;
        public UniformBlock CameraBlock = UniformBlock.For<CameraData>(0);
        //public ShaderProgram LightProgram;
        public ShaderProgram DebugProgram;

        private GeometryBuffer GBuffer;
        //private SceneBuffer SBuffer;

        // mesh that encompasses the entire screen
        private static Mesh<Simple2D> ScreenMesh = Mesh<Simple2D>.From<Simple2D>(new float[8] { -1, -1, 1, -1, 1, 1, -1, 1 }, PrimitiveType.TriangleFan);

        public List<ILightObject> LightObjects = new List<ILightObject>();
        public List<IRenderable> Objects = new List<IRenderable>();
        public List<UniformBlock> UniformBlocks = new List<UniformBlock>();

        private Vector2i size;
        public Vector2i Size
        {
            get => size;
            set
            {
                size = value;
                // resize framebuffers
                GBuffer.Size = size;
                //SBuffer.Size = size;
            }
        }

        public Scene(int Width, int Height)
        {
            size = new Vector2i(Width, Height);

            GBuffer = new GeometryBuffer(Width, Height);
            //SBuffer = new SceneBuffer(Width, Height);

            // setup camera
            Camera = new Camera(this, 50, Width, Height, 2, 512);
            UniformBlocks.Add(CameraBlock);
            
            // setup Light program
            /*
            LightProgram = ShaderProgram.ReadFrom("Resources/Shaderscripts/PostProcess/Light.vert", "Resources/Shaderscripts/PostProcess/Light.frag");
            LightProgram.SetUniformSampler2D("AlbedoTexture", GBuffer.AlbedoTexture);
            LightProgram.SetUniformSampler2D("NormalTexture", GBuffer.NormalTexture);
            LightProgram.SetUniformSampler2D("PositionTexture", GBuffer.PositionTexture);
            //LightProgram.DebugUniforms();
            */

            // setup Debug program
            DebugProgram = ShaderProgram.ReadFrom("Resources/Shaderscripts/PostProcess/GeomDebug.vert", "Resources/Shaderscripts/PostProcess/GeomDebug.frag");
            DebugProgram.SetUniformSampler2D("ColourTexture", GBuffer.AlbedoTexture);
            DebugProgram.SetUniformSampler2D("NormalTexture", GBuffer.NormalTexture);
            DebugProgram.SetUniformSampler2D("PositionTexture", GBuffer.PositionTexture);
            DebugProgram.DebugUniforms();
        }

        /// <summary>
        /// renders objects inside this viewport and updates the viewport textures
        /// </summary>
        public void Render()
        {
            // bind uniform blocks
            foreach (UniformBlock UB in UniformBlocks) UB.Bind();

            // render objects into Geometry buffer
            GBuffer.Use();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            foreach (IRenderable RO in Objects) RO.Render();

            #region Temp Debug
            // debug
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.ClearColor(Color.DarkRed);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            DebugProgram.Use();
            ScreenMesh.Render();
            #endregion

            #region Deferred Render Commented Out
            /*
            // ambient light pass
            SBuffer.Use();
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // point light pass
            LightProgram.Use();
            foreach (ILightObject LO in LightObjects) 
            {
                LightProgram.SetUniform("Position", LO.Position);
                LightProgram.SetUniform("Colour", LO.Colour);
                LO.Render();
            }

            // directional light pass[NOT DONE]
            */
            #endregion
        }
        #region Object Management
        public void Add(IRenderable item) => Objects.Add(item);
        public void Remove(IRenderable item) => Objects.Remove(item);
        public void Add(ILightObject item) => LightObjects.Add(item);
        public void Remove(ILightObject item) => LightObjects.Remove(item);
        #endregion

        #region FrameBuffer Objects
        private class GeometryBuffer : FrameBuffer
        {
            public readonly int AlbedoTexture; // colour texture
            public readonly int NormalTexture; // normal texture
            public readonly int PositionTexture; // position texture
            public readonly int DepthBuffer; // depth buffer
            public GeometryBuffer(int Width, int Height): base(Width, Height)
            {
                // geometry-buffer textures
                PositionTexture = NewTextureAttachment(FramebufferAttachment.ColorAttachment0, Width, Height);
                NormalTexture = NewTextureAttachment(FramebufferAttachment.ColorAttachment1, Width, Height);
                AlbedoTexture = NewTextureAttachment(FramebufferAttachment.ColorAttachment2, Width, Height);
                DepthBuffer = NewRenderBufferAttachment(RenderbufferStorage.DepthComponent24, FramebufferAttachment.DepthAttachment, Width, Height);

                // draws to multiple textures at once
                GL.DrawBuffers(3, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2 });

                FramebufferErrorCode FrameStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
                if (FrameStatus != FramebufferErrorCode.FramebufferComplete) throw new Exception(FrameStatus.ToString());

                RefreshCol = new Color4(1, 0, 1, 1);
            }

            public override void Use()
            {
                base.Use();
                //GL.Enable(EnableCap.DepthTest);
                //GL.Disable(EnableCap.Blend);
                //GL.DepthMask(true);
            }
        }

        private class SceneBuffer : FrameBuffer
        {
            public readonly int ColourTexture; // colour texture
            public readonly int DepthStencil; // depth buffer

            public SceneBuffer(int Width, int Height) : base(Width, Height)
            {

                // geometry-buffer textures
                ColourTexture = NewTextureAttachment(FramebufferAttachment.ColorAttachment0, Width, Height);
                DepthStencil = NewRenderBufferAttachment(RenderbufferStorage.Depth24Stencil8, FramebufferAttachment.DepthStencilAttachment, Width, Height);

                // draws to multiple textures at once
                GL.DrawBuffers(1, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0 });

                FramebufferErrorCode FrameStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
                if (FrameStatus != FramebufferErrorCode.FramebufferComplete) throw new Exception(FrameStatus.ToString());

                RefreshCol = Color.FromArgb(0, 0, 0, 0);

            }
            public override void Use()
            {
                // begin light pass
                //GL.Enable(EnableCap.Blend);
                //GL.BlendEquation(BlendEquationMode.FuncAdd);
                //GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
                base.Use();
            }
        }
        #endregion
    }
}


