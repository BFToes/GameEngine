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
    class Light_Directional : Light
    {
        #region inherited Light Setup
        private static readonly ShaderProgram shadowprogram = ShaderProgram.ReadFrom(
            "Resources/Shaderscripts/Rendering/Shadow.vert", 
            "Resources/Shaderscripts/Rendering/Shadow_Directional.geom", 
            "Resources/Shaderscripts/Rendering/Shadow.frag");
        private static readonly ShaderProgram lightprogram = ShaderProgram.ReadFrom(
            "Resources/Shaderscripts/Rendering/Light_Directional.vert", 
            "Resources/Shaderscripts/Rendering/Light_Directional.frag");
        protected readonly UniformBlock lightblock = UniformBlock.For<DirectionalLightData>(1);

        public override ShaderProgram ShadowProgram => shadowprogram;
        public override ShaderProgram LightProgram => lightprogram;
        public override Mesh LightMesh => Mesh.Screen;
        protected override UniformBlock LightBlock => lightblock;
        
        
        static Light_Directional()
        {
            shadowprogram.SetUniformBlock("CameraBlock", 0);
            shadowprogram.SetUniformBlock("LightBlock", 1);
            lightprogram.SetUniformBlock("CameraBlock", 0);
            lightprogram.SetUniformBlock("LightBlock", 1);

            SetAlbedoTexture += (Tex) => lightprogram.SetUniformSampler2D("AlbedoTexture", Tex);
            SetNormalTexture += (Tex) => lightprogram.SetUniformSampler2D("NormalTexture", Tex);
            SetPositionTexture += (Tex) => lightprogram.SetUniformSampler2D("PositionTexture", Tex);
            
            SetSpecularIntensity += (SI) => lightprogram.SetUniform("SpecularIntensity", SI);
            SetSpecularPower += (SP) => lightprogram.SetUniform("SpecularPower", SP);
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

        public Light_Directional(Vector3 Direction, Vector3 Colour, float DiffuseIntensity = 1f, float AmbientIntensity = 0.2f)
        {
            this.Direction = Direction;
            this.Colour = Colour;
            this.AmbientIntensity = AmbientIntensity;
            this.DiffuseIntensity = DiffuseIntensity;
            LightBlock.Set(new DirectionalLightData(Direction, Colour, AmbientIntensity, DiffuseIntensity));
        }
    }
}
