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
    public abstract class Light
    {
        #region Debug Fields
        private const bool SHOW_EDGE = false;
        #endregion

        #region
        protected static Action<int> SetNormalTexture = (Tex) => { };
        protected static Action<int> SetAlbedoTexture = (Tex) => { };
        protected static Action<int> SetPositionTexture = (Tex) => { };
        public static int NormalTexture
        {
            get => throw new NotImplementedException();
            set => SetNormalTexture(value);
        }
        public static int AlbedoTexture
        {
            get => throw new NotImplementedException();
            set => SetAlbedoTexture(value);
        }
        public static int PositionTexture
        {
            get => throw new NotImplementedException();
            set => SetPositionTexture(value);
        }

        protected static Action<float> SetSpecularIntensity = (Tex) => { };
        protected static Action<float> SetSpecularPower = (Tex) => { };
        public static float SpecularIntensity
        {
            get => throw new NotImplementedException();
            set => SetSpecularIntensity(value);
        }
        public static float SpecularPower
        {
            get => throw new NotImplementedException();
            set => SetSpecularPower(value);
        }
        #endregion

        public abstract ShaderProgram ShadowProgram { get; }
        public abstract ShaderProgram LightProgram { get; }
        public abstract Mesh LightMesh { get; }

        protected abstract UniformBlock LightBlock { get; }

        public virtual void UseLight() 
        {
            GL.Clear(ClearBufferMask.StencilBufferBit);

            GL.DepthMask(false);
            if (!SHOW_EDGE) GL.ColorMask(false, false, false, false);
            GL.Enable(EnableCap.DepthClamp);

            LightBlock.Bind();
            ShadowProgram.Use();
        }
        public virtual void Illuminate() 
        {
            GL.DepthMask(true);
            GL.ColorMask(true, true, true, true);
            GL.Disable(EnableCap.DepthClamp);

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.StencilFunc(StencilFunction.Equal, 0x0, 0xff);

            LightProgram.Use();
            LightMesh.Draw();

            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
            GL.StencilFunc(StencilFunction.Always, 0, 0xff);

        }
    }
    
}
