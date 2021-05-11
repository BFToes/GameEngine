using System;
using System.Collections.Generic;
using System.Text;
using Graphics.Shaders;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Graphics
{
    public class UniformBlock
    {
        private int UBO;
        private int BindingPoint;
        private int SizeInBytes;
        private int Length;
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="BindingPoint"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        public static UniformBlock For<T>(int BindingPoint, int Length = 1) where T : struct, IUniformBufferStruct 
            => new UniformBlock(BindingPoint, new T().SizeInBytes, Length);
        private UniformBlock(int BindingPoint, int SizeInBytes, int Length)
        {
            this.SizeInBytes = SizeInBytes;
            this.BindingPoint = BindingPoint;
            this.Length = Length;
            this.UBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, UBO);
            GL.BufferData(BufferTarget.UniformBuffer, SizeInBytes * Length, (IntPtr)null, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }
        public void Set<T>(T Data, int Index = 0) where T : struct, IUniformBufferStruct
        {
            GL.NamedBufferSubData(UBO, (IntPtr)(Index * SizeInBytes), SizeInBytes, ref Data);
        }
        public void Set<T1>(int Offset, int Size, T1 Data, int Index = 0) where T1 : struct
        {
            GL.NamedBufferSubData(UBO, (IntPtr)(Index * SizeInBytes + Offset), Size, ref Data);
            /*
            Matrix4[] M = new Matrix4[2];
            GL.GetNamedBufferSubData(UBO, IntPtr.Zero, SizeInBytes, M);
            */
        }
        public void Bind()
        {
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, BindingPoint, UBO);
        }
    }
}
