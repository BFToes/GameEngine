using Graphics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using System.Collections.Generic;
using Delaunator;
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
                RW.ViewPort.Camera = new Camera(50, RW.Size.X, RW.Size.Y, 2, 100);
                //BilWarp RO1 = new BilWarp(RW, RW.ViewPort);
                var RO1 = new Test(RW, RW.ViewPort);
                var RO2 = DelaunayPlain.FromRand(RW.ViewPort, 20);
                

                Action<MouseMoveEventArgs> OnDrag = (e) => ((Transform3D)RO1.Transform).Position += new Vector3(-e.Delta.X / RW.Size.X * ((Transform3D)RO1.Transform).Position.Z, e.Delta.Y / RW.Size.Y * ((Transform3D)RO1.Transform).Position.Z, 0);
                Action<MouseWheelEventArgs> OnScroll = (e) => ((Transform3D)RO1.Transform).Position += new Vector3(0, 0, e.OffsetY);
                RW.MouseDown += (e) => { RW.MouseMove += OnDrag;};
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
            ((Transform3D)Transform).Position = new Vector3(0, 0, -3);
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
    class DelaunayPlain : RenderObject<SimpleVertex>
    {

        public static DelaunayPlain FromRand(ViewPort VP, int n)
        {
            
            var R = new Random(0);
            List<Vector2> RandP = new List<Vector2>();
            for (int i = 0; i < n; i++) RandP.Add(new Vector2((float)R.NextDouble() * 2 - 1, (float)R.NextDouble() * 2 - 1));
            var V = Triangulator.From(RandP, (V) => V.X, (V) => V.Y).ToTriangles((x, y) => new SimpleVertex(x, y));
            
            return new DelaunayPlain(VP, V);
        }
        public DelaunayPlain(ViewPort VP, SimpleVertex[] V) : base(VP, V,
            $"Resources/shaderscripts/Simple.vert",
            $"Resources/shaderscripts/Simple.frag")
        {
            this.RenderingType = PrimitiveType.Triangles;
            this.PolygonMode = PolygonMode.Line;

            Transform = new Transform3D();
        }

    }
}


