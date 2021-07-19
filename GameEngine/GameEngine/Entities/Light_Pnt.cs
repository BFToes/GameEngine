using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using GameEngine.Resources.Shaders;
using GameEngine.Resources;
using GameEngine.Entities.Culling;
using GameEngine.Entities.Lighting;
using GameEngine.Geometry.Transform;
using System.Linq;

namespace GameEngine.Entities
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

        ShaderProgram IVolumeLight.ShadowProgram => ShadowProgram;
        ShaderProgram IVolumeLight.LightProgram => LightProgram;
        UniformBlock IVolumeLight.LightBlock => LightBlock;
        Mesh IVolumeLight.LightMesh => Mesh.SimpleSphere;

        static Light_Pnt()
        {
            // because this is an inherited static constructor it will get called on first use of the object
            Attenuation = new float[] {1f, 0.333f, 0.142f, 0.076f, 0.047f, 0.023f, 0.012f, 0 }; //{ 200f, 0f };//{ 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f };//
            ShadowProgram.SetUniformBlock("CameraBlock", 0);
            ShadowProgram.SetUniformBlock("LightBlock", 1);
            LightProgram.SetUniformBlock("CameraBlock", 0);
            LightProgram.SetUniformBlock("LightBlock", 1);

            IVolumeLight.SetNormalTexture += (Tex) => LightProgram.SetUniformSampler("NormalTexture", Tex);
            IVolumeLight.SetPositionTexture += (Tex) => LightProgram.SetUniformSampler("PositionTexture", Tex);
            IVolumeLight.SetAlbedoTexture += (Tex) => LightProgram.SetUniformSampler("AlbedoTexture", Tex);
            IVolumeLight.SetSpecularIntensity += (SI) => LightProgram.SetUniform("SpecularIntensity", SI);
            IVolumeLight.SetSpecularPower += (SP) => LightProgram.SetUniform("SpecularPower", SP);

            LightProgram.SetUniformSampler("NormalTexture", IVolumeLight.NormalTexture);
            LightProgram.SetUniformSampler("PositionTexture", IVolumeLight.PositionTexture);
            LightProgram.SetUniformSampler("AlbedoTexture", IVolumeLight.AlbedoTexture);
            LightProgram.SetUniform("SpecularIntensity", IVolumeLight.SpecularIntensity);
            LightProgram.SetUniform("SpecularPower", IVolumeLight.SpecularPower);
                    
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
                LightProgram.SetUniformSampler("Attenuation", AttenSampler = Texture.Create_Sampler(Value, value.Length, 1, LightSteps ? TextureMinFilter.Nearest : TextureMinFilter.Linear, LightSteps ? TextureMagFilter.Nearest : TextureMagFilter.Linear));
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
        private float aintensity, dintensity;

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

        public Light_Pnt(Vector3 Position, Vector3 Colour, float DIntensity = 1f, float AIntensity = 0.0f, float Scale = 10f) : base(new TransformAligned3D())
        {
            colour = Colour;
            dintensity = DIntensity;
            aintensity = AIntensity;
            Transform.Position = Position;
            Transform.Scale = new Vector3(Scale);
            LightBlock.Set(new PointLightData(Transform.Matrix, Position, colour, aintensity, dintensity));

            Set_WorldMatrix += Update;
        }

        private void Update(Matrix4 M)
        {
            LightBlock.Set(0, M);
            LightBlock.Set(80, WorldPosition);
            CullShape.Update(M);
        }

        void IVolumeLight.UseLight() => IVolumeLight.Use(this);
        void IVolumeLight.Illuminate()
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


        Sphere CullShape = new Sphere(Vector3.Zero, float.MaxValue);
        Sphere ICullable<Sphere>.CullShape => CullShape;
        Sphere ICullObserver<Sphere>.Observer => CullShape;
        public bool Detects(ICullable<Sphere> Entity) => CullShape.Intersect(Entity.CullShape);
        public bool Detects(ICullable<Box> Entity) => CullShape.Intersect(Entity.CullShape);
    }
}