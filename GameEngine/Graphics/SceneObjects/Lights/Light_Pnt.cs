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
    class Light_Pnt : SpatialEntity<AbstractTransform3D>, VolumeLight
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
            Attenuation = new Vector3(0.1f, 0.1f, 0);
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

        public Light_Pnt(Vector3 Position, Vector3 Colour, float DiffuseIntensity = 1f, float AmbientIntensity = 0.2f) : base(new Transform3D())
        {
            this.Position = Position;
            colour = Colour;
            aintensity = AmbientIntensity;
            dintensity = DiffuseIntensity;
            Transform.Scale = new Vector3(CalcDistance(Colour, Attenuation, DiffuseIntensity));

            LightBlock.Set(new PointLightData(Transform.Matrix, Position, colour, aintensity, dintensity));
        }

        public void UseLight()
        {
            VolumeLight.Use(this);
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
