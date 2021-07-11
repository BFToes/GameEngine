using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Graphics.Shaders;
using Graphics.Resources;
using Graphics.Rendering;
using System.Linq;
namespace Graphics.Entities
    {
        class Light_Pnt : SpatialEntity<TransformAligned3D>, IVolumeLight
        {
            #region Inherited Light Setup
            private static readonly ShaderProgram ShadowProgram = ShaderProgram.ReadFrom(
                "Resources/Shaderscripts/Rendering/Shadow/Shadow.vert",
                "Resources/Shaderscripts/Rendering/Shadow/Shadow_Point.geom",
                "Resources/Shaderscripts/Rendering/Shadow/Shadow.frag");
            private static readonly ShaderProgram LightProgram = ShaderProgram.ReadFrom(
                "Resources/Shaderscripts/Rendering/Light/Light_Point.vert",
                "Resources/Shaderscripts/Rendering/Light/Light_Point.frag");
            private static readonly ShaderProgram ShowMeshProgram = ShaderProgram.ReadFrom(
                "Resources/Shaderscripts/Rendering/Light/Light_Point.vert",
                "Resources/Shaderscripts/Rendering/Light/Light_Debug.frag");
            protected readonly UniformBlock LightBlock = UniformBlock.For<PointLightData>(1);

            ShaderProgram ILight.ShadowProgram => ShadowProgram;
            ShaderProgram ILight.LightProgram => LightProgram;
            UniformBlock ILight.LightBlock => LightBlock;
            Mesh ILight.LightMesh => Mesh.SimpleSphere;

            static Light_Pnt()
            {
                // because this is an inherited static constructor it will get called on first use of the object
                Attenuation = new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f };//, 0.333f, 0.142f, 0.076f, 0.047f, 0.023f, 0.012f, 0 }; //{ 200f, 0f };//
                ShadowProgram.SetUniformBlock("CameraBlock", 0);
                ShadowProgram.SetUniformBlock("LightBlock", 1);
                LightProgram.SetUniformBlock("CameraBlock", 0);
                LightProgram.SetUniformBlock("LightBlock", 1);

                ILight.SetNormalTexture += (Tex) => LightProgram.SetUniformSampler("NormalTexture", Tex);
                ILight.SetPositionTexture += (Tex) => LightProgram.SetUniformSampler("PositionTexture", Tex);
                ILight.SetAlbedoTexture += (Tex) => LightProgram.SetUniformSampler("AlbedoTexture", Tex);
                ILight.SetSpecularIntensity += (SI) => LightProgram.SetUniform("SpecularIntensity", SI);
                ILight.SetSpecularPower += (SP) => LightProgram.SetUniform("SpecularPower", SP);

                LightProgram.SetUniformSampler("NormalTexture", ILight.NormalTexture);
                LightProgram.SetUniformSampler("PositionTexture", ILight.PositionTexture);
                LightProgram.SetUniformSampler("AlbedoTexture", ILight.AlbedoTexture);
                LightProgram.SetUniform("SpecularIntensity", ILight.SpecularIntensity);
                LightProgram.SetUniform("SpecularPower", ILight.SpecularPower);
                    
                // Debug tool
                ShowMeshProgram.SetUniformBlock("CameraBlock", 0);
                ShowMeshProgram.SetUniformBlock("LightBlock", 1);
                ShowMeshProgram.SetUniform("DiffuseColor", Vector3.One);
            }
            private static int AttenSampler;
            private static float[] AttenCurve;
            private static bool lightsteps = false;
            public static float[] Attenuation
            {
                get => AttenCurve;
                set
                {
                    GL.DeleteTexture(AttenSampler);
                    float[] Value = value.AsParallel().SelectMany(f => new float[] { f, 0, 0, 0 }).ToArray();
                    LightProgram.SetUniformSampler("Attenuation", AttenSampler = TextureManager.Create_Sampler(Value, value.Length, 1, LightSteps ? TextureMinFilter.Nearest : TextureMinFilter.Linear, LightSteps ? TextureMagFilter.Nearest : TextureMagFilter.Linear));
                    AttenCurve = value;
                }
            }
            public static bool LightSteps
            {
                get => lightsteps;
                set
                {
                    lightsteps = value;
                    Attenuation = AttenCurve;
                }
            }
            #endregion

            #region Light Settings
            private Vector3 colour;
            private float aintensity;
            private float dintensity;

            public Vector3 Colour
            {
                get => colour;
                set => LightBlock.Set(0, colour = value);
            }
            public float AmbientIntensity
            {
                get => aintensity;
                set => LightBlock.Set(76, aintensity = value);
            }
            public float DiffuseIntensity
            {
                get => dintensity;
                set => LightBlock.Set(92, dintensity = value);
            }
            #endregion

            public Light_Pnt(Vector3 Position, Vector3 Colour, float DIntensity = 0.1f, float AIntensity = 0.0f, float Scale = 100f) : base(new TransformAligned3D())
            {
                colour = Colour;
                dintensity = DIntensity;
                aintensity = AIntensity;
                Transform.Position = Position;
                Transform.Scale = new Vector3(Scale);
                LightBlock.Set(new PointLightData(Transform.Matrix, Position, colour, aintensity, dintensity));

                Set_WorldMatrix += (M) => LightBlock.Set(0, M);
                Set_WorldMatrix += (M) => LightBlock.Set(80, WorldPosition);

                LightProgram.DebugUniforms();
            }

            void ILight.UseLight() => IVolumeLight.Use(this);
            void ILight.Illuminate()
            {
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Front);

                IVolumeLight.Illuminate(this);

                GL.Disable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Back);

                if (IVolumeLight.DEBUG_SHOW_LIGHT_MESH)
                {
                    ShowMeshProgram.Use();
                    Mesh.SimpleSphere.Draw(PolygonMode.Line);
                }
            }

            bool CullShape.InView(Observer Observer) => true;
    }
    }
