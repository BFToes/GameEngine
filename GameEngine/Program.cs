using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common; // mouse event
using OpenTK.Windowing.GraphicsLibraryFramework; // mouse button
using System;

using GameEngine.Rendering;
using GameEngine.Resources;
using GameEngine.Entities;

namespace GameEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            using RenderWindow RW = RenderWindow.New(true, 800, 800);

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
    class World : Scene
    {

        public World() : base(800, 800)
        {
            Camera.Transform.Position = new Vector3(0, 2, 3);

            Test.CreateSquare(8, 1, 3f, this);

            new Floor(this);
            this.Add(new Light_Dir(new Vector3(0, -1, -1), new Vector3(0.8f)));
        }
    }
    class Floor : RenderObject<Vertex3D>
    {
        private static readonly Occluder occluder = new Occluder("Resources/Meshes/Cube.obj");
        public Floor(Scene Scene) : base(Mesh.Cube)
        {
            Add(occluder); // add child
            Transform.Scale = new Vector3(256, 0.01f, 256);
            Transform.Position = new Vector3(0, -0.01f, 0);

            Texture.Add_Sampler("Resources/Textures/Grid.png", TextureMinFilter.Filter4Sgis, TextureMagFilter.Nearest, TextureWrapMode.ClampToBorder, 4);
            Material.SetUniformSampler2D("DiffuseTexture", "Resources/Textures/Grid.png");
            Material.SetUniformSampler2D("SpecularTexture","Resources/Textures/SpecMap.png");

            Scene.Add(this);
        }
    }
    class Test : RenderObject<Vertex3D>
    {
        public float x, y;
        
        private static readonly Mesh<Vertex3D> mesh = Mesh.Construct("Resources/Meshes/Cube.obj", (p, n, t) => new Vertex3D(p, n, t));
        private static readonly Mesh<Simple3D> Occmesh = Occluder.BuildMesh("Resources/Meshes/Cube.obj");
        private Occluder occluder = new Occluder(Occmesh);

        public static void CreateSquare(int Width, int Height, float Divide, Scene Scene)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var R = new Test(x, y, Scene);
                    Scene.Process += R.OnProcess;
                    R.Transform.Position = new Vector3((x - (Width - 1) / 2f) * Divide, 2f, (y - (Height - 1) / 2f) * Divide);
                }
            }
        }

        public Test(float x, float y, Scene Scene) : base(mesh)
        {
            this.x = x;
            this.y = y;

            Add(occluder);
            Transform.Position = new Vector3();
            Transform.Scale = new Vector3(1, 1, 1);

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
    }

    class TestLight : RenderObject<Vertex3D>
    {
        public const float Radius = 5;
        public const int TotalLights = 0;
        private Light_Pnt Light;
        private float time, Floatn;
        private static Color4[] Colours = new Color4[] { Color4.Red, Color4.Lime, Color4.Blue, Color4.Yellow, Color4.Cyan, Color4.Magenta };
        public TestLight(Scene Scene, int n) : base(Mesh.Sphere,
            "Resources/shaderscripts/Default.vert",
            "Resources/shaderscripts/SolidColour.frag")
        {
            Vector3 C = new Vector3(Colours[n].R, Colours[n].G, Colours[n].B);

            Material.SetUniformSampler2D("SpecularTexture", "Resources/Textures/SpecMap.png");
            Material.SetUniform("DiffuseColor", C);
            this.Transform.Scale = new Vector3(0.1f);
            Light = new Light_Pnt(Vector3.Zero, C, 2, 0, 80);

            this.Floatn = (float)n / TotalLights;
            this.Add(Light);
            Scene.Add(this);
        }
        public override void OnProcess(float delta)
        { 
            time += delta;
            Transform.Position = new Vector3(MathF.Sin(2 * MathF.PI * Floatn + time) * Radius, 1.6f, MathF.Cos(2 * MathF.PI * Floatn + time) * Radius);
            base.OnProcess(delta);
        }
    }
}


