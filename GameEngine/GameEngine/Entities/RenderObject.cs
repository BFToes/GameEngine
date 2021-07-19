using GameEngine.Resources;
using GameEngine.Entities.Culling;
using GameEngine.Geometry.Transform;
namespace GameEngine.Entities
{
    interface IRenderable : ICullable<Sphere>
    {
        public void Render();
    }

    /// <summary>
    /// An Object that renders onto the screen.
    /// </summary>
    class RenderObject<Vertex> : SpatialEntity<AbstractTransform3D>, IRenderable where Vertex : struct, IVertex
    {
        public ShaderProgram Material;
        public Mesh<Vertex> RenderMesh;


        private Sphere Sphere = new Sphere();
        Sphere ICullable<Sphere>.CullShape => Sphere;
        Sphere ICullObserver<Sphere>.Observer => Sphere;

        public RenderObject(Mesh<Vertex> Mesh, string VertexShader = "Resources/shaderscripts/Default.vert", string FragmentShader = "Resources/shaderscripts/Default.frag") : base(new Transform3D())
        {
            Material = ShaderProgram.ReadFrom(VertexShader, FragmentShader);
            RenderMesh = Mesh;
            Set_WorldMatrix += (WorldMatrix) => Material.SetUniform("Model", WorldMatrix);
            Set_WorldMatrix += Sphere.Update;
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
        bool ICullObserver<Sphere>.Detects(ICullable<Sphere> Entity) => Sphere.Intersect(Entity.CullShape);
        bool ICullObserver<Sphere>.Detects(ICullable<Box> Entity) => Sphere.Intersect(Entity.CullShape);
    }
}
