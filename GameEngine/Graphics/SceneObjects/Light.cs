using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Graphics.Shaders;
using Graphics.Resources;
using Graphics.Rendering;

namespace Graphics.SceneObject
{
    abstract class Light
    {
        public static ShaderProgram PntLightProg = ShaderProgram.ReadFrom("Resources/Shaderscripts/Rendering/Light.vert", "Resources/Shaderscripts/Rendering/Light.frag");
        //public static ShaderProgram DirLightProg = ...
        public static ShaderProgram StencilProgram = ShaderProgram.ReadFrom("Resources/Shaderscripts/Rendering/Shadow.vert"); // just needs to cover the stencil buffers
        public static readonly Mesh<Simple3D> PntLightMesh = Mesh.ReadFrom("Resources/Meshes/Sphere.obj", (p, n, t) => new Simple3D(p));

        private static float specularintensity;
        public static float SpecularIntensity
        {
            get => specularintensity;
            set
            {
                PntLightProg.SetUniform("SpecularIntensity", value);
                specularintensity = value;
            }
        }

        private static float specularpower;
        public static float SpecularPower
        {
            get => specularpower;
            set
            {
                PntLightProg.SetUniform("SpecularPower", value);
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
                PntLightProg.SetUniform("Attenuation", value);
            }
        }

        protected UniformBlock LightBlock = UniformBlock.For<LightData>(1);

        static Light()
        {
            SpecularIntensity = 0.5f;
            SpecularPower = 4;
            Attenuation = new Vector3(0.1f, 0.1f, 0);
            PntLightProg.SetUniformBlock("CameraBlock", 0);
            PntLightProg.SetUniformBlock("LightBlock", 1);
            PntLightProg.DebugUniforms();
        }

        public abstract void Render();
    }
    class PointLight : Light
    {
        //private ShadowCube Shadowframebuffer = new ShadowCube(800, 800);

        private Transform Transform = new Transform();
       
        private Vector3 colour;
        private Vector3 acurve;
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
        public float Scale 
        { 
            get => Transform.Scale.X;
            set
            {
                float M = value / Transform.Scale.X ;
                acurve.X /= M * M; // exponent
                acurve.Y /= M; // linear
                Transform.Scale = new Vector3(value);
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
        public override void Render() 
        {
            LightBlock.Bind();
            PntLightProg.Use();
            PntLightMesh.Render();
        }

        /// <summary>
        /// solves quadratic to find light scale
        /// </summary>
        private static float CalcDistance(Vector3 Colour, Vector3 Curve, float DIntensity)
        {
            const float OneOverMin = 64; // 1 / n where n is the smallest value of light thats going to make a difference
            float MaxChannel = MathF.Max(MathF.Max(Colour.X, Colour.Y), Colour.Z);
            
            // linear
            if (Curve.X == 0) return (OneOverMin - Curve.Z) / Curve.Y;

            // quadratic
            float discrim = Curve.Y * Curve.Y - 4 * Curve.X * (Curve.Z - OneOverMin * MaxChannel * DIntensity);
            if (discrim > 0) return (-Curve.Y + MathF.Sqrt(discrim)) / 2 / Curve.X;

            else throw new Exception("Light must degrade with distance so light curve exponent or linear component must be greater than 0");
        }

        /*
        private class ShadowCube : FrameBuffer 
        {
            public readonly int ShadowTexture;

            public ShadowCube(int Width, int Height) : base(Width, Height)
            {
                // this frame buffer draws to no textures and only fills the depth texture
                ShadowTexture = NewTextureCubeAttachment(Width, Height);

                GL.DrawBuffer(DrawBufferMode.None);
                GL.ReadBuffer(ReadBufferMode.None);

                FramebufferErrorCode FrameStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
                if (FrameStatus != FramebufferErrorCode.FramebufferComplete) throw new Exception(FrameStatus.ToString());

                RefreshCol = new Color4(0, 0, 0, 0);
            }
        }
        */
    }

    class DirectionLight : Light
    {

        // reuses light position as light direction
        // ignores attenuation curve
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

        }
        public override void Render()
        {
            LightBlock.Bind();
            //LightProgram.Use();
            //LightMesh.Render();
        }
    }
}
