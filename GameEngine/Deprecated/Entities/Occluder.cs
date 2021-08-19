using GameEngine.Resources;
using OpenTK.Graphics.OpenGL4;
using GameEngine.Geometry.Transform;
using GameEngine.Entities.Lighting;
using GameEngine.Entities.Culling;
using System;
namespace GameEngine.Entities
{
    [Obsolete]
    class Occluder : SpatialEntity<AbstractTransform3D>, IOccluder
    {
        private readonly Mesh<Simple3D> OccMesh;


        public static Mesh<Simple3D> BuildMesh(string path) => Mesh.Construct(path,
                (p, n, t) => new Simple3D(p), // simple3D only stores positional data, some shadows require normal data
                PrimitiveType.TrianglesAdjacency); // volume shadows require triangle adjacency

        public Occluder(Mesh<Simple3D> Mesh) : base(new Transform3D())
        {
            OccMesh = Mesh;
        }

        public Occluder(string path) : base(new Transform3D())
        {
            OccMesh = BuildMesh(path);
        }
        

        public void Occlude(IVolumeLight Light)
        {
            Light.ShadowProgram.SetUniform("Model", WorldMatrix);
            OccMesh.Draw();
        }

        Sphere CullShape = new Sphere();
        Sphere ICullable<Sphere>.CullShape => CullShape;
        Sphere ICullObserver<Sphere>.Observer => CullShape;
        bool ICullObserver<Sphere>.Detects(ICullable<Sphere> Entity) => CullShape.Intersect(Entity.CullShape);
        bool ICullObserver<Sphere>.Detects(ICullable<Box> Entity) => CullShape.Intersect(Entity.CullShape);
    }
}
