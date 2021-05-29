using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using Graphics.Shaders;
using System.Drawing;
using OpenTK.Mathematics;
using Graphics.SceneObjects;
using Graphics.Resources;
using Graphics.Rendering;
namespace Graphics.SceneObjects
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
     * 
     * TILED LIGHTING  ->       1. Geometry pass
     *                          2. construct screenspace grid with fixed pixel/work group size
     *                          3. find out the min/max depth of each tile.
     *                          4. find which lights affect this tile by constructing a per tile frustrum
     *                          5. switch over to calculate all lights that affect this tile
     * 
     * FRUSTRUM CULLING ->      A frustrum cull removes the object that are outside the view frustrum from 
     *                          being rendered just a good thing to have. I can also add an axis aligned 
     *                          bounding box around a mesh to optimise search.
    
     * LIGHT OCTATREE ->        currently If I add 300+ lights ontop of each other, they all render seperately 
     *                          which is slowing down the program alot. I can instead group lights by distance
     *                          and render it as One shader pass but with different parameters so its just as 
     *                          bright. This could also be used to speed frustrum culling search time.
     *   
     * NEED TO SORT OUT OBJECT MANAGEMENT AND RELATIVE TRANSFORMS
     * 
     * IDISPOSABLE YOU PIECE OF UTTER SHIT.. 
     */

    /* Uniformblocks:
         * 0 -> Camera
         * 1 -> Light
         * ...
         */
    class Scene
    {
        public Camera Camera;
        public ShaderProgram PostProcessProgram = ShaderProgram.ReadFrom(
            "Resources/Shaderscripts/Rendering/GeomDebug.vert", 
            "Resources/Shaderscripts/Rendering/GeomDebug.frag");

        private GeometryBuffer GBuffer;

        private List<Occluder> OccluderObjects = new List<Occluder>();
        private List<IRenderable> Objects = new List<IRenderable>();
        private List<Light> PointLightObjects = new List<Light>();

        private Vector2i size;
        public Vector2i Size
        {
            get => size;
            set
            {
                size = value;
                
                GBuffer.Size = size;
                Camera.Resize(size);
            }
        }

        public Action<float> Process = (delta) => { };

        public Scene(int Width, int Height)
        {
            size = new Vector2i(Width, Height);
            GBuffer = new GeometryBuffer(Width, Height);
            Camera = new Camera(50, Width, Height, 0.1f, 512);
            PostProcessProgram.SetUniformSampler2D("ColourTexture", GBuffer.AlbedoTexture);
            PostProcessProgram.SetUniformSampler2D("NormalTexture", GBuffer.NormalTexture);
            PostProcessProgram.SetUniformSampler2D("PositionTexture", GBuffer.PositionTexture);
        }

        /// <summary>
        /// renders objects inside this viewport and updates the viewport textures
        /// </summary>
        public void Render(int DrawTarget = 0)
        {
            GBuffer.Use();

            foreach (IRenderable RO in Objects) 
                RO.Render();

            BeginLightPass(DrawTarget);

            foreach (Light LO in PointLightObjects)
            {
                LO.UseLight();

                foreach (Occluder Occ in OccluderObjects)
                    Occ.Occlude(LO);
               
                LO.Illuminate();
            }
        }

        public void BeginLightPass(int DrawTarget)
        {
            GL.Enable(EnableCap.StencilTest);
            GL.Disable(EnableCap.CullFace);

            // copies depth to render context
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, GBuffer);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, DrawTarget);
            GL.BlitFramebuffer(0, 0, Size.X, Size.Y, 0, 0, Size.X, Size.Y, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
            
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, DrawTarget);
            GL.ClearColor(0, 0, 0, 0); GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        public void Use()
        {
            Camera.Block.Bind();
            Light.AlbedoTexture = GBuffer.AlbedoTexture;
            Light.NormalTexture = GBuffer.NormalTexture;
            Light.PositionTexture = GBuffer.PositionTexture;
            Light.SpecularIntensity = 0.4f;
            Light.SpecularPower = 1;

            // blending functions
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);

            // stencil functions
            GL.StencilFunc(StencilFunction.Always, 0, 0xff);
            GL.StencilOpSeparate(StencilFace.Front, StencilOp.Keep, StencilOp.DecrWrap, StencilOp.Keep);
            GL.StencilOpSeparate(StencilFace.Back, StencilOp.Keep, StencilOp.IncrWrap, StencilOp.Keep);

            // cullface functions
            GL.CullFace(CullFaceMode.Back);
        }

        #region Object Management
        // currently really stupid
        public void Add(IRenderable item) => Objects.Add(item);
        public void Remove(IRenderable item) => Objects.Remove(item);
        public void Add(Light item) => PointLightObjects.Add(item);
        public void Remove(Light item) => PointLightObjects.Remove(item);
        public void Add(Occluder item) => OccluderObjects.Add(item);
        public void Remove(Occluder item) => OccluderObjects.Remove(item);
        #endregion

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

                RefreshColour = new Color4(0, 0, 0, 0);
            }

            public override void Use()
            {
                base.Use();
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.DepthMask(true);
                GL.Disable(EnableCap.StencilTest);
                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Back);
            }
        }
    }
}


