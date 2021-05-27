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
        public readonly static ShaderProgram ShadowProgram = ShaderProgram.ReadFrom(
            "Resources/Shaderscripts/Rendering/Shadow.vert",
            "Resources/Shaderscripts/Rendering/Shadow.geom",
            "Resources/Shaderscripts/Rendering/Shadow.frag");


        private Mesh<Simple3D> OccMesh;
        public ITransform Transform;

        public Occluder(Mesh<Simple3D> OccMesh) 
        {
            this.OccMesh = OccMesh;
            this.Transform = new Transform(); // 3D transform
        }

        public void Occlude() // all updated at different times
        {
            ShadowProgram.SetUniform("Model", Transform.Matrix);
            OccMesh.Draw();
        }
    }
}
