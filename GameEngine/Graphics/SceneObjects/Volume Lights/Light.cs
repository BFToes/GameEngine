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
    public interface Light
    {       
        #region Static Light Settings
        private static int normaltexture;
        private static int albedotexture;
        private static int positiontexture;
        private static float specularintensity;
        private static float specularpower;

        protected static event Action<int> SetNormalTexture = (Tex) => { normaltexture = Tex; };
        protected static event Action<int> SetAlbedoTexture = (Tex) => { albedotexture = Tex; };
        protected static event Action<int> SetPositionTexture = (Tex) => { positiontexture = Tex; };
        protected static event Action<float> SetSpecularIntensity = (intensity) => { specularintensity = intensity; };
        protected static event Action<float> SetSpecularPower = (power) => { specularpower = power; };
        public static int NormalTexture
        {
            get => normaltexture;
            set => SetNormalTexture(value);
        }
        public static int AlbedoTexture
        {
            get => albedotexture;
            set => SetAlbedoTexture(value);
        }
        public static int PositionTexture
        {
            get => positiontexture;
            set => SetPositionTexture(value);
        }
        public static float SpecularIntensity
        {
            get => specularintensity;
            set => SetSpecularIntensity(value);
        }
        public static float SpecularPower
        {
            get => specularpower;
            set => SetSpecularPower(value);
        }
        #endregion

        public ShaderProgram ShadowProgram { get; }
        public ShaderProgram LightProgram { get; }
        protected UniformBlock LightBlock { get; }
        protected Mesh LightMesh { get; }

        public void UseLight();
        public void Illuminate();
    }
    
    public interface VolumeLight : Light
    {
        #region Debug Fields
        private const bool SHOW_EDGE = false;
        #endregion
        protected static void Use(VolumeLight Light)
        {
            GL.Clear(ClearBufferMask.StencilBufferBit);
            
            GL.DepthMask(false);
            if (!SHOW_EDGE) GL.ColorMask(false, false, false, false);
            GL.Enable(EnableCap.PolygonOffsetFill);
            GL.Enable(EnableCap.DepthClamp);

            Light.LightBlock.Bind();
            Light.ShadowProgram.Use();

        }
        protected static void Illuminate(VolumeLight Light)
        {
            GL.DepthMask(true);
            GL.ColorMask(true, true, true, true);
            GL.Disable(EnableCap.DepthClamp);
            GL.Disable(EnableCap.PolygonOffsetFill);

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.StencilFunc(StencilFunction.Equal, 0x0, 0xff);
            
            Light.LightProgram.Use();
            Light.LightMesh.Draw();

            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
            GL.StencilFunc(StencilFunction.Always, 0, 0xff);
        }
    }
}
