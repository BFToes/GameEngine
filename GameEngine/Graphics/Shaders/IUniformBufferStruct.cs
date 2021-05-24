using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace Graphics.Shaders
{
    /* To maintain std140 layout all fields must be a multiple of vec4
     * so theyre padded
     * eg vec3 will have a 1 float pad which comes out as 4 bytes
     * 
     * there are more rules than this.. just go look it up.
     */
    public interface IUniformBufferStruct
    {
        public int SizeInBytes { get; }
    }

    [StructLayout(LayoutKind.Explicit)]
    struct CameraData : IUniformBufferStruct
    {
        [FieldOffset(0)]
        public Matrix4 Projection; // + 64
        [FieldOffset(64)]
        public Matrix4 View; // + 64
        [FieldOffset(128)]
        public Vector3 Position; // + 16
        [FieldOffset(144)]
        public Vector2 ScreenSize; // + 8
        public int SizeInBytes => 152;
        public CameraData(Matrix4 Projection, Matrix4 View, Vector2 ScreenSize)
        {
            this.Projection = Projection;
            this.View = View;
            this.Position = new Vector3(View.Column3);
            this.ScreenSize = ScreenSize;
        }
    }
    
    [StructLayout(LayoutKind.Explicit)]
    struct LightData : IUniformBufferStruct
    {
        [FieldOffset(0)]
        Matrix4 Model;

        [FieldOffset(64)]
        Vector3 Colour; // 64 + 12

        [FieldOffset(76)]
        float AmbientIntensity; // 76 + 4

        [FieldOffset(80)]
        Vector3 Position; // 80 + 12

        [FieldOffset(92)]
        float DiffuseIntensity; // 92 + 4

        [FieldOffset(96)]
        Vector3 Attenuation; // 96 + 12
        public int SizeInBytes => 108;
        public LightData(Matrix4 Model, Vector3 Position, Vector3 Colour, float AmbientIntensity, float DiffuseIntensity, Vector3 Attenuation )
        {
            this.Model = Model;
            this.Position = Position;
            this.Colour = Colour;
            this.AmbientIntensity = AmbientIntensity;
            this.DiffuseIntensity = DiffuseIntensity;
            this.Attenuation = Attenuation;
        }
    }

}
