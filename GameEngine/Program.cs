using Graphics.Entities;
using Graphics.Rendering;
using Graphics.Resources;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
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

            
            for (int y = 0; y < Test.SquareLength; y++)
            {
                for (int x = 0; x < Test.SquareLength; x++)
                {
                    var R = new Test(x, y, this);
                    Process += R.OnProcess;
                }
            }


            var RO4 = new Floor(this);
            this.Add(new Light_Dir(new Vector3(0, -1, -1), new Vector3(0.2f)));
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
        public const int SquareLength = 1;
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
            Transform.Scale = new Vector3(1f, 1f, 1f);

            Material.SetUniformSampler2D("DiffuseTexture", "Resources/Textures/Test.png");
            Material.SetUniformSampler2D("SpecularTexture", "Resources/Textures/SpecMap.png");
            Material.SetUniform("World", Transform.Matrix);

            for (int n = 0; n < TestLight.TotalLights; n++)
            {
                var L = new TestLight(Scene, n);
                Add(L);
            }

            Scene.Add(this);
        }
        public override void OnProcess(float delta)
        {
            Transform.Position = new Vector3(x - (SquareLength - 1) / 2f, MathF.Sin((y + x) * 0.2f) + 0.01f, y - (SquareLength - 1) / 2f);
            base.OnProcess(delta);
        }
    }

    class TestLight : RenderObject<Vertex3D>
    {
        public const float Radius = 3;
        public const int TotalLights = 3;
        private Light_Pnt Light;
        private float time;
        private float Floatn;
        private static Color4[] Colours = new Color4[] { Color4.Red, Color4.Lime, Color4.Blue, Color4.Yellow, Color4.Cyan, Color4.Magenta };
        public TestLight(Scene Scene, int n) : base(Mesh.Cube,
            "Resources/shaderscripts/Default.vert",
            "Resources/shaderscripts/SolidColour.frag")
        {
            Vector3 C = new Vector3(Colours[n].R, Colours[n].G, Colours[n].B);

            Material.SetUniformSampler2D("SpecularTexture", "Resources/Textures/SpecMap.png");
            Material.SetUniform("DiffuseColor", C);
            this.Transform.Scale = new Vector3(0.1f);
            Light = new Light_Pnt(Vector3.Zero, C);
            Light.DiffuseIntensity = 25;

            this.Floatn = (float)n / TotalLights;
            this.Add(Light);
            Scene.Add(this);
        }
        public override void OnProcess(float delta)
        { 
            time += delta;
            Transform.Position = new Vector3(MathF.Sin(2 * MathF.PI * Floatn + time) * Radius, 5.5f, MathF.Cos(2 * MathF.PI * Floatn + time) * Radius);
            base.OnProcess(delta);
        }
    }
}


