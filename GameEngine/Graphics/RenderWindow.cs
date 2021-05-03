using System;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using System.Drawing;
namespace Graphics
{
    class RenderWindow : GameWindow
    {
        public Scene Scene;
        public Action<float> Process;
        public float Time;

        private int VAO; // vertex array object
        private int VBO; // vertex buffer

        public RenderWindow(GameWindowSettings GWS, NativeWindowSettings NWS) : base(GWS, NWS)
        {
            Scene = new Scene(
                $"Resources/shaderscripts/PostProcess/PostProcess.vert",
                $"Resources/shaderscripts/PostProcess/PostProcess.frag", 
                Size.X, Size.Y);
            Process = (delta) => { Time += delta; };
            VSync = VSyncMode.On;

            #region vertex buffer/array object setup
            // setup array
            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);
            // vector2 position
            GL.VertexArrayAttribBinding(VAO, 0, 0); // generates a new attribute binding to location in vertex buffer array
            GL.EnableVertexArrayAttrib(VAO, 0); // enables the attribute binding to location
            GL.VertexArrayAttribFormat(VAO, 0, 2, VertexAttribType.Float, false, 0);
            // vector2 FragUV
            GL.VertexArrayAttribBinding(VAO, 1, 0);
            GL.EnableVertexArrayAttrib(VAO, 1);
            GL.VertexArrayAttribFormat(VAO, 1, 2, VertexAttribType.Float, false, 8);
            // setup buffer
            VBO = GL.GenBuffer(); // generates and binds vertex buffer object
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO); // uses this buffer
            GL.VertexArrayVertexBuffer(VAO, 0, VBO, IntPtr.Zero, new Vertex2D().SizeInBytes); // assigns vertice data

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, 4 * new Vertex2D().SizeInBytes, new float[16] { -1, -1, 0, 0, 1, -1, 1, 0, 1, 1, 1, 1, -1, 1, 0, 1 }, BufferUsageHint.StaticDraw);
            #endregion

            #region OpenGL Functions to Enable
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            #endregion

        }

        protected override void OnResize(ResizeEventArgs e)
        {
            Size = e.Size;
            Scene.Size = e.Size;
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            
            GL.ClearColor(Color.DarkRed);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            //Title = $"{MathF.Round(1 / (float)e.Time)}";

            Process((float)e.Time);

            Scene.OnRender();

            // use default
            GL.CullFace(CullFaceMode.Back);
            GL.DepthFunc(DepthFunction.Less);
            GL.BlendFunc(BlendingFactor.Src1Alpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Viewport(0, 0, Size.X, Size.Y);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // draw viewport frame on screen
            Scene.Material.Use();
            GL.BindVertexArray(VAO);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            SwapBuffers(); // swap out old buffer with new buffer
        }
    }
}

