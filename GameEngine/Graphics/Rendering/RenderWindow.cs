using System;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using System.Drawing;
using Graphics.Entities;
namespace Graphics.Rendering
{
    class RenderWindow : GameWindow
    {
        private Scene scene;
        public Scene Scene 
        {
            get => scene;
            set
            {
                scene = value;
                scene.Use();
            }
            
        }
        public RenderWindow(GameWindowSettings GWS, NativeWindowSettings NWS) : base(GWS, NWS)
        {
            VSync = VSyncMode.On;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            Size = e.Size;
            Scene.Size = e.Size;
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Scene.Process((float)e.Time);

            Scene.Render();

            SwapBuffers(); 
        }
    }
}

