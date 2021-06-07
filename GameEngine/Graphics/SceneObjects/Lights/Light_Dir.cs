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
    class Light_Dir : Entity, VolumeLight
    {
        #region inherited Light Setup
        private static readonly ShaderProgram shadowprogram = ShaderProgram.ReadFrom(
            "Resources/Shaderscripts/Rendering/Shadow.vert", 
            "Resources/Shaderscripts/Rendering/Shadow_Directional.geom", 
            "Resources/Shaderscripts/Rendering/Shadow.frag");
        private static readonly ShaderProgram lightprogram = ShaderProgram.ReadFrom(
            "Resources/Shaderscripts/Rendering/Light_Directional.vert", 
            "Resources/Shaderscripts/Rendering/Light_Directional.frag");
        private readonly UniformBlock LightBlock = UniformBlock.For<DirectionalLightData>(1);

        ShaderProgram Light.ShadowProgram => shadowprogram;
        ShaderProgram Light.LightProgram => lightprogram;
        UniformBlock Light.LightBlock => LightBlock;
        Mesh Light.LightMesh => Mesh.Screen;

        static Light_Dir()
        {
            shadowprogram.SetUniformBlock("CameraBlock", 0);
            shadowprogram.SetUniformBlock("LightBlock", 1);
            lightprogram.SetUniformBlock("CameraBlock", 0);
            lightprogram.SetUniformBlock("LightBlock", 1);

            Light.SetAlbedoTexture += (Tex) => lightprogram.SetUniformSampler2D("AlbedoTexture", Tex);
            Light.SetNormalTexture += (Tex) => lightprogram.SetUniformSampler2D("NormalTexture", Tex);
            Light.SetPositionTexture += (Tex) => lightprogram.SetUniformSampler2D("PositionTexture", Tex);

            Light.SetSpecularIntensity += (SI) => lightprogram.SetUniform("SpecularIntensity", SI);
            Light.SetSpecularPower += (SP) => lightprogram.SetUniform("SpecularPower", SP);
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

        public void UseLight() => VolumeLight.Use(this);

        public void Illuminate() => VolumeLight.Illuminate(this);
    }
}
