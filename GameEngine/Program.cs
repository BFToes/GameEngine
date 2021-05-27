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
        /* Thing To Do:
         * 
         * Animation System:
         * - ???
         * 
         * 
         * PROBLEMS TO SORT OUT:
         *      Setup the fucking Idisposables already 
         *      its causing problems
         * 
         *      Simplify This shit:
         *          Stop making everything so needlessly complicated
         *          You dont need to implement every possible feature conceivable
         *          Make class do 1 thing and and do it well
         */

        static void Main(string[] args)
        {
            GameWindowSettings GWS = GameWindowSettings.Default;
            NativeWindowSettings NWS = NativeWindowSettings.Default;
            NWS.Size = new Vector2i(800);
            using (RenderWindow RW = new RenderWindow(GWS, NWS))
            {
                //RW.Scene.Camera = new Camera(50, RW.Size.X, RW.Size.Y, 2, 1024);
                RW.Scene.Camera.Position = new Vector3(0, 1, 3);
                Test RO1 = new Test(RW.Scene, new Vector3(0));
                Floor Floor = new Floor(RW.Scene);
                //for (int i = 0; i < 300; i++) new TestLight(RW.Scene, new Vector3(0, 1, 0));
                TestLight RL1 = new TestLight(RW.Scene, 0, 1, 0, 8, 8, 8);

                RW.Process += (delta) => RL1.Position = new Vector3(MathF.Sin(RW.Time) * 4, 4, MathF.Cos(RW.Time) * 4);

                //RW.Process += (delta) => RO1.Transform.Rotation = new Vector3(RW.Time * 0.3f, RW.Time * 0.7f, 0);

                Action<MouseMoveEventArgs> MoveCamera = (e) => RW.Scene.Camera.Position += 10 * new Vector3(RW.Scene.Camera.Matrix * -new Vector4(-e.DeltaX / RW.Size.X, e.DeltaY / RW.Size.Y, 0, 1));
                Action<MouseMoveEventArgs> RotaCamera = (e) => RW.Scene.Camera.Rotation += new Vector3(e.DeltaY / RW.Size.Y, e.DeltaX / RW.Size.X, 0);
                RW.MouseWheel += (e) => RW.Scene.Camera.Position += new Vector3(RW.Scene.Camera.Matrix * - new Vector4(0, 0, e.OffsetY, 1));
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
    class Floor : RenderObject<Vertex3D>
    {
        public Floor(Scene Scene) : base(Mesh<Vertex3D>.From(new Vertex3D[]
        {
            new Vertex3D(-1, 0,-1, 0, 1, 0, 0, 1), new Vertex3D( 1, 0, 1, 0, 1, 0, 1, 0), new Vertex3D( 1, 0,-1, 0, 1, 0, 1, 1), // top
            new Vertex3D( 1, 0, 1, 0, 1, 0, 1, 0), new Vertex3D(-1, 0,-1, 0, 1, 0, 0, 1), new Vertex3D(-1, 0, 1, 0, 1, 0, 0, 0),

        }))
        {
            Transform.Scale = new Vector3(256, 1, 256);
            TextureManager.Add_Texture("Resources/Textures/Grid.png", TextureMinFilter.Filter4Sgis, TextureMagFilter.Nearest, TextureWrapMode.ClampToBorder, 4);
            Material.SetUniformSampler2D("DiffuseTexture", "Resources/Textures/Grid.png");
            Material.SetUniformSampler2D("SpecularTexture","Resources/Textures/SpecMap.png");
            //Material.DebugUniforms();

            Scene.Add(this);
        }
    }
    class Test : RenderObject<Vertex3D>
    {
        private static Mesh<Vertex3D> ObjMesh = Mesh.Construct("Resources/Meshes/belly button.obj", (p, n, t) => new Vertex3D(p, n, t));
        private static Occluder Occluder = new Occluder(Mesh.Construct("Resources/Meshes/Cube.obj", (p, n, t) => new Simple3D(p)));
        public Test(Scene Scene, Vector3 Position) : base(ObjMesh)
        {
            Transform.Position = Position;
            Transform.Scale = new Vector3(0.4f);

            Material.SetUniformSampler2D("DiffuseTexture", "Resources/Textures/Test.png");
            Material.SetUniformSampler2D("SpecularTexture", "Resources/Textures/SpecMap.png");
            Material.SetUniform("World", Transform.Matrix);
            Scene.Add(this);
            Scene.Add(Occluder);
        }
    }

    class TestLight : PointLight
    {
        public TestLight(Scene Scene, float Px = 0, float Py = 1, float Pz = 0, float r = 1, float g = 1, float b = 1) : base(new Vector3(Px, Py, Pz), new Vector3(r, g, b))
        {
            Scene.Add(this);
        }
    }
}


