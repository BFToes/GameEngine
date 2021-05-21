using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using Graphics.Shaders;
using System.Reflection;

namespace Graphics
{
    interface IRenderable
    {
        public void Render();
    }

    /// <summary>
    /// An Object that renders onto the screen.
    /// </summary>
    abstract class RenderObject<Vertex> : IRenderable where Vertex : struct, IVertex
    {
        public ShaderProgram Material;
        public Mesh<Vertex> RenderMesh;
        public ITransform Transform;
        
        public RenderObject(Scene Scene, Mesh<Vertex> Mesh, string VertexShader = "Resources/shaderscripts/Default.vert", string FragmentShader = "Resources/shaderscripts/Default.frag")
        {
            Material = ShaderProgram.ReadFrom(VertexShader, FragmentShader);
            Transform = new Transform();
            RenderMesh = Mesh;

            Material.SetUpdatingUniform("Model", () => Transform.Matrix);
            Material.SetUniformBlock("Camera", 0); // 0 = Camera Block Binding Index
            Scene.Add(this);
        }
      
        /// <summary>
        /// Show this object in the Framebuffer
        /// </summary>
        public void Render()
        {
            Material.Use(); // tell openGL to use this objects program
            RenderMesh.Render();
        }
    }
}
