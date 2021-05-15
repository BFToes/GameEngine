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
            Scene = new Scene(
                $"Resources/shaderscripts/PostProcess/PostProcess.vert",
                $"Resources/shaderscripts/PostProcess/PostProcess.frag", 
                Size.X, Size.Y);
            Process = (delta) => Time += delta;
            

            #region OpenGL Functions and window parameters
            VSync = VSyncMode.On;
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            //GL.Enable(EnableCap.Blend);
            #endregion
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            Size = e.Size;
            Scene.Size = e.Size;
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Scene.Process();
            Process((float)e.Time);
            //Title = $"{MathF.Round(1 / (float)e.Time)}";

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.ClearColor(Color.DarkRed);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // use default
            GL.CullFace(CullFaceMode.Back);
            GL.DepthFunc(DepthFunction.Less);
            GL.BlendFunc(BlendingFactor.Src1Alpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Viewport(0, 0, Size.X, Size.Y);
            
            Scene.Render();

            SwapBuffers(); // swap out old buffer with new buffer
        }
    }
}

