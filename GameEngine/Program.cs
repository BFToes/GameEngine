using Graphics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System;
using OpenTK.Graphics.OpenGL4;

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
                
                RW.Process += (delta) => ((Transform3D)RO1.Transform).Rotation = new Vector3(0, RW.Time * 0.3f, 0);
                //RW.Process += (delta) => ((Transform3D)RO1.Transform).Scale = new Vector3(1, (MathF.Cos(RW.Time * 1.2f) + 1) / 2, 1);
                //RW.Process += (delta) => ((Transform3D)RO1.Transform).Position = new Vector3(MathF.Cos(RW.Time *-0.7f), MathF.Sin(RW.Time * 0.7f), -3 + MathF.Sin(RW.Time * 0.5f));


                RW.Run();
            }
        }
    }

    class Test : RenderObject<Vertex3D>
    {
        
        /* front 
         * Back 
         * Left
         * Right
         * Top
         * Bottom
         * 
         */

        public Test(RenderWindow RW, ViewPort RL) : base(RL, new Vertex3D[]
        {
            new Vertex3D(-1,-1,-1, 0, 0, 0, 0, 0), new Vertex3D( 1,-1,-1, 0, 0, 0, 1, 0), new Vertex3D( 1, 1,-1, 0, 0, 0, 1, 1), // back N
            new Vertex3D( 1, 1,-1, 0, 0, 0, 1, 1), new Vertex3D(-1, 1,-1, 0, 0, 0, 0, 1), new Vertex3D(-1,-1,-1, 0, 0, 0, 0, 0),

            new Vertex3D(-1,-1, 1, 1, 0, 0, 0, 0), new Vertex3D( 1,-1, 1, 1, 0, 0, 1, 0), new Vertex3D( 1, 1, 1, 1, 0, 0, 1, 1), // front Y
            new Vertex3D( 1, 1, 1, 1, 0, 0, 1, 1), new Vertex3D(-1, 1, 1, 1, 0, 0, 0, 1), new Vertex3D(-1,-1, 1, 1, 0, 0, 0, 0),

            new Vertex3D(-1, 1, 1, 0, 1, 0, 1, 0), new Vertex3D(-1, 1,-1, 0, 1, 0, 1, 1), new Vertex3D(-1,-1,-1, 0, 1, 0, 0, 1), // Y
            new Vertex3D(-1,-1,-1, 0, 1, 0, 0, 1), new Vertex3D(-1,-1, 1, 0, 1, 0, 0, 0), new Vertex3D(-1, 1, 1, 0, 1, 0, 1, 0),

            new Vertex3D( 1,-1,-1, 1, 1, 0, 0, 1), new Vertex3D( 1, 1,-1, 1, 1, 0, 1, 1), new Vertex3D( 1, 1, 1, 1, 1, 0, 1, 0), // Y
            new Vertex3D( 1, 1, 1, 1, 1, 0, 1, 0), new Vertex3D( 1,-1, 1, 1, 1, 0, 0, 0), new Vertex3D( 1,-1,-1, 1, 1, 0, 0, 1),

            new Vertex3D(-1,-1,-1, 0, 0, 1, 0, 1), new Vertex3D( 1,-1,-1, 0, 0, 1, 1, 1), new Vertex3D( 1,-1, 1, 0, 0, 1, 1, 0), // N
            new Vertex3D( 1,-1, 1, 0, 0, 1, 1, 0), new Vertex3D(-1,-1, 1, 0, 0, 1, 0, 0), new Vertex3D(-1,-1,-1, 0, 0, 1, 0, 1),

            new Vertex3D( 1, 1, 1, 1, 0, 1, 1, 0), new Vertex3D( 1, 1,-1, 1, 0, 1, 1, 1), new Vertex3D(-1, 1,-1, 1, 0, 1, 0, 1), // top side N
            new Vertex3D(-1, 1,-1, 1, 0, 1, 0, 1), new Vertex3D(-1, 1, 1, 1, 0, 1, 0, 0), new Vertex3D( 1, 1, 1, 1, 0, 1, 1, 0), 
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
            $"Resources/shaderscripts/Bilinear Warp/BilinearW.frag"
            )
        {
            RenderingType = PrimitiveType.LinesAdjacency;
            Transform = new Transform3D();
            
            Material.Uniforms["Texture"] = () => "Resources/Textures/Test.png";
            Material.Uniforms["VP"] = () => new Vector4i(RW.ViewPort.Rect.X, RW.ViewPort.Rect.Y, RW.ViewPort.Rect.Width, RW.ViewPort.Rect.Height);
            
        }
    }
}


