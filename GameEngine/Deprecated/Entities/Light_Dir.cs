using OpenTK.Mathematics;
using GameEngine.Resources.Shaders;
using GameEngine.Resources;
using GameEngine.Entities.Lighting;
using GameEngine.Entities.Culling;
using System;
namespace GameEngine.Entities
{
    [Obsolete]
    class Light_Dir : Entity, IVolumeLight
    {
        #region inherited Light Setup
        private static readonly ShaderProgram shadowprogram = ShaderProgram.ReadFrom(
            "Resources/Shaderscripts/Rendering/Shadow/Shadow.vert",
            "Resources/Shaderscripts/Rendering/Shadow/Shadow_Directional.geom",
            "Resources/Shaderscripts/Rendering/Shadow/Shadow.frag");
        private static readonly ShaderProgram lightprogram = ShaderProgram.ReadFrom(
            "Resources/Shaderscripts/Rendering/Light/Light_Directional.vert",
            "Resources/Shaderscripts/Rendering/Light/Light_Directional.frag");
        private readonly UniformBlock LightBlock = UniformBlock.For<DirectionalLightData>(1);

        ShaderProgram IVolumeLight.ShadowProgram => shadowprogram;
        ShaderProgram IVolumeLight.LightProgram => lightprogram;
        UniformBlock IVolumeLight.LightBlock => LightBlock;
        Mesh IVolumeLight.LightMesh => Mesh.SimpleScreen;

        static Light_Dir()
        {
            shadowprogram.SetUniformBlock("CameraBlock", 0);
            shadowprogram.SetUniformBlock("LightBlock", 1);
            lightprogram.SetUniformBlock("CameraBlock", 0);
            lightprogram.SetUniformBlock("LightBlock", 1);

            IVolumeLight.SetAlbedoTexture += (Tex) => lightprogram.SetUniformSampler("AlbedoTexture", Tex);
            IVolumeLight.SetNormalTexture += (Tex) => lightprogram.SetUniformSampler("NormalTexture", Tex);
            IVolumeLight.SetPositionTexture += (Tex) => lightprogram.SetUniformSampler("PositionTexture", Tex);

            IVolumeLight.SetSpecularIntensity += (SI) => lightprogram.SetUniform("SpecularIntensity", SI);
            IVolumeLight.SetSpecularPower += (SP) => lightprogram.SetUniform("SpecularPower", SP);
        }
        #endregion

        #region Light Settings
        public Vector3 Colour
        {
            get => LightBlock.Get<Vector3>(0);
            set => LightBlock.Set(0, value);
        }
        public float AmbientIntensity
        {
            get => LightBlock.Get<float>(12);
            set => LightBlock.Set(12, value);
        }
        public Vector3 Direction
        {
            get => LightBlock.Get<Vector3>(16);
            set => LightBlock.Set(16, value);
        }
        public float DiffuseIntensity
        {
            get => LightBlock.Get<float>(28);
            set => LightBlock.Set(28, value);
        }
        #endregion

        public Light_Dir(Vector3 Direction, Vector3 Colour, float DiffuseIntensity = 1f, float AmbientIntensity = 0.2f)
        {
            this.Direction = Direction;
            this.Colour = Colour;
            this.AmbientIntensity = AmbientIntensity;
            this.DiffuseIntensity = DiffuseIntensity;
            LightBlock.Set(new DirectionalLightData(Direction, Colour, AmbientIntensity, DiffuseIntensity));
        }

        void IVolumeLight.UseLight() => IVolumeLight.Use(this);
        void IVolumeLight.Illuminate() => IVolumeLight.Illuminate(this);


        Sphere CullShape = new Sphere(Vector3.Zero, 1);
        Sphere ICullable<Sphere>.CullShape => CullShape;
        Sphere ICullObserver<Sphere>.Observer => CullShape;
        public bool Detects(ICullable<Sphere> Entity) => CullShape.Intersect(Entity.CullShape);
        public bool Detects(ICullable<Box> Entity) => CullShape.Intersect(Entity.CullShape);
    }
}
