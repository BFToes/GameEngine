using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using Graphics.Shaders;
using System.Drawing;
using OpenTK.Mathematics;
using Graphics.SceneObjects;
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
     * 
     * TILED LIGHTING  ->       1. Geometry pass
     *                          2. construct screenspace grid with fixed pixel/work group size
     *                          3. find out the min/max depth of each tile.
     *                          4. find which lights affect this tile by constructing a per tile frustrum
     *                          5. switch over to calculate all lights that affect this tile
     * On second thoughts maybe not what i want
     * I quite like the sphere mesh approuch so i dont want to rewrite that. might be necessary tho
     * 
     * FRUSTRUM CULLING ->      A frustrum cull removes the object that are outside the view frustrum from 
     *                          being rendered just a good thing to have. I can also add a bounding box around 
     *                          a mesh to optimise search.
     * LIGHT OCTATREE ->        currently If I add 300+ lights ontop of each other, they all render seperately 
     *                          which is slowing down the program alot. I can instead group lights by distance
     *                          and render it as One shader pass but with different parameters so its just as 
     *                          bright. This could also be used to speed frustrum culling search time.
     *   
     * NEED TO SORT OUT OBJECT MANAGEMENT
     * NEED TO SORT OUT RELATIVE TRANSFORMS
     * 
     * 
     * 
     * IDISPOSABLE YOU PIECE OF UTTER SHIT..
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

        public ShaderProgram PostProcess = ShaderProgram.ReadFrom("Resources/Shaderscripts/Rendering/GeomDebug.vert", "Resources/Shaderscripts/Rendering/GeomDebug.frag");

        private GeometryBuffer GBuffer;
        private SceneBuffer SBuffer; // could maybe be obselete I might be able to render light to geometry aswell or just into default

        private List<Light> LightObjects = new List<Light>();
        private List<Occluder> OccluderObjects = new List<Occluder>();
        private List<IRenderable> Objects = new List<IRenderable>();

        private Vector2i size;

        public Vector2i Size
        {
            get => size;
            set
            {
                size = value;
                
                GBuffer.Size = size;
                SBuffer.Size = size;
                Camera.Resize(size);
            }
        }

        public Scene(int Width, int Height)
        {
            size = new Vector2i(Width, Height);

            GBuffer = new GeometryBuffer(Width, Height);
            SBuffer = new SceneBuffer(Width, Height);

            // setup camera
            Camera = new Camera(50, Width, Height, 2, 512);
                   
        }

        /// <summary>
        /// renders objects inside this viewport and updates the viewport textures
        /// </summary>
        public void Render()
        {
            Camera.Block.Bind();
            
            // Geometry pass
            GBuffer.Use();
            GL.DepthMask(true);
            GL.Disable(EnableCap.StencilTest);
            GL.Enable(EnableCap.DepthTest);
            
            foreach (IRenderable RO in Objects) RO.Render();
            
            // Light pass
            SBuffer.Use();
            foreach (Light LO in LightObjects)
            {
                /* bind LightBlock
                 * Use ShadowProgram
                 * 
                 * foreach Occluder:
                 *      Render Occluder
                 * 
                 * Render Light
                 */
                LO.Illuminate();
            }
            
            #region Draw to screen
            GL.Disable(EnableCap.Blend);
            GL.CullFace(CullFaceMode.Back);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.ClearColor(Color.Crimson);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            // render into screen
            PostProcess.Use();
            Mesh.Screen.Render();
            #endregion

        }

        public void Use()
        {
            // means its more awkward to use more than 1 scene at a time
            
            PostProcess.SetUniformSampler2D("ColourTexture", GBuffer.AlbedoTexture);
            PostProcess.SetUniformSampler2D("NormalTexture", GBuffer.NormalTexture);
            PostProcess.SetUniformSampler2D("PositionTexture", GBuffer.PositionTexture);
            PostProcess.SetUniformSampler2D("ShadedTexture", SBuffer.ColourTexture);

            Light.PntLightProg.SetUniformSampler2D("AlbedoTexture", GBuffer.AlbedoTexture);
            Light.PntLightProg.SetUniformSampler2D("NormalTexture", GBuffer.NormalTexture);
            Light.PntLightProg.SetUniformSampler2D("PositionTexture", GBuffer.PositionTexture);
        }

        #region Object Management
        // currently really stupid
        public void Add(IRenderable item) => Objects.Add(item);
        public void Remove(IRenderable item) => Objects.Remove(item);
        public void Add(Light item) => LightObjects.Add(item);
        public void Remove(Light item) => LightObjects.Remove(item);
        public void Add(Occluder item) => OccluderObjects.Add(item);
        public void Remove(Occluder item) => OccluderObjects.Remove(item);
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

                RefreshCol = new Color4(0, 0, 0, 0);
            }

            public override void Use() 
            {
                base.Use();
                //GL.Enable(EnableCap.CullFace);
                GL.DepthMask(true);
                GL.Enable(EnableCap.DepthTest);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            }
        }
        private class SceneBuffer : FrameBuffer
        {
            public readonly int ColourTexture; // colour texture

            public SceneBuffer(int Width, int Height) : base(Width, Height)
            {
                // geometry-buffer textures
                ColourTexture = NewTextureAttachment(FramebufferAttachment.ColorAttachment0, Width, Height);
                GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

                FramebufferErrorCode FrameStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
                if (FrameStatus != FramebufferErrorCode.FramebufferComplete) throw new Exception(FrameStatus.ToString());

                RefreshCol = Color.FromArgb(0, 0, 0, 0);

            }
            public override void Use()
            {
                base.Use();
                // So light adds together
                GL.Enable(EnableCap.Blend);
                GL.BlendEquation(BlendEquationMode.FuncAdd);
                GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);

                // so light shadows work
                GL.DepthMask(false);
                GL.Disable(EnableCap.DepthTest);
                GL.Enable(EnableCap.StencilTest);

                GL.CullFace(CullFaceMode.Front);

                GL.Clear(ClearBufferMask.ColorBufferBit);
            }
        }
        #endregion


    }
}


