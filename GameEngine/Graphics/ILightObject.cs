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
        0, 12     vec3 Colour; 
        12, 4     float AmbientIntensity;
        16, 12    vec3 Position;
        28, 4     float DiffuseIntensity;
        32, 12    vec3 Attenuation;
        44 = total
    };  
    */

    interface ILightObject
    {
        public void Render();
    }
    class PointLight : ILightObject
    {
        private Transform Transform = new Transform();
        private UniformBlock LightBlock = UniformBlock.For<LightData>(1); // light gets block binding 1

        public static Mesh<Simple3D> LightMesh = Mesh<Simple3D>.ReadFrom("Resources/Meshes/Sphere.obj", (p,n,t) => new Simple3D(p));

        public Vector3 Position { get => Transform.Position; set => Transform.Position = value; }
        public Vector3 Colour 
        {
            get => LightBlock.Get<Vector3>(0);
            set => LightBlock.Set(0, value); 
        }
        public float AmbientIntensity 
        {
            get => LightBlock.Get<float>(16);
            set => LightBlock.Set(16, value);
        }
        public float DiffuseIntensity 
        {
            get => LightBlock.Get<float>(28);
            set => LightBlock.Set(28, value);
        }
        public Vector3 Attenuation 
        {
            get => LightBlock.Get<Vector3>(32);
            set => LightBlock.Set(32, value);
        }

        public PointLight(Vector3 Position, Vector3 Colour, float DiffuseIntensity = 0.2f, float AmbientIntensity = 0, float CurveExp = 0.3f, float CurveLin = 0, float CurveCon = 0)
        {
            this.Colour = Colour;
            this.Position = Position;
            this.AmbientIntensity = AmbientIntensity;
            this.DiffuseIntensity = DiffuseIntensity;
            this.Attenuation = new Vector3(CurveCon, CurveLin, CurveExp);
            Transform.Scale = new Vector3(CalcDistance(Colour, Attenuation, DiffuseIntensity));
        }
        public void Render() 
        {
            LightBlock.Bind();
            StencilPass();
            LightPass();
        }
        private void StencilPass() 
        { 
            // do something
        }
        private void LightPass() 
        { 
            // do something else
        }

        /// <summary>
        /// solves quadratic to find light scale
        /// </summary>
        private static float CalcDistance(Vector3 Colour, Vector3 Curve, float DIntensity)
        {
            float MaxChannel = MathF.Max(MathF.Max(Colour.X, Colour.Y), Colour.Z);
            return (-Curve.Y + MathF.Sqrt(Curve.Y * Curve.Y - 4 * Curve.Z * (Curve.X - 256 * MaxChannel * DIntensity))) / 2 / Curve.Z;
        }
    }
}
