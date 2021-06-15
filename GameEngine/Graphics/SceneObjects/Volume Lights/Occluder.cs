using System;
using System.Collections.Generic;
using System.Text;
using Graphics.Resources;
using OpenTK.Graphics.OpenGL4;

namespace Graphics.Entities
{
    public interface IOccluder
    {
        public void Occlude(Light Light);
    }


    class Occluder : SpatialEntity<AbstractTransform3D>, IOccluder
    {
        private Mesh<Simple3D> OccMesh;

        public Occluder(string path) : base(new Transform3D())
        {
            OccMesh = Mesh.Construct(path, 
                (p, n, t) => new Simple3D(p), // simple3D only stores positional data, some shadows require normal data
                PrimitiveType.TrianglesAdjacency); // volume shadows require triangle adjacency
        }

        public void Occlude(Light Light)
        {
            Light.ShadowProgram.SetUniform("Model", WorldMatrix);
            OccMesh.Draw();
        }
    }
}
