using System;
using System.Collections.Generic;
using System.Text;
using Graphics.Resources;
using OpenTK.Graphics.OpenGL4;
using Graphics.Shaders;
using OpenTK.Mathematics;

namespace Graphics.SceneObjects
{
    class Occluder
    {
        private Mesh<Simple3D> OccMesh;
        public ITransform Transform;

        public Occluder(string path) 
        {
            this.OccMesh = Mesh.Construct(path, (p, n, t) => new Simple3D(p), PrimitiveType.TrianglesAdjacency);
            this.Transform = new Transform(); // 3D transform
        }

        public void Occlude(Light Light) // all updated at different times
        {
            Light.ShadowProgram.SetUniform("Model", Transform.Matrix);
            OccMesh.Draw();
        }
    }
}
