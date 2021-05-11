using Graphics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
namespace GameEngine
{
    class Program
    {
        /* Thing To Do:
         * 
         * Animation System:
         * - ???
         * 
         * 3D model Importer:
         * - library?
         * - ???
         * 
         * Lighting:
         * - set up camera like light thing
         * - setup invisible occluder objects and render from light source
         * - pre or post processing???
         * 
         * 
         * 
         * 
         * PROBLEMS TO SORT OUT:
         * Setup the fucking Idisposables already 
         * its causing problems
         * 
         * Uniform camera block should probably be set to the scene and not the camera 
         * that way it wont have to be recreated when a new camera is assigned
         * 
         * Simplify This shit:
         *      Stop making everything so needlessly complicated
         *      You dont need to implement every possible feature conceivable
         *      Make class do 1 thing and and do it well
         *      
         * Maybe I should Just switch to c++
         *      Advantages:
         *          struct inheritance
         *          all the tutorial can be followed more easily
         *          less messy
         *          pointers are useful
         *          
         *      Disadvantages:
         *          No garbage collection
         *          new language difficulties
         *          less inbuilt functionality
         * 
         */

        static void Main(string[] args)
        {
            GameWindowSettings GWS = GameWindowSettings.Default;
            NativeWindowSettings NWS = NativeWindowSettings.Default;
            NWS.Size = new Vector2i(800);
            using (RenderWindow RW = new RenderWindow(GWS, NWS))
            {
                //RW.Scene.Camera = new Camera(50, RW.Size.X, RW.Size.Y, 2, 1024);
                RW.Scene.Camera.Position = new Vector3(0, 0, 3);
                Floor Floor = new Floor(RW.Scene);
                Test RO1 = new Test(RW, RW.Scene);
                //var T = DelaunayPlain.FromRand(RW.ViewPort, 20000);


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

                RW.Process += (delta) => RO1.Transform.Rotation = new Vector3(RW.Time * 0.3f, RW.Time * 0.7f, 0);
                //RW.Process += (delta) => RW.ViewPort.Camera.Rotation = new Vector3(0, RW.Time * 0.3f, 0);
                //RW.Process += (delta) => ((Transform3D)RO1.Transform).Scale = new Vector3(1, (MathF.Cos(RW.Time * 1.2f) + 1) / 2, 1);
                //RW.Process += (delta) => ((Transform3D)RO1.Transform).Position = new Vector3(0, 0, MathF.Sin(RW.Time * 0.5f) - 4);

                RW.Run();
            }
        }
    }
    class Floor : RenderObject<Vertex3D>
    {
        public Floor(Scene RL) : base(RL, new Vertex3D[]
        {
            new Vertex3D(-1, 0,-1, 0, 1, 0, 0, 1), new Vertex3D( 1, 0, 1, 0, 1, 0, 1, 0), new Vertex3D( 1, 0,-1, 0, 1, 0, 1, 1), // top
            new Vertex3D( 1, 0, 1, 0, 1, 0, 1, 0), new Vertex3D(-1, 0,-1, 0, 1, 0, 0, 1), new Vertex3D(-1, 0, 1, 0, 1, 0, 0, 0),

        },
            $"Resources/shaderscripts/Default.vert",
            $"Resources/shaderscripts/Default.frag")
        {
            RenderingType = PrimitiveType.Triangles;
            Transform.Scale = new Vector3(256, 1, 256);
            Transform.Position = new Vector3(0, -3, 0);
            TextureManager.Add_Texture("Resources/Textures/Grid.png", TextureMinFilter.Filter4Sgis, TextureMagFilter.Nearest, TextureWrapMode.ClampToBorder, 4);
            Material.SetUniformSampler2D("DiffuseTexture", "Resources/Textures/Grid.png");
            Material.SetUniformSampler2D("SpecularTexture","Resources/Textures/SpecMap.png");


        }
    }
    class Test : RenderObject<Vertex3D>
    {
        public Test(RenderWindow RW, Scene RL) : base(RL, new Vertex3D[]
        {
            new Vertex3D( 1, 1, 1, 0, 0, 1, 1, 1), new Vertex3D(-1,-1, 1, 0, 0, 1, 0, 0), new Vertex3D( 1,-1, 1, 0, 0, 1, 1, 0), // front
            new Vertex3D(-1,-1, 1, 0, 0, 1, 0, 0), new Vertex3D( 1, 1, 1, 0, 0, 1, 1, 1), new Vertex3D(-1, 1, 1, 0, 0, 1, 0, 1),

            new Vertex3D(-1,-1,-1, 0, 0,-1, 0, 0), new Vertex3D( 1, 1,-1, 0, 0,-1, 1, 1), new Vertex3D( 1,-1,-1, 0, 0,-1, 1, 0), // back
            new Vertex3D( 1, 1,-1, 0, 0,-1, 1, 1), new Vertex3D(-1,-1,-1, 0, 0,-1, 0, 0), new Vertex3D(-1, 1,-1, 0, 0,-1, 0, 1),

            new Vertex3D(-1,-1,-1,-1, 0, 0, 0, 1), new Vertex3D(-1, 1, 1,-1, 0, 0, 1, 0), new Vertex3D(-1, 1,-1,-1, 0, 0, 1, 1), // left
            new Vertex3D(-1, 1, 1,-1, 0, 0, 1, 0), new Vertex3D(-1,-1,-1,-1, 0, 0, 0, 1), new Vertex3D(-1,-1, 1,-1, 0, 0, 0, 0),

            new Vertex3D( 1, 1, 1, 1, 0, 0, 1, 0), new Vertex3D( 1,-1,-1, 1, 0, 0, 0, 1), new Vertex3D( 1, 1,-1, 1, 0, 0, 1, 1), // right
            new Vertex3D( 1,-1,-1, 1, 0, 0, 0, 1), new Vertex3D( 1, 1, 1, 1, 0, 0, 1, 0), new Vertex3D( 1,-1, 1, 1, 0, 0, 0, 0),

            new Vertex3D(-1, 1,-1, 0, 1, 0, 0, 1), new Vertex3D( 1, 1, 1, 0, 1, 0, 1, 0), new Vertex3D( 1, 1,-1, 0, 1, 0, 1, 1), // top
            new Vertex3D( 1, 1, 1, 0, 1, 0, 1, 0), new Vertex3D(-1, 1,-1, 0, 0, 0, 0, 1), new Vertex3D(-1, 1, 1, 0, 1, 0, 0, 0),

            new Vertex3D( 1,-1, 1, 0,-1, 0, 1, 0), new Vertex3D(-1,-1,-1, 0,-1, 0, 0, 1), new Vertex3D( 1,-1,-1, 0,-1, 0, 1, 1), // bottom
            new Vertex3D(-1,-1,-1, 0,-1, 0, 0, 1), new Vertex3D( 1,-1, 1, 0,-1, 0, 1, 0), new Vertex3D(-1,-1, 1, 0,-1, 0, 0, 0),
        },
            $"Resources/shaderscripts/Default.vert",
            $"Resources/shaderscripts/Default.frag")
        {
            RenderingType = PrimitiveType.Triangles;
            Material.SetUniformSampler2D("DiffuseTexture", "Resources/Textures/Test.png");
            Material.SetUniformSampler2D("SpecularTexture", "Resources/Textures/SpecMap.png");
            Material.SetUpdatingUniform("Time", () => RW.Time);
            Material.SetUniform("Model", Transform.Matrix);
        }
    }
}


