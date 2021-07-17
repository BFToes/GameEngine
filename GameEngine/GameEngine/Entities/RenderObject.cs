using GameEngine.Resources;
using GameEngine.Entities.Culling;
using GameEngine.Geometry.Transform;
namespace GameEngine.Entities
{
    interface IRenderable : ICullable<CullSphere>
    {
        public void Render();
    }

    /// <summary>
    /// An Object that renders onto the screen.
    /// </summary>
    abstract class RenderObject<Vertex> : SpatialEntity<AbstractTransform3D>, IRenderable where Vertex : struct, IVertex
    {
        public ShaderProgram Material;
        public Mesh<Vertex> RenderMesh;


        private CullSphere Sphere = new CullSphere();
        public CullSphere CullShape => Sphere;

        public RenderObject(Mesh<Vertex> Mesh, string VertexShader = "Resources/shaderscripts/Default.vert", string FragmentShader = "Resources/shaderscripts/Default.frag") : base(new Transform3D())
        {
            Material = ShaderProgram.ReadFrom(VertexShader, FragmentShader);
            RenderMesh = Mesh;
            Set_WorldMatrix += (WorldMatrix) => Material.SetUniform("Model", WorldMatrix);
            Set_WorldMatrix += Sphere.Extract;
            Material.SetUniformBlock("CameraBlock", 0); // 0 = Camera Block Binding Index
        }

        /// <summary>
        /// Show this object in the Framebuffer
        /// </summary>
        public void Render()
        {
            Material.Use(); // tell openGL to use this objects program
            RenderMesh.Draw();

            
        }
    }
}
