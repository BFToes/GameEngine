using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using System;
namespace GameEngine.Rendering
{
    [Obsolete]
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

        public static RenderWindow New(bool Vsync = true, int SizeX = 800, int SizeY = 800, string Title = "Title")
        {
            GameWindowSettings GWS = GameWindowSettings.Default;
            NativeWindowSettings NWS = NativeWindowSettings.Default;

            NWS.StartFocused = true;
            NWS.Size = new Vector2i(SizeX, SizeY);
            NWS.Title = Title;

            //GWS.IsMultiThreaded = true; // idk if this will work automatically or not
            //GWS.UpdateFrequency = 0.1;
            return new RenderWindow(GWS, NWS);
        }

        private RenderWindow(GameWindowSettings GWS, NativeWindowSettings NWS) : base(GWS, NWS) { }

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

