using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Graphics.Shaders;
using Graphics.Resources;
using Graphics.Rendering;
namespace Graphics.SceneObjects
{
    class Light_Point : Light
    {
        #region Inherited Light Setup
        private static readonly ShaderProgram shadowprogram = ShaderProgram.ReadFrom(
            "Resources/Shaderscripts/Rendering/Shadow.vert", 
            "Resources/Shaderscripts/Rendering/Shadow_Point.geom", 
            "Resources/Shaderscripts/Rendering/Shadow.frag");
        private static readonly ShaderProgram lightprogram = ShaderProgram.ReadFrom(
            "Resources/Shaderscripts/Rendering/Light_Point.vert", 
            "Resources/Shaderscripts/Rendering/Light_Point.frag");
        protected readonly UniformBlock lightblock = UniformBlock.For<PointLightData>(1);

        public override ShaderProgram ShadowProgram => shadowprogram;
        public override ShaderProgram LightProgram => lightprogram;
        public override Mesh LightMesh => Mesh.Sphere;
        protected override UniformBlock LightBlock => lightblock;
        
        
        private static Vector3 acurve;
        public static Vector3 Attenuation
        {
            get => acurve;
            set
            {
                lightprogram.SetUniform("Attenuation", value);
                acurve = value;
            }
        }


        static Light_Point()
        {
            // this has a high chance of going wrong because its an inherited static class
            // it means thats its called when first used this is really fucking annoying
            // but it means that both scene lights, if theyre going to be used, must be
            // initiated before scene is set
            Attenuation = new Vector3(0.1f, 0.1f, 0);
            shadowprogram.SetUniformBlock("CameraBlock", 0);
            shadowprogram.SetUniformBlock("LightBlock", 1);
            lightprogram.SetUniformBlock("CameraBlock", 0);
            lightprogram.SetUniformBlock("LightBlock", 1);

            SetNormalTexture += (Tex) => lightprogram.SetUniformSampler2D("NormalTexture", Tex);
            SetPositionTexture += (Tex) => lightprogram.SetUniformSampler2D("PositionTexture", Tex);
            SetAlbedoTexture += (Tex) => lightprogram.SetUniformSampler2D("AlbedoTexture", Tex);

            SetSpecularIntensity += (SI) => lightprogram.SetUniform("SpecularIntensity", SI);
            SetSpecularPower += (SP) => lightprogram.SetUniform("SpecularPower", SP);
        }
        #endregion

        #region Light Settings
        const float Precision = 64;

        private Vector3 colour;
        private float aintensity;
        private float dintensity;

        public Vector3 Position
        {
            get => Transform.Position;
            set
            {
                LightBlock.Set(80, value);
                Transform.Position = value;
                LightBlock.Set(0, Transform.Matrix);
            }
        }
        public Vector3 Colour
        {
            get => colour;
            set
            {
                colour = value;
                LightBlock.Set(0, value);
                Transform.Scale = new Vector3(CalcDistance(colour, Attenuation, dintensity));
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
                Transform.Scale = new Vector3(CalcDistance(colour, Attenuation, dintensity));
            }
        }
        #endregion

        private Transform Transform = new Transform();

        public Light_Point(Vector3 Position, Vector3 Colour, float DiffuseIntensity = 1f, float AmbientIntensity = 0.2f)
        {
            this.Position = Position;
            colour = Colour;
            aintensity = AmbientIntensity;
            dintensity = DiffuseIntensity;
            Transform.Scale = new Vector3(CalcDistance(Colour, Attenuation, DiffuseIntensity));

            LightBlock.Set(new PointLightData(Transform.Matrix, Position, colour, aintensity, dintensity));
        }

        public override void UseLight()
        {
            base.UseLight();
            ShadowProgram.SetUniform("LightPosition", Position);
        }

        private static float CalcDistance(Vector3 Colour, Vector3 Curve, float DIntensity)
        {
            float MaxChannel = MathF.Max(MathF.Max(Colour.X, Colour.Y), Colour.Z);
            if (Curve.X == 0) 
                return (Precision - Curve.Z) / Curve.Y; // linear
            float discrim = Curve.Y * Curve.Y - 4 * Curve.X * (Curve.Z - Precision * MaxChannel * DIntensity);
            if (discrim >= 0) 
                return (-Curve.Y + MathF.Sqrt(discrim)) / 2 / Curve.X; // quadratic
            else 
                throw new Exception("Light must degrade with distance so light curve exponent or linear component must be greater than 0");
        }
        public override void Illuminate()
        {
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);
            
            base.Illuminate();

            GL.Disable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
        }
    }
}
