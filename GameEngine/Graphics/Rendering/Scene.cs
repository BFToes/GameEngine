using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using Graphics.Shaders;
using System.Drawing;
using OpenTK.Mathematics;

using Graphics.SceneObject;
using Graphics.Resources;

namespace Graphics.Rendering
{
    /* Volume Lighting
     * https://ogldev.org/www/tutorial36/tutorial36.html
     * ^
     * newer OpenGL but more complicated and less explicit on what to do. still better than some.
     * 
     * Stencil shadows
     * https://ogldev.org/www/tutorial40/tutorial40.html - maybe actually read it tho
     * https://www.angelfire.com/games5/duktroa/RealTimeShadowTutorial.htm
     * https://nehe.gamedev.net/tutorial/shadows/16010/
     * https://www.gamedev.net/articles/programming/graphics/the-theory-of-stencil-shadow-volumes-r1873/
     * 
     * IDISPOSABLE YOU PIECE OF UTTER SHIT..
     * PBR materials -> needs lighting
     * Learn about attribute coding
     * 
     */

    /* Uniformblocks:
         * 0 -> Camera
         * 1 -> Light
         * ...
         */

    class Scene
    {
        public Camera Camera;

        public ShaderProgram LightProgram; // doesnt make sense for this to be here but it needs geometry textures ¯\_(ツ)_/¯
        public ShaderProgram DebugProgram; // should delete later

        private GeometryBuffer GBuffer;
        private SceneBuffer SBuffer; // could maybe be obselete I might be able to render light to geometry aswell or just into default

        // mesh that encompasses the entire screen
        public static Mesh<Simple2D> ScreenMesh = Mesh<Simple2D>.From<Simple2D>(new float[8] { -1, -1, 1, -1, 1, 1, -1, 1 }, PrimitiveType.TriangleFan);

        public List<ILightObject> LightObjects = new List<ILightObject>();
        public List<IRenderable> Objects = new List<IRenderable>();
        public List<UniformBlock> UniformBlocks = new List<UniformBlock>(); // probably not the best way for this to work

        private Vector2i size;
        public Vector2i Size
        {
            get => size;
            set
            {
                size = value;
                Camera.Resize(size);
                GBuffer.Size = size;
                SBuffer.Size = size;
            }
        }

        public Scene(int Width, int Height)
        {
            size = new Vector2i(Width, Height);

            GBuffer = new GeometryBuffer(Width, Height);
            SBuffer = new SceneBuffer(Width, Height);

            // setup camera
            Camera = new Camera(50, Width, Height, 2, 512);
            
            // setup Light program
            LightProgram = ShaderProgram.ReadFrom("Resources/Shaderscripts/PostProcess/Light.vert", "Resources/Shaderscripts/PostProcess/Light.frag");
            LightProgram.SetUniformSampler2D("AlbedoTexture", GBuffer.AlbedoTexture);
            LightProgram.SetUniformSampler2D("NormalTexture", GBuffer.NormalTexture);
            LightProgram.SetUniformSampler2D("PositionTexture", GBuffer.PositionTexture);

            LightProgram.SetUniform("SpecularIntensity", 1f);
            LightProgram.SetUniform("SpecularPower", 1f);

            LightProgram.SetUniformBlock("CameraBlock", 0);
            LightProgram.SetUniformBlock("LightBlock", 1);

            

            LightProgram.DebugUniforms();

            LightObjects.Add(new PointLight(new Vector3(0, 29, 5), new Vector3(12, 0, 12)));

            #region Debug Stuff

            // setup Debug program
            DebugProgram = ShaderProgram.ReadFrom("Resources/Shaderscripts/PostProcess/GeomDebug.vert", "Resources/Shaderscripts/PostProcess/GeomDebug.frag");
            DebugProgram.SetUniformSampler2D("ColourTexture", GBuffer.AlbedoTexture);
            DebugProgram.SetUniformSampler2D("NormalTexture", GBuffer.NormalTexture);
            DebugProgram.SetUniformSampler2D("PositionTexture", GBuffer.PositionTexture);
            DebugProgram.SetUniformSampler2D("ShadedTexture", SBuffer.ColourTexture);
            DebugProgram.DebugUniforms();
            #endregion

            
        }

        /// <summary>
        /// renders objects inside this viewport and updates the viewport textures
        /// </summary>
        public void Render()
        {
            /*
               1. Render the objects as usual into the G buffer so that the depth buffer will be properly populated.
               2. Disable writing into the depth buffer. From now on we want it to be read-only.
               3. Disable back face culling. We want the rasterizer to process all polygons of the sphere.
               4. Set the stencil test to always succeed. What we really care about is the stencil operation.
               5. Configure the stencil operation for the back facing polygons to increment the value in the stencil buffer when the depth test fails but to keep it unchanged when either depth test or stencil test succeed.
               6. Configure the stencil operation for the front facing polygons to decrement the value in the stencil buffer when the depth test fails but to keep it unchanged when either depth test or stencil test succeed.
               7. Render the light sphere.
             */
            Camera.Block.Bind();

            // render objects into Geometry buffer
            GBuffer.Use();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            foreach (IRenderable RO in Objects) RO.Render();

            
            
            SBuffer.Use();
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // point light pass
            LightProgram.Use();
            foreach (ILightObject LO in LightObjects) LO.Render();

            #region Draw to screen
            GL.Disable(EnableCap.Blend);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.ClearColor(Color.DarkRed);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            DebugProgram.Use();
            ScreenMesh.Render();
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

                // this frame buffer draws to multiple textures at once
                GL.DrawBuffers(3, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2 });

                FramebufferErrorCode FrameStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
                if (FrameStatus != FramebufferErrorCode.FramebufferComplete) throw new Exception(FrameStatus.ToString());

                RefreshCol = new Color4(1, 0, 1, 1);
            }

            public override void Use()
            {           
                base.Use();
                GL.Enable(EnableCap.DepthTest); // only geometry pass updates depth buffer
                GL.DepthMask(true);
                GL.Enable(EnableCap.CullFace);
                GL.Disable(EnableCap.Blend);
                
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

                GL.DrawBuffers(1, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0 });

                FramebufferErrorCode FrameStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
                if (FrameStatus != FramebufferErrorCode.FramebufferComplete) throw new Exception(FrameStatus.ToString());

                RefreshCol = Color.FromArgb(0, 1, 0, 0);

            }
            public override void Use()
            {
                // after geometry buffer the depth buffer is already populated and the stencil pass
                // depends on it, but it does not write to it.
                GL.Disable(EnableCap.DepthTest);
                GL.DepthMask(false);

                base.Use();
                

                // So light adds together
                GL.Enable(EnableCap.Blend);
                GL.BlendEquation(BlendEquationMode.FuncAdd);
                GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);

                // so light shadows work
                GL.Enable(EnableCap.StencilTest);

            }
        }
        #endregion
    }
}


