using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

using Graphics;
using Graphics.Resources;
using Graphics.Rendering;
using Graphics.SceneObjects;
namespace GameEngine
{
    class Program
    {


        static void Main(string[] args)
        {
            GameWindowSettings GWS = GameWindowSettings.Default;
            NativeWindowSettings NWS = NativeWindowSettings.Default;
            NWS.Size = new Vector2i(800);
            
            using (RenderWindow RW = new RenderWindow(GWS, NWS))
            {
                Scene Scene = new World();
                RW.Scene = Scene;

                Action<MouseMoveEventArgs> MoveCamera = (e) => RW.Scene.Camera.Position += 10 * new Vector3(RW.Scene.Camera.Matrix * -new Vector4(-e.DeltaX / RW.Size.X, e.DeltaY / RW.Size.Y, 0, 1));
                Action<MouseMoveEventArgs> RotaCamera = (e) => RW.Scene.Camera.Rotation += new Vector3(e.DeltaY / RW.Size.Y, e.DeltaX / RW.Size.X, 0);

                RW.MouseWheel += (e) => RW.Scene.Camera.Position += new Vector3(RW.Scene.Camera.Matrix * -new Vector4(0, 0, e.OffsetY, 1));
                RW.MouseDown += (e) =>
                {
                    switch (e.Button)
                    {
                        case MouseButton.Button1:
                            RW.MouseMove += MoveCamera;
                            break;
                        case MouseButton.Button2:
                            RW.MouseMove += RotaCamera;
                            break;
                    }
                };
                RW.MouseUp += (e) =>
                {
                    switch (e.Button)
                    {
                        case MouseButton.Button1:
                            RW.MouseMove -= MoveCamera;
                            break;
                        case MouseButton.Button2:
                            RW.MouseMove -= RotaCamera;
                            break;
                    }
                };

                RW.Run();
            }
        }
    }
    class World : Scene
    {
        private float Time;

        public World() : base(800, 800)
        {
            Camera.Position = new Vector3(0, 2, 3);

            var RO1 = new Test(this, new Vector3(2, 3, 0));
            var RO2 = new Floor(this);
            var RL1 = new TestLight_Point(this, 0, 5, 5, 1, 1, 1);
            var RL2 = new TestLight_Direction(this, -1, -1, 1, 0.5f, 0.5f, 0.5f);

            Process += (delta) => Time += delta;
            Process += (delta) => RL1.Position = new Vector3(MathF.Sin(Time) * 4, 3, MathF.Cos(Time) * 4);
        }
    }
    class Floor : RenderObject<Vertex3D>
    {
        //private Occluder Occluder = new Occluder("Resources/Meshes/Cube.obj");
        public Floor(Scene Scene) : base(Mesh.Construct("Resources/Meshes/Cube.obj", (p, n, t) => new Vertex3D(p, n, t)))
        {
            Transform.Scale = new Vector3(16, 0.01f, 16);
            Transform.Position = new Vector3(0, 0, 0);
            //Occluder.Transform.Scale = new Vector3(16, 0.01f, 16);
            //Occluder.Transform.Position = new Vector3(0, 0, 0);

            TextureManager.Add_Texture("Resources/Textures/Grid.png", TextureMinFilter.Filter4Sgis, TextureMagFilter.Nearest, TextureWrapMode.ClampToBorder, 4);
            Material.SetUniformSampler2D("DiffuseTexture", "Resources/Textures/Grid.png");
            Material.SetUniformSampler2D("SpecularTexture","Resources/Textures/SpecMap.png");
            //Material.DebugUniforms();

            Scene.Add(this);
            //Scene.Add(Occluder);
        }
    }
    class Test : RenderObject<Vertex3D>
    {
        private Occluder Occluder = new Occluder("Resources/Meshes/Cube.obj");
        public Test(Scene Scene, Vector3 Position) : base(Mesh.Cube)
        {
            Transform.Position = Position;
            Occluder.Transform.Position = Position;
            
            Material.SetUniformSampler2D("DiffuseTexture", "Resources/Textures/Test.png");
            Material.SetUniformSampler2D("SpecularTexture", "Resources/Textures/SpecMap.png");
            Material.SetUniform("World", Transform.Matrix);
            Scene.Add(this);
            Scene.Add(Occluder);
        }
    }

    class TestLight_Point : Light_Point
    {
        public TestLight_Point(Scene Scene, float Px = 0, float Py = 1, float Pz = 0, float r = 1, float g = 1, float b = 1) : base(new Vector3(Px, Py, Pz), new Vector3(r, g, b))
        {
            Scene.Add(this);
        }
    }
    class TestLight_Direction : Light_Directional
    {
        public TestLight_Direction(Scene Scene, float Dx = 0, float Dy = 1, float Dz = 0, float r = 1, float g = 1, float b = 1) : base(new Vector3(Dx, Dy, Dz), new Vector3(r, g, b))
        {
            Scene.Add(this);
        }
    }
}


