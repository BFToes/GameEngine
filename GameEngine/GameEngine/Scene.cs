using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using GameEngine.Entities;
using GameEngine.Entities.Lighting;
using GameEngine.Rendering;
namespace GameEngine
{
    /* FIX ENTITY CLASSES ->    They should only be doing one thing. like a transform object, then the
     *                          other classes should extend off them. I CAN USE INTERNAL CLASSES BABY:
     *                          Split functionality into entity and component:
     *                           * Transform Component
     *                           * Mesh Component
     *                           * Occluder Component
     *                           * Render Component
     *                           * ScreenSpace Refection
     *                           * Particle System
     *                           * Culling Component
     *                           * Collider Component
     *                           * Animation Component
     * Entity:
     *    Entity Parent 
     *    List<Entity> Children
     *    List<EntityComponent> components
     *    Component GetComponent<Component>()
     *    Render()
     *    Update()
     * EntityComponent:
     *    Entity entity
     *    bool Renderable
     *    bool Updatable
     *    void Update(float)
     *    void Render()
     *  
     * REPLACE MESH WITH ASSIMP LIBRARY
     * REPLACE SHADERPROGRAM WITH ASSIMP LIBRARY
     *  
     *  
     *  
     * 
     * ### MESHES:
     * MESH SKELETAL ANIM ->    like squish w armatures an stuff? ASIMP is a word? opgdev has a tutorial
     * MESH NORMALIZATION ->    for bounding box on frustrum culling
     * MESH SIMPLIFICATION ->   for occluder objects. edge colapse algorithm. or just resample?
     * MESH MANAGEMENT ->       like with sampler2Ds and it would be good to like idk merge that kinda resource 
     *                          management. hard to know if this is a good idea.
     * MESH MATERIAL IMPORT ->  idk there were like funky lil .mtl files ¯\_(ツ)_/¯
     *
     * REDO SHADERPROGRAM ->    currently the sampler units dont pack very wells
     *
     *
     * ### LIGHTS:
     * SSAO ->                  screen space ambient occlusion. yh fuck it why not.
     * 
     * 
     * ### OPITIMISATIONS:
     * FRUSTRUM CULLING ->      A frustrum cull removes the object that are outside the view frustrum from 
     *                          being rendered. just a good thing to have. This applies to lights and camera
     *                          I can also add an axis aligned bounding box around a mesh to optimise search.
     *                          Starting to become very necessary.   
     *                           *  Volume Bounding Trees -> a tree for bounding volumes. 
     *                           *  https://cesium.com/blog/2015/08/04/fast-hierarchical-culling/ ->
     *                              when traversing the culling tree, optimisations can be made so that the 
     *                              children nodes only check the planes that intersected the parent node. 
     *                              bitmask for each plane on frustum observer
     *
     * OBJECT CHUNK SYSTEM ->   Octa tree of objects for faster frustrum culling and tesselation for far away
     *                          objects. can also use it for transparency and depth sorting. more importantly
     *                          it can be used for light "Frustrum" culling(it will almost never be a frustrum
     *                          more likely its gonna be a cube)
     * ENTITY UN/LOADING ->     This is the idisposable problem. you're lazy. this also leads to serialization. 
     *                          we all know serialization is disusting. this should probably done after chunk 
     *                          octatree system.
     * Tesselation ->           Linked to chunk system. low res for far away things an stuff. Im stuff haha.
     *                          
     * ### OTHER:                         
     * MANAGE OBJECT SCOPE ->   alot of classes are public that dont need to be, I havent got a good code 
     *                          interface. I want to be able to use the name space add objects and thats all 
     *                          you can see.  
     * MULTI-THREADING ->       I have no clue how to implement this in a graphics environment. Apparently it
     *                          works with OpenGL. like how does that even work for a gpu.
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
        private readonly GeometryBuffer GBuffer;

        private readonly List<Occluder> OccluderEntities = new List<Occluder>(); // ICullable
        private readonly List<IRenderable> RenderEntities = new List<IRenderable>(); // Potenitally ICullable
        private readonly List<IVolumeLight> VLightEntities = new List<IVolumeLight>(); // Potenitally ICullible

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
            Camera = new Camera(50, Width, Height, 0.125f, 512);
        }

        /// <summary>
        /// renders objects inside this viewport and updates the viewport textures
        /// </summary>
        public void Render(int DrawTarget = 0)
        {
            GBuffer.Use();

            foreach (IRenderable RO in RenderEntities)
                if (Camera.Detects(RO)) RO.Render();
                    
            BeginLightPass(DrawTarget);
            
            foreach (IVolumeLight LO in VLightEntities)
            {
                if (Camera.Detects(LO))
                {
                    LO.UseLight();

                    foreach (IOccluder Occ in OccluderEntities)
                        if (LO.Detects(Occ)) Occ.Occlude(LO);

                    LO.Illuminate();
                }
            }
        }
        /// <summary>
        /// Sets up openGL to use light objects
        /// </summary>
        /// <param name="DrawTarget"></param>
        private void BeginLightPass(int DrawTarget)
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
        /// <summary>
        /// setup openGL to use this scenes functions
        /// </summary>
        public void Use()
        {
            Camera.Use();
            IVolumeLight.AlbedoTexture = GBuffer.AlbedoTexture;
            IVolumeLight.NormalTexture = GBuffer.NormalTexture;
            IVolumeLight.PositionTexture = GBuffer.PositionTexture;
            IVolumeLight.SpecularIntensity = 0.1f;
            IVolumeLight.SpecularPower = 4;

            // blending functions
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);

            // stencil functions
            GL.StencilFunc(StencilFunction.Always, 0, 0xff);
            GL.StencilOpSeparate(StencilFace.Front, StencilOp.Keep, StencilOp.DecrWrap, StencilOp.Keep);
            GL.StencilOpSeparate(StencilFace.Back, StencilOp.Keep, StencilOp.IncrWrap, StencilOp.Keep);

            // cullface functions
            GL.CullFace(CullFaceMode.Back);

            // polygon fix
            GL.PolygonOffset(0.1f, 1f);
        }
        /// <summary>
        /// adds an entity and all its children into the scene
        /// </summary>
        public void Add(Entity Entity)
        {
            switch (Entity)
            {
                case IVolumeLight LO: 
                    VLightEntities.Add(LO); 
                    foreach (Entity E in Entity.GetChildren()) 
                        Add(E); 
                    break;
                case IRenderable RO: 
                    RenderEntities.Add(RO); 
                    foreach (Entity E in Entity.GetChildren()) 
                        Add(E); 
                    break;
                case Occluder Occ: 
                    OccluderEntities.Add(Occ); 
                    foreach (Entity E in Entity.GetChildren()) 
                        Add(E); 
                    break;
                default: throw new Exception($"Unrecognised Entity:{Entity}");
            }
        }
        /// <summary>
        /// Removes this entity and all its children from the scene
        /// </summary>
        public void Remove(Entity Entity)
        {
            switch (Entity)
            {
                case IVolumeLight LO: 
                    VLightEntities.Remove(LO); 
                    foreach (Entity Child in Entity.GetChildren()) 
                        Remove(Child); 
                    break;
                case IRenderable RO: 
                    RenderEntities.Remove(RO); 
                    foreach (Entity Child in Entity.GetChildren()) 
                        Remove(Child); 
                    break;
                case Occluder Occ: 
                    OccluderEntities.Remove(Occ); 
                    foreach (Entity Child in Entity.GetChildren()) 
                        Remove(Child); 
                    break;
                default: throw new Exception($"Unrecognised Entity: {Entity}");
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

                RefreshColour = new Color4(0, 0, 0, 0);
            }

            public override void Use()
            {
                // prepares openGL for rendering objects instead of lighting
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


