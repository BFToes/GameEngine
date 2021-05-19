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
        public UniformBlock CameraBlock;
        public ShaderProgram AmbientProgram;
        public ShaderProgram LightProgram;

        private GeometryBuffer GBuffer;
        private SceneBuffer SBuffer;

        // mesh that encompasses the entire screen
        private static Mesh<SimpleVertex> ScreenMesh = Mesh<SimpleVertex>.From<SimpleVertex>(new float[16] { -1, -1, 0, 0, 1, -1, 1, 0, 1, 1, 1, 1, -1, 1, 0, 1 });

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
                GBuffer.Size = size;
                GBuffer.Size = size;
            }
        }

        public Scene(int Width, int Height)
        {
            size = new Vector2i(Width, Height);

            GBuffer = new GeometryBuffer(Width, Height);
            SBuffer = new SceneBuffer(Width, Height);

            // set camera object
            Camera = new Camera(this, 50, Width, Height, 2, 512);
            CameraBlock = UniformBlock.For<CameraData>(0);
            UniformBlocks.Add(CameraBlock);

            AmbientProgram = ShaderProgram.ReadFrom("Resources/Shaderscripts/PostProcess/Ambient.vert", "Resources/Shaderscripts/PostProcess/Ambient.frag");
            AmbientProgram.SetUniform("Ambient", 0.1f);
            AmbientProgram.SetUniformSampler2D("ColourTexture", GBuffer.AlbedoTexture);
            AmbientProgram.SetUniformSampler2D("NormalTexture", GBuffer.NormalTexture);
            AmbientProgram.SetUniformSampler2D("PositionTexture", GBuffer.PositionTexture);

            LightProgram = ShaderProgram.ReadFrom("Resources/Shaderscripts/PostProcess/Light.vert", "Resources/Shaderscripts/PostProcess/Light.frag");

            
            /*
            // set up light block
            LightBlock = UniformBlock.For<LightData>(1, 32);
            UniformBlocks.Add(LightBlock);

            // set up shader program
            PostProcess = ShaderProgram.ReadFrom(
                $"Resources/{VertexShader}", 
                $"Resources/{FragmentShader}");

            PostProcess.SetUniformSampler2D("PositionTexture", GBuffer.PositionTexture);
            PostProcess.SetUniformSampler2D("NormalTexture", GBuffer.NormalTexture);
            PostProcess.SetUniformSampler2D("ColourTexture", GBuffer.AlbedoTexture);
            
            // set up light block
            PostProcess.SetUniform("LightCount", 0);
            PostProcess.SetUniformBlock("LightBlock", 1); // tell shader program to use binding index 1 for light block

            LightBlock.Set(0, new Vector4(2, 0, 0, 0), 0); // set light position
            LightBlock.Set(16, new Vector4(0, 1, 0, 0), 0); // set light color

            LightBlock.Set(0, new Vector4(-2, 0, 0, 0), 1); // set light position
            LightBlock.Set(16, new Vector4(0, 0, 1, 0), 1); // set light color

            LightBlock.Set(0, new Vector4(0, 0, 3.5f, 0), 2); // set light position
            LightBlock.Set(16, new Vector4(1, 0, 0, 0), 2); // set light color

            PostProcess.SetUniform("LightCount", 3); // number of lights
            */
        }

        /// <summary>
        /// renders objects inside this viewport and updates the viewport textures
        /// </summary>
        public void Render()
        {
            /* 
             * For Each Object:
                  * Render Scene objects into Geometry buffer for position, normal, Albedo
             * Render ambient light pass to the main framebuffer
               - only writes to colour buffer
             * For Each Light:
                  * Render Shadow volumes to the FBO stencil buffer
                    - only writes to stencil buffer
                  * Render Scene to FBO colour buffer from geometry buffer applying lighting 
                    - only writes to colour buffer with additive blending
             */

            // bind uniform blocks
            foreach (UniformBlock UB in UniformBlocks) UB.Bind();

            // render scene into Gbuffer
            GBuffer.Use();
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            foreach (IRenderable RO in Objects) RO.Render();

            
            // light pass
            SBuffer.Use();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            AmbientProgram.Use();
            
            // final post processing pass which currently adds lights is called when the frame buffer is rendered
        }
        public void Add(IRenderable item) => Objects.Add(item);
        public void Remove(IRenderable item) => Objects.Remove(item);
        public void Add(ILightObject item) => LightObjects.Add(item);
        public void Remove(ILightObject item) => LightObjects.Remove(item);

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
                
                RefreshCol = Color.FromArgb(0, 0, 0, 0);

            }

            public override void Use()
            {
                base.Use();
                GL.Enable(EnableCap.DepthTest);
                GL.Disable(EnableCap.Blend);
                GL.DepthMask(true);
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
                GL.Enable(EnableCap.Blend);
                GL.BlendEquation(BlendEquationMode.FuncAdd);
                GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
                base.Use();
                
                GL.DepthMask(false);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.StencilBufferBit);
            }
        }
    }
}


