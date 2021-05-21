using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Graphics.Shaders;
namespace Graphics
{
    /*
    struct LightData {
        vec3 Colour;
        float AmbientIntensity;
        vec3 Position;
        float DiffuseIntensity;
        vec3 Attenuation;
    };  
    */

    interface ILightObject
    {
        public Vector3 Position { get; set; }
        public float AmbientIntensity { get; set; }
        public Vector3 Colour { get; set; }
        public float DiffuseIntensity { get; set; }
        public Vector3 Attenuation { get; set; }
        public void Render();
    }
    class LightObject : ILightObject
    {
        private Transform Transform = new Transform();
        public static Mesh<Simple3D> LightMesh = Mesh<Simple3D>.ReadFrom("Resources/Meshes/Sphere.obj", (p,n,t) => new Simple3D(p));
        public Vector3 Position { get => Transform.Position; set => Transform.Position = value; }
        public float AmbientIntensity { get; set; }
        public Vector3 Colour { get; set; }
        public float DiffuseIntensity { get; set; }
        public Vector3 Attenuation { get; set; }

        public LightObject(Vector3 Position, Vector3 Colour, float DiffuseIntensity = 0.2f, float AmbientIntensity = 0, float CurveExp = 0.3f, float CurveLin = 0, float CurveCon = 0)
        {
            this.Colour = Colour;
            this.Position = Position;
            this.AmbientIntensity = AmbientIntensity;
            this.DiffuseIntensity = DiffuseIntensity;
            this.Attenuation = new Vector3(CurveCon, CurveLin, CurveExp);
            Transform.Scale = new Vector3(CalcScale(Colour, CurveExp, CurveLin, CurveCon, DiffuseIntensity));
        }
        public void Render() 
        {
            

        }

        /// <summary>
        /// solves quadratic to find light scale
        /// </summary>
        private static float CalcScale(Vector3 Colour, float CurveExp, float CurveLin, float CurveCon, float DIntensity)
        {
            float MaxChannel = MathF.Max(MathF.Max(Colour.X, Colour.Y), Colour.Z);
            return (-CurveLin + MathF.Sqrt(CurveLin * CurveLin - 4 * CurveExp * (CurveCon - 256 * MaxChannel * DIntensity))) / 2 / CurveExp;
        }
    }
}
