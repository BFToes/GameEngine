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
    abstract class Light
    {
        #region Light Settings
        // bigger number higher precision, powers of 2 preferred
        protected const float Precision = 64;

        private static float specularintensity;
        public static float SpecularIntensity
        {
            get => specularintensity;
            set
            {
                PointLight.LightProgram.SetUniform("SpecularIntensity", value);
                specularintensity = value;
            }
        }

        private static float specularpower;
        public static float SpecularPower
        {
            get => specularpower;
            set
            {
                PointLight.LightProgram.SetUniform("SpecularPower", value);
                specularpower = value;
            }
        }

        private static Vector3 acurve;
        public static Vector3 Attenuation
        {
            get => acurve;
            set
            {
                acurve = value;
                PointLight.LightProgram.SetUniform("Attenuation", value);
            }
        }
        #endregion

        static Light()
        {
            SpecularIntensity = 0.5f;
            SpecularPower = 4;
            Attenuation = new Vector3(0.1f, 0.1f, 0);
            PointLight.LightProgram.SetUniformBlock("CameraBlock", 0);
            PointLight.LightProgram.SetUniformBlock("LightBlock", 1);
            PointLight.LightProgram.DebugUniforms();
        }

        public static void SetUniformSamplers(int Albedo, int Normal, int Position)
        {
            PointLight.LightProgram.SetUniformSampler2D("AlbedoTexture", Albedo);
            PointLight.LightProgram.SetUniformSampler2D("NormalTexture", Normal);
            PointLight.LightProgram.SetUniformSampler2D("PositionTexture", Position);
            PointLight.LightProgram.SetUniformSampler2D("AlbedoTexture", Albedo);
            PointLight.LightProgram.SetUniformSampler2D("NormalTexture", Normal);
            PointLight.LightProgram.SetUniformSampler2D("PositionTexture", Position);
        }
        
        protected UniformBlock LightBlock = UniformBlock.For<LightData>(1);
        public abstract void Illuminate();
        public virtual void BeginShadowPass()
        {
            LightBlock.Bind();


        }
    }
    class PointLight : Light
    {
        public static readonly ShaderProgram LightProgram = ShaderProgram.ReadFrom("Resources/Shaderscripts/Rendering/Light.vert","Resources/Shaderscripts/Rendering/Light.frag");
        public static readonly Mesh<Simple3D> PntLightMesh = Mesh.ReadFrom("Resources/Meshes/Sphere.obj", (p, n, t) => new Simple3D(p));

        private Transform Transform = new Transform();
       
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
        
        public PointLight(Vector3 Position, Vector3 Colour, float DiffuseIntensity = 1f, float AmbientIntensity = 0.2f)
        {
            this.Position = Position;
            colour = Colour;
            aintensity = AmbientIntensity;
            dintensity = DiffuseIntensity;
            Transform.Scale = new Vector3(CalcDistance(Colour, Attenuation, DiffuseIntensity));

            LightBlock.Set(new LightData(Transform.Matrix, Position, colour, aintensity, dintensity));
        }
        public override void Illuminate() 
        {
            LightBlock.Bind();
            LightProgram.Use();
            PntLightMesh.Render();
        }

        /// <summary>
        /// solves quadratic to find light scale
        /// </summary>
        private static float CalcDistance(Vector3 Colour, Vector3 Curve, float DIntensity)
        {
            float MaxChannel = MathF.Max(MathF.Max(Colour.X, Colour.Y), Colour.Z);
            
            // linear
            if (Curve.X == 0) 
                return (Precision - Curve.Z) / Curve.Y;

            // quadratic
            float discrim = Curve.Y * Curve.Y - 4 * Curve.X * (Curve.Z - Precision * MaxChannel * DIntensity);
            if (discrim >= 0) 
                return (-Curve.Y + MathF.Sqrt(discrim)) / 2 / Curve.X;

            else 
                throw new Exception("Light must degrade with distance so light curve exponent or linear component must be greater than 0");
        }
    }
    
    class DirectionLight : Light
    {

        // reuses light position as light direction
        // ignores attenuation curve and transform matrix
        private Vector3 colour;
        private Vector3 direction;
        private float aintensity;
        private float dintensity;
        public Vector3 Direction
        {
            get => direction;
            set
            {
                Direction = value;
                LightBlock.Set(80, value);
            }
        }
        public Vector3 Colour
        {
            get => colour;
            set
            {
                colour = value;
                LightBlock.Set(0, value);
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
            }
        }

        public DirectionLight(Vector3 Direction, Vector3 Colour, float DiffuseIntensity = 1f, float AmbientIntensity = 0.2f)
        {
            this.Direction = Direction;
            this.Colour = Colour;
            this.dintensity = DiffuseIntensity;
            this.aintensity = AmbientIntensity;

        }
        public override void Illuminate()
        {
            LightBlock.Bind();
            //LightProgram.Use();

            //LightMesh.Render();
        }
    }
    
}
