using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
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

                Action<MouseMoveEventArgs> MoveCamera = (e) => RW.Scene.Camera.Transform.Position += 10 * new Vector3(RW.Scene.Camera.Transform.Matrix * -new Vector4(-e.DeltaX / RW.Size.X, e.DeltaY / RW.Size.Y, 0, 1));
                Action<MouseMoveEventArgs> RotaCamera = (e) => RW.Scene.Camera.Transform.Rotation += new Vector3(e.DeltaY / RW.Size.Y, e.DeltaX / RW.Size.X, 0);

                RW.MouseWheel += (e) => RW.Scene.Camera.Transform.Position += new Vector3(RW.Scene.Camera.Transform.Matrix * -new Vector4(0, 0, e.OffsetY, 1));
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
            Camera.Transform.Position = new Vector3(0, 2, 3);
            Process += (delta) => Time += delta;

            
            for (int y = 0; y < Test.TotalObjects; y++)
            {
                for (int x = 0; x < Test.TotalObjects; x++)
                {
                    var R = new Test(x, y, this);
                    Process += R.Process;
                }
            }


            for (float n = 0; n < 1; n += 1f / TestLight.TotalLights)
            {
                var L = new TestLight(this, n, n, 1 - n, n);
                Process += L.Process;

            }

            var RO4 = new Floor(this);
            
            

            //Process += (delta) => RL4.Direction = new Vector3(MathF.Sin(Time * 2), -1, MathF.Cos(Time * 2));
        }
    }
    class Floor : RenderObject<Vertex3D>
    {
        private Occluder occluder = new Occluder("Resources/Meshes/Cube.obj");
        public Floor(Scene Scene) : base(Mesh.Cube)
        {
            Add(occluder); // add child
            Transform.Scale = new Vector3(16, 0.01f, 16);
            Transform.Position = new Vector3(0, -0.01f, 0);

            TextureManager.Add_Texture("Resources/Textures/Grid.png", TextureMinFilter.Filter4Sgis, TextureMagFilter.Nearest, TextureWrapMode.ClampToBorder, 4);
            Material.SetUniformSampler2D("DiffuseTexture", "Resources/Textures/Grid.png");
            Material.SetUniformSampler2D("SpecularTexture","Resources/Textures/SpecMap.png");

            Scene.Add(this);
        }
    }
    class Test : RenderObject<Vertex3D>
    {
        public const int TotalObjects = 4;
        public float x;
        public float y;
        private Occluder occluder = new Occluder("Resources/Meshes/belly button.obj");
        private static Mesh<Vertex3D> mesh = Mesh.Construct("Resources/Meshes/belly button.obj", (p, n, t) => new Vertex3D(p, n, t));
        public Test(float x, float y, Scene Scene) : base(mesh)
        {
            this.x = x;
            this.y = y;

            Add(occluder);
            Transform.Position = new Vector3();
            Transform.Scale = new Vector3(0.1f, 0.1f, 0.1f);

            Material.SetUniformSampler2D("DiffuseTexture", "Resources/Textures/Test.png");
            Material.SetUniformSampler2D("SpecularTexture", "Resources/Textures/SpecMap.png");
            Material.SetUniform("World", Transform.Matrix);
            Scene.Add(this);
        }
        public void Process(float delta)
        {
            Transform.Position = new Vector3(x - (TotalObjects - 1) / 2f, MathF.Sin((y + x) * 0.2f) + 0.01f, y - (TotalObjects - 1) / 2f);
        }
    }

    class TestLight : Light_Pnt
    {
        public const float Radius = 4;
        public const int TotalLights = 3;
        private float n;
        public TestLight(Scene Scene, float n, float r = 1, float g = 1, float b = 1) : base(new Vector3(0, 0, 0), new Vector3(r, g, b))
        {
            this.n = n;
            Scene.Add(this);
        }
        public void Process(float delta)
        {
            Position = new Vector3(MathF.Sin(2 * MathF.PI * n) * Radius, 3, MathF.Cos(2 * MathF.PI * n) * Radius);
        }
    }
    class TestLight_Direction : Light_Dir
    {
        public TestLight_Direction(Scene Scene, float Dx = 0, float Dy = -1, float Dz = 1, float r = 1, float g = 1, float b = 1) : base(new Vector3(Dx, Dy, Dz), new Vector3(r, g, b))
        {
            Scene.Add(this);
        }
    }
}


