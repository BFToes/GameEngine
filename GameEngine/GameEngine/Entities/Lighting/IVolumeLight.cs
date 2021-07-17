using OpenTK.Graphics.OpenGL4;

namespace GameEngine.Entities.Lighting
{
    interface IVolumeLight : ILight
    {
        #region Debug Fields
        protected const bool DEBUG_SHOW_SHADOW_VOLUME = false;
        protected const bool DEBUG_SHOW_LIGHT_MESH = false;
        #endregion
        protected static void Use(IVolumeLight Light)
        {
            GL.Clear(ClearBufferMask.StencilBufferBit);
            
            GL.DepthMask(false);
            if (!DEBUG_SHOW_SHADOW_VOLUME) GL.ColorMask(false, false, false, false);
            GL.Enable(EnableCap.PolygonOffsetFill);
            GL.Enable(EnableCap.DepthClamp);

            Light.LightBlock.Bind();
            Light.ShadowProgram.Use();

        }
        protected static void Illuminate(IVolumeLight Light)
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
