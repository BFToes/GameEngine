using Graphics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;

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
                //BilWarp RO1 = new BilWarp(RW, RW.ViewPort);
                Test RO1 = new Test(RW, RW.ViewPort);
                ((Transform3D)RO1.Transform).Position = new Vector3(0, 0, -3);
                
                Action<MouseMoveEventArgs> OnDrag = (e) => ((Transform3D)RO1.Transform).Position += new Vector3(-e.Delta.X / RW.Size.X * ((Transform3D)RO1.Transform).Position.Z, e.Delta.Y / RW.Size.Y * ((Transform3D)RO1.Transform).Position.Z, 0);
                Action<MouseWheelEventArgs> OnScroll = (e) => ((Transform3D)RO1.Transform).Position += new Vector3(0, 0, e.OffsetY);
                RW.MouseDown += (e) => { RW.MouseMove += OnDrag; };
                RW.MouseUp += (e) => { RW.MouseMove -= OnDrag; };
                RW.MouseWheel += OnScroll;
                RW.Process += (delta) => ((Transform3D)RO1.Transform).Rotation = new Vector3(RW.Time * 0.7f, RW.Time * 0.3f, 0);
                //RW.Process += (delta) => ((Transform3D)RO1.Transform).Scale = new Vector3(1, (MathF.Cos(RW.Time * 1.2f) + 1) / 2, 1);
                //RW.Process += (delta) => ((Transform3D)RO1.Transform).Position = new Vector3(0, 0, MathF.Sin(RW.Time * 0.5f) - 4);

                RW.Run();
            }
        }
    }

    class Test : RenderObject<Vertex3D>
    {
        public Test(RenderWindow RW, ViewPort RL) : base(RL, new Vertex3D[]
        {
            new Vertex3D( 1, 1, 1, 1, 0, 0, 1, 1), new Vertex3D(-1,-1, 1, 1, 0, 0, 0, 0), new Vertex3D( 1,-1, 1, 1, 0, 0, 1, 0), // front
            new Vertex3D(-1,-1, 1, 1, 0, 0, 0, 0), new Vertex3D( 1, 1, 1, 1, 0, 0, 1, 1), new Vertex3D(-1, 1, 1, 1, 0, 0, 0, 1),

            new Vertex3D(-1,-1,-1, 0, 0, 0, 0, 0), new Vertex3D( 1, 1,-1, 0, 0, 0, 1, 1), new Vertex3D( 1,-1,-1, 0, 0, 0, 1, 0), // back
            new Vertex3D( 1, 1,-1, 0, 0, 0, 1, 1), new Vertex3D(-1,-1,-1, 0, 0, 0, 0, 0), new Vertex3D(-1, 1,-1, 0, 0, 0, 0, 1),

            new Vertex3D(-1,-1,-1, 0, 1, 0, 0, 1), new Vertex3D(-1, 1, 1, 0, 1, 0, 1, 0), new Vertex3D(-1, 1,-1, 0, 1, 0, 1, 1), // left
            new Vertex3D(-1, 1, 1, 0, 1, 0, 1, 0), new Vertex3D(-1,-1,-1, 0, 1, 0, 0, 1), new Vertex3D(-1,-1, 1, 0, 1, 0, 0, 0),

            new Vertex3D( 1, 1, 1, 1, 1, 0, 1, 0), new Vertex3D( 1,-1,-1, 1, 1, 0, 0, 1), new Vertex3D( 1, 1,-1, 1, 1, 0, 1, 1), // right
            new Vertex3D( 1,-1,-1, 1, 1, 0, 0, 1), new Vertex3D( 1, 1, 1, 1, 1, 0, 1, 0), new Vertex3D( 1,-1, 1, 1, 1, 0, 0, 0),

            new Vertex3D(-1, 1,-1, 1, 0, 1, 0, 1), new Vertex3D( 1, 1, 1, 1, 0, 1, 1, 0), new Vertex3D( 1, 1,-1, 1, 0, 1, 1, 1), // top
            new Vertex3D( 1, 1, 1, 1, 0, 1, 1, 0), new Vertex3D(-1, 1,-1, 1, 0, 1, 0, 1), new Vertex3D(-1, 1, 1, 1, 0, 1, 0, 0),

            new Vertex3D( 1,-1, 1, 0, 0, 1, 1, 0), new Vertex3D(-1,-1,-1, 0, 0, 1, 0, 1), new Vertex3D( 1,-1,-1, 0, 0, 1, 1, 1), // bottom
            new Vertex3D(-1,-1,-1, 0, 0, 1, 0, 1), new Vertex3D( 1,-1, 1, 0, 0, 1, 1, 0), new Vertex3D(-1,-1, 1, 0, 0, 1, 0, 0), 
        },
            $"Resources/shaderscripts/Default.vert",
            $"Resources/shaderscripts/Default.frag")
        {
            RenderingType = PrimitiveType.Triangles;
            Transform = new Transform3D();

            Material.Uniforms["Texture"] = () => "Resources/Textures/Test.png";
        }
    }
    class BilWarp : RenderObject<Vertex3D>
    {
        public BilWarp(RenderWindow RW, ViewPort RL) : base(RL, new Vertex3D[]
            {
                /*
                new Vertex3D(-1, 1, 1, 0, 0, 0, 0, 1),
                new Vertex3D(-1,-1, 1, 0, 0, 0, 0, 0),
                new Vertex3D( 1,-1, 1, 0, 0, 0, 1, 0),
                new Vertex3D( 1, 1, 1, 0, 0, 0, 1, 1),
                */
                new Vertex3D(-1,-1,-1, 0, 0, 0, 0, 0),
                new Vertex3D( 1,-1,-1, 0, 0, 0, 1, 0),
                new Vertex3D( 1, 1,-1, 0, 0, 0, 1, 1),
                new Vertex3D(-1, 1,-1, 0, 0, 0, 0, 1),

                new Vertex3D(-1,-1, 1, 0, 0, 0, 0, 0),
                new Vertex3D( 1,-1, 1, 0, 0, 0, 1, 0),
                new Vertex3D( 1, 1, 1, 0, 0, 0, 1, 1),
                new Vertex3D(-1, 1, 1, 0, 0, 0, 0, 1),

                new Vertex3D(-1, 1, 1, 0, 0, 0, 1, 0),
                new Vertex3D(-1, 1,-1, 0, 0, 0, 1, 1),
                new Vertex3D(-1,-1,-1, 0, 0, 0, 0, 1),
                new Vertex3D(-1,-1, 1, 0, 0, 0, 0, 0),

                new Vertex3D( 1, 1, 1, 0, 0, 0, 1, 0),
                new Vertex3D( 1, 1,-1, 0, 0, 0, 1, 1),
                new Vertex3D( 1,-1,-1, 0, 0, 0, 0, 1),
                new Vertex3D( 1,-1, 1, 0, 0, 0, 0, 0),

                new Vertex3D(-1,-1,-1, 0, 0, 0, 0, 1),
                new Vertex3D( 1,-1,-1, 0, 0, 0, 1, 1),
                new Vertex3D( 1,-1, 1, 0, 0, 0, 1, 0),
                new Vertex3D(-1,-1, 1, 0, 0, 0, 0, 0),

                new Vertex3D(-1, 1,-1, 0, 0, 0, 0, 1),
                new Vertex3D( 1, 1,-1, 0, 0, 0, 1, 1),
                new Vertex3D( 1, 1, 1, 0, 0, 0, 1, 0),
                new Vertex3D(-1, 1, 1, 0, 0, 0, 0, 0),
            },
            $"Resources/shaderscripts/Bilinear Warp/BilinearW.vert",
            $"Resources/shaderscripts/Bilinear Warp/BilinearW.geom",
            $"Resources/shaderscripts/Bilinear Warp/BilinearW.frag")
        {
            RenderingType = PrimitiveType.LinesAdjacency;
            Transform = new Transform3D();
            
            Material.Uniforms["Texture"] = () => "Resources/Textures/Test.png";
            Material.Uniforms["VP"] = () => new Vector4i(RW.ViewPort.Rect.X, RW.ViewPort.Rect.Y, RW.ViewPort.Rect.Width, RW.ViewPort.Rect.Height);
            
        }
    }
}


