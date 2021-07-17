using System;
using System.Collections.Generic;
using System.Text;
using Graphics.Rendering;
using Graphics.Resources;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
namespace Graphics.Entities
{
    interface IOccluder
    {
        public void Occlude(ILight Light);
    }


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
        

        public void Occlude(ILight Light)
        {
            Light.ShadowProgram.SetUniform("Model", WorldMatrix);
            OccMesh.Draw();
        }
    }
}
