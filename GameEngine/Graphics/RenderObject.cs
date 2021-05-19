using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using Graphics.Shaders;
using System.Reflection;

namespace Graphics
{


    /// <summary>
    /// An Object that renders onto the screen.
    /// </summary>
    abstract class RenderObject<Vertex> : IRenderable where Vertex : struct, IVertex
    {
        public ShaderProgram Material;
        public Mesh<Vertex> RenderMesh;
        public ITransform Transform;
        
        // used in Render() to determine how this object renders
        protected PrimitiveType RenderingType = PrimitiveType.Triangles;
        protected PolygonMode PolygonMode = PolygonMode.Fill;

        public RenderObject(Scene Canvas, Mesh<Vertex> Mesh, string VertexShader, string FragmentShader)
        {
            Material = ShaderProgram.ReadFrom(VertexShader, FragmentShader);
            Transform = new Transform();
            RenderMesh = Mesh;

            Material.SetUpdatingUniform("Model", () => Transform.Matrix);
            Material.SetUniformBlock("Camera", 0); // 0 = Camera Block Binding Index

            Canvas.Add(this);
        }
      
        /// <summary>
        /// Show this object in the Framebuffer
        /// </summary>
        public void Render()
        {
            Material.Use(); // tell openGL to use this objects program
            GL.BindVertexArray(RenderMesh); // use this object's mesh
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode); // use this programs rendering modes
            GL.DrawArrays(RenderingType, 0, RenderMesh.Length); // draw these vertices in triangles, 0 to the number of vertices
        }
    }
}
