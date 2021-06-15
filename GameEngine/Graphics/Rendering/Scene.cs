using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using Graphics.Entities;
namespace Graphics.Rendering
{
    /* FRUSTRUM CULLING ->      A frustrum cull removes the object that are outside the view frustrum from 
     *                          being rendered. just a good thing to have. This applies to lights and camera
     *                          I can also add an axis aligned bounding box around a mesh to optimise search.
    
     * LIGHT OCTATREE CHUNK ->  Currently If I add 300+ lights ontop of each other, they all render seperately 
     *                          which is slowing down the program alot. I can instead group lights by distance
     *                          and render it as One shader pass but with different parameters so its just as 
     *                          bright. This could also be used to speed frustrum culling search time. 
     *                          
     *                          This wont work for shadows unless I do soft shadows in a double pass with big 
     *                          circle and little circle.
     *                          
     * ENTITY UN/LOADING ->     This is the idisposable problem. your lazy. this also leads to serialization. 
     *                          we all know serialization is disusting. this should probably done after chunk 
     *                          octatree system.
     *                          
     * SHADOW PROBLEM ->        Fix with tesselation??? i could also increase epsilon with distance away from 
     *                          camera. the epsilon only needs to be small when the camera is nearby altern-
     *                          atively i could change the matrix order and use eplsilon after transform. this
     *                          would be more efficient but harder to understand.
     *                          
     * SSAO ->                  screen space ambient occlusion.
     * 
     * FXAA ->                  Antialiasing. TSAA??? whatever the fuck that is.
     *                          
     * MANAGE OBJECT SCOPE ->   alot of classes are public that dont need to be, I havent got a good code 
     *                          interface. I want to be able to use the name space add objects and thats all 
     *                          you can see. 
     * 
     * MULTI-THREADING ->       I have no clue how to implement this in a graphics environment. Apparently it
     *                          works with OpenGL. 
     * 
     * IDISPOSABLE ->           YOU PIECE OF UTTER SHIT.. 
     * 
     * EMPTY DELEGATES ->       slight performance issue as each time its called it must call the empty instruction
     *                          really isnt an issue.
     */

    /* Uniformblocks:
         * 0 -> Camera
         * 1 -> Light
         * ...
         */
    class Scene
    {
        public Camera Camera;
        
        private GeometryBuffer GBuffer;

        private List<Occluder> OccluderObjects = new List<Occluder>();
        private List<IRenderable> Objects = new List<IRenderable>();
        private List<Light> LightObjects = new List<Light>();

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

        public Action<float> Process = delegate { };

        public Scene(int Width, int Height)
        {
            size = new Vector2i(Width, Height);
            GBuffer = new GeometryBuffer(Width, Height);
            Camera = new Camera(50, Width, Height, 0.1f, 512);
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

            foreach (Light LO in LightObjects)
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

            // copies depth to draw target
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, GBuffer);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, DrawTarget);
            GL.BlitFramebuffer(0, 0, Size.X, Size.Y, 0, 0, Size.X, Size.Y, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
            
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, DrawTarget);
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        public void Use()
        {
            Camera.Block.Bind();
            Light.AlbedoTexture = GBuffer.AlbedoTexture;
            Light.NormalTexture = GBuffer.NormalTexture;
            Light.PositionTexture = GBuffer.PositionTexture;
            Light.SpecularIntensity = 1.5f;
            Light.SpecularPower = 5;

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

        public void Add(Entity Entity)
        {
            switch (Entity)
            {
                case Light LO: 
                    LightObjects.Add(LO); 
                    foreach (Entity E in Entity.GetChildren()) 
                        Add(E); 
                    break;
                case IRenderable RO: 
                    Objects.Add(RO); 
                    foreach (Entity E in Entity.GetChildren()) 
                        Add(E); 
                    break;
                case Occluder Occ: 
                    OccluderObjects.Add(Occ); 
                    foreach (Entity E in Entity.GetChildren()) 
                        Add(E); 
                    break;
                default: throw new Exception($"Unrecognised Entity:{Entity}");
            }

        }
        public void Remove(Entity Entity)
        {
            switch (Entity)
            {
                case Light LO: 
                    LightObjects.Remove(LO); 
                    foreach (Entity E in Entity.GetChildren()) 
                        Remove(E); 
                    break;
                case IRenderable RO: 
                    Objects.Remove(RO); 
                    foreach (Entity E in Entity.GetChildren()) 
                        Remove(E); 
                    break;
                case Occluder Occ: 
                    OccluderObjects.Remove(Occ); 
                    foreach (Entity E in Entity.GetChildren()) 
                        Remove(E); 
                    break;
                default: throw new Exception($"Unrecognised Entity:{Entity}");
            }
        }

        private class GeometryBuffer : FrameBuffer
        {
            public readonly int AlbedoTexture;   // colour texture
            public readonly int NormalTexture;   // normal texture
            public readonly int PositionTexture; // position texture
            public readonly int DepthBuffer;     // depth buffer
            public GeometryBuffer(int Width, int Height): base(Width, Height)
            {
                // geometry-buffer textures
                PositionTexture = NewTextureAttachment(FramebufferAttachment.ColorAttachment0, Width, Height);
                NormalTexture = NewTextureAttachment(FramebufferAttachment.ColorAttachment1, Width, Height);
                AlbedoTexture = NewTextureAttachment(FramebufferAttachment.ColorAttachment2, Width, Height);
                DepthBuffer = NewRenderBufferAttachment(RenderbufferStorage.DepthComponent24, FramebufferAttachment.DepthAttachment, Width, Height);

                // this frame buffer draws to multiple textures at once
                GL.DrawBuffers(3, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2 });

                RefreshColour = Color4.Blue;//new Color4(0, 0, 0, 0);
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


