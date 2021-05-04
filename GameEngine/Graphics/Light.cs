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
        Camera LightPerspective;
        public RenderPriority RenderType { get => RenderPriority.Lighting; }
        public bool Visible { get; set; }
        private bool visible;

        private int Dep;

        public Light(Scene Scene) : base(128, 128)
        {

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
        }

        public event Action<bool> Set_Visible;

        public void Render()
        {
        }
    }
}
