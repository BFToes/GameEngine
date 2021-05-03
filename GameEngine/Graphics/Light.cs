using System;
using System.Collections.Generic;
using System.Text;
using Graphics.Shaders;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Graphics
{
    class Light : FrameBufferObject, IRenderLight
    {
        Scene Scene;
        ShaderProgram Material;
        Camera LightPerspective;
        public RenderPriority RenderType { get => RenderPriority.Lighting; }
        public bool Visible { get; set; }
        private bool visible;

        private int Dep;

        public Light(Scene Scene) : base(128, 128)
        {
            this.Scene = Scene;

            Set_Visible = (value) =>
            {
                visible = value;
                if (visible) Scene.Add(this);
                else Scene.Remove(this);
            };
            Visible = true;

            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            Dep = NewTextureAttachment(PixelInternalFormat.DepthComponent24, PixelFormat.DepthComponent, PixelType.Float, FramebufferAttachment.DepthStencilAttachment, Size.X, Size.Y);

            Material = new ShaderProgram(
                $"Resources/shaderscripts/Light/Light.vert",
                $"Resources/shaderscripts/Light/Light.frag");


        }

        public event Action<int> Set_Z_Index;
        public event Action<bool> Set_Visible;

        public void OnRender()
        {
            GL.CullFace(CullFace);
            GL.DepthFunc(DepthFunc);

            // use this viewport
            GL.Viewport(0, 0, Size.X, Size.Y);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, FBO);


            // clear this viewport frame buffer
            GL.ClearColor(RefreshCol);
            GL.Clear(ClearBufferMask.DepthBufferBit);


            Material.Use();

            Camera TempCam = Scene.Camera;
            Scene.Camera = LightPerspective;
            foreach (IRenderObject RO in Scene.Objects) RO.LightRender();



        }
    }
}
