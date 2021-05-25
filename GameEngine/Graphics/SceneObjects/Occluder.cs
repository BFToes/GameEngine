using System;
using System.Collections.Generic;
using System.Text;
using Graphics.Resources;
namespace Graphics.SceneObjects
{
    class Occluder
    {
        private Mesh<Vertex3D> OccMesh;

        public Occluder(Mesh<Vertex3D> OccMesh) 
        {
            this.OccMesh = OccMesh;
        }

        public void Occlude()
        {
            OccMesh.Render();
        }
    }
}
