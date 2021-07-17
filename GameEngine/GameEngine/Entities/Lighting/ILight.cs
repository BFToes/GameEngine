using GameEngine.Resources;
using GameEngine.Resources.Shaders;
using System;

namespace GameEngine.Entities.Lighting
{
    interface ILight
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
    
    
}
