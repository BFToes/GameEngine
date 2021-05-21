using System;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using System.Drawing;
namespace Graphics
{
    class RenderWindow : GameWindow
    {
        public Scene Scene;
        public Action<float> Process;
        public float Time;
        public RenderWindow(GameWindowSettings GWS, NativeWindowSettings NWS) : base(GWS, NWS)
        {
            Scene = new Scene(Size.X, Size.Y);
            Process = (delta) => Time += delta;

            VSync = VSyncMode.On;
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.DepthFunc(DepthFunction.Less);
            //GL.Enable(EnableCap.Blend);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            Size = e.Size;
            Scene.Size = e.Size;
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Process((float)e.Time);

            Scene.Render();

            SwapBuffers(); // swap out old buffer with new buffer
        }
    }
}

