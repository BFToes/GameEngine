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
        public int SizeInBytes => 128;
        public CameraData(Matrix4 Proj, Matrix4 View)
        {
            this.Projection = Proj;
            this.View = View;
        }
    }
    
}
