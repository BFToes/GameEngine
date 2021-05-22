using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Graphics.Shaders;
using Graphics.Resources;
namespace Graphics.SceneObject
{
    interface ILightObject
    {
        public void Render();
    }
    class PointLight : ILightObject
    {
        private Transform Transform = new Transform();
        private UniformBlock LightBlock;

        public static Mesh<Simple3D> LightMesh = Mesh<Simple3D>.ReadFrom("Resources/Meshes/Sphere.obj", (p,n,t) => new Simple3D(p));

        private Vector3 colour;
        private Vector3 acurve;
        private float aintensity;
        private float dintensity;

        public Vector3 Position { get => Transform.Position; set => Transform.Position = value; }
        public Vector3 Colour 
        {
            get => colour;
            set
            {
                colour = value;
                LightBlock.Set(0, value);
                Transform.Scale = new Vector3(CalcDistance(colour, acurve, dintensity));
            }
        }
        public float AmbientIntensity 
        {
            get => aintensity;
            set => LightBlock.Set(16, value);
        }
        public float DiffuseIntensity 
        {
            get => dintensity;
            set
            {
                dintensity = value;
                LightBlock.Set(28, value);
                Transform.Scale = new Vector3(CalcDistance(colour, acurve, dintensity));
            }
        }
        public Vector3 Attenuation 
        {
            get => acurve;
            set
            {
                acurve = value;
                LightBlock.Set(32, value);
                Transform.Scale = new Vector3(CalcDistance(colour, acurve, dintensity));
            }
        }

        public PointLight(Vector3 Position, Vector3 Colour, float DiffuseIntensity = 0.2f, float AmbientIntensity = 0, float CurveExp = 0.3f, float CurveLin = 0, float CurveCon = 0)
        {
            this.Position = Position;
            colour = Colour;
            aintensity = AmbientIntensity;
            dintensity = DiffuseIntensity; 
            acurve = new Vector3(CurveCon, CurveLin, CurveExp); 
            Transform.Scale = new Vector3(CalcDistance(colour, acurve, dintensity));

            LightBlock = UniformBlock.For<LightData>(1);
            LightBlock.Set(new LightData(Transform.Matrix, Position, colour, aintensity, dintensity, acurve));
            float[] F = LightBlock.Get(28);
            LightData D = LightBlock.Get<LightData>();
        }
        public void Render() 
        {
            LightBlock.Bind();
            LightMesh.Render();
            //Scene.ScreenMesh.Render();
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
