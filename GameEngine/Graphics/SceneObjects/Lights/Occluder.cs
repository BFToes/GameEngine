using System;
using System.Collections.Generic;
using System.Text;
using Graphics.Resources;
using OpenTK.Graphics.OpenGL4;
using Graphics.Shaders;
using OpenTK.Mathematics;

namespace Graphics.SceneObjects
{
    class Occluder : SpatialEntity<AbstractTransform3D>
    {
        private Mesh<Simple3D> OccMesh;

        public Occluder(string path) : base(new Transform3D())
        {
            OccMesh = Mesh.Construct(path, 
                (p, n, t) => new Simple3D(p), // simple3D only stores positional data
                PrimitiveType.TrianglesAdjacency); // all occluders must use Triangle Adjacency
        }

        public void Occlude(Light Light)
        {
            Light.ShadowProgram.SetUniform("Model", WorldMatrix);
            OccMesh.Draw();
        }
    }
}
