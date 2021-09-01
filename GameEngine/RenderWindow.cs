using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using ECS;

namespace GameEngine
{
    sealed class RenderWindow : GameWindow
    {
        private Scene _scene;
        public Scene scene
        {
            get => _scene;
            set
            {
                value.size = this.Size;
                _scene = value;
            }
        }


        public static RenderWindow New(Scene scene = null, bool Vsync = true, int SizeX = 800, int SizeY = 800, string Title = "Title")
        {
            GameWindowSettings GWS = GameWindowSettings.Default;
            NativeWindowSettings NWS = NativeWindowSettings.Default;

            NWS.Size = new Vector2i(SizeX, SizeY);
            NWS.Title = Title;

            return new RenderWindow(GWS, NWS, scene);
        }
        private RenderWindow(GameWindowSettings GWS, NativeWindowSettings NWS, Scene scene) : base(GWS, NWS) 
        {
            this.scene = scene ?? new Scene(NWS.Size.X, NWS.Size.Y);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            Size = e.Size;
            scene.size = e.Size;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            scene.Process((float)e.Time);

            scene.Render(0);

            SwapBuffers();
        }
    }
}
