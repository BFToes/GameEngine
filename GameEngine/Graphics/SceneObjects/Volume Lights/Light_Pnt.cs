using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Graphics.Shaders;
using Graphics.Resources;
using Graphics.Rendering;
namespace Graphics.Entities
{
    class Light_Pnt : SpatialEntity<TransformAligned3D>, VolumeLight
    {
        #region Inherited Light Setup
        private static readonly ShaderProgram ShadowProgram = ShaderProgram.ReadFrom(
            "Resources/Shaderscripts/Rendering/Shadow.vert", 
            "Resources/Shaderscripts/Rendering/Shadow_Point.geom", 
            "Resources/Shaderscripts/Rendering/Shadow.frag");
        private static readonly ShaderProgram LightProgram = ShaderProgram.ReadFrom(
            "Resources/Shaderscripts/Rendering/Light_Point.vert", 
            "Resources/Shaderscripts/Rendering/Light_Point.frag");
        protected readonly UniformBlock LightBlock = UniformBlock.For<PointLightData>(1);

        ShaderProgram Light.ShadowProgram => ShadowProgram;
        ShaderProgram Light.LightProgram => LightProgram;
        UniformBlock Light.LightBlock => LightBlock;
        Mesh Light.LightMesh => Mesh.Sphere;

        static Light_Pnt()
        {
            // because this is an inherited static constructor it will get called on first use of the object
            Attenuation = new Vector3(0.01f, 0.0f, 0);
            ShadowProgram.SetUniformBlock("CameraBlock", 0);
            ShadowProgram.SetUniformBlock("LightBlock", 1);
            LightProgram.SetUniformBlock("CameraBlock", 0);
            LightProgram.SetUniformBlock("LightBlock", 1);

            Light.SetNormalTexture += (Tex) => LightProgram.SetUniformSampler2D("NormalTexture", Tex);
            Light.SetPositionTexture += (Tex) => LightProgram.SetUniformSampler2D("PositionTexture", Tex);
            Light.SetAlbedoTexture += (Tex) => LightProgram.SetUniformSampler2D("AlbedoTexture", Tex);
            Light.SetSpecularIntensity += (SI) => LightProgram.SetUniform("SpecularIntensity", SI);
            Light.SetSpecularPower += (SP) => LightProgram.SetUniform("SpecularPower", SP);

            LightProgram.SetUniformSampler2D("NormalTexture", Light.NormalTexture);
            LightProgram.SetUniformSampler2D("PositionTexture", Light.PositionTexture);
            LightProgram.SetUniformSampler2D("AlbedoTexture", Light.AlbedoTexture);
            LightProgram.SetUniform("SpecularIntensity", Light.SpecularIntensity);
            LightProgram.SetUniform("SpecularPower", Light.SpecularPower);
        }
        private static Vector3 acurve;
        public static Vector3 Attenuation
        {
            get => acurve;
            set
            {
                LightProgram.SetUniform("Attenuation", value);
                acurve = value;
            }
        }
        #endregion

        #region Light Settings
        const float Precision = 64;

        private Vector3 colour;
        private float aintensity;
        private float dintensity;

        public Vector3 Colour
        {
            get => colour;
            set
            {
                colour = value;
                LightBlock.Set(0, value);
                LightBlock.Set(92, CalcIntensity(colour, Attenuation, Transform.Scale));
            }
        }
        public float AmbientIntensity // might delete
        {
            get => aintensity;
            set => LightBlock.Set(76, aintensity = value);
        }
        public float DiffuseIntensity
        {
            get => dintensity;
            set 
            {
                dintensity = value;
                // setting Lightblock unneccessary as its set when transform is changed
                Transform.Scale = CalcScale(Colour, Attenuation, value);
            }
        }
        #endregion

        public Light_Pnt(Vector3 Position, Vector3 Colour, float AmbientIntensity = 0.2f, float DiffuseIntensity = 8f) : base(new TransformAligned3D())
        {
            colour = Colour; 
            Transform.Position = Position;
            dintensity = CalcIntensity(Colour, Attenuation, Transform.Scale);
            LightBlock.Set(new PointLightData(Transform.Matrix, Position, colour, aintensity, dintensity));

            Set_WorldMatrix += (M) => LightBlock.Set(0, M);
            Set_WorldMatrix += (M) => LightBlock.Set(80, WorldPosition);
            Set_WorldMatrix += (M) => LightBlock.Set(92, CalcIntensity(Colour, Attenuation, M.ExtractScale()));
        }
        
        private static float CalcIntensity(Vector3 Color, Vector3 Curve, Vector3 Scale)
        {
            float C = MathF.Max(MathF.Max(Color.X, Color.Y), Color.Z);
            float S = MathF.Max(MathF.Max(Scale.X, Scale.Y), Scale.Z);

            return (Curve.X * S * S + Curve.Y * S + Curve.Z) / Precision;
        }
        private static Vector3 CalcScale(Vector3 Color, Vector3 Curve, float Intensity)
        {
            float MaxChannel = MathF.Max(MathF.Max(Color.X, Color.Y), Color.Z);
            if (Curve.X == 0) 
                return new Vector3((Precision - Curve.Z) / Curve.Y); // linear
            float discrim = Curve.Y * Curve.Y - 4 * Curve.X * (Curve.Z - Precision * MaxChannel * Intensity);
            if (discrim >= 0) 
                return new Vector3((-Curve.Y + MathF.Sqrt(discrim)) / 2 / Curve.X); // quadratic
            else
                throw new Exception("Light attenuation equation unsolveable. try changing the values to actually cross the x axis");
        }
        public void UseLight()
        {
            VolumeLight.Use(this);
        }
        void Light.Illuminate()
        {
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

            VolumeLight.Illuminate(this);

            GL.Disable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
        }
    }
}
