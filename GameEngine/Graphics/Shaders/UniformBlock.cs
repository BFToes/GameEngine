using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace Graphics.Shaders
{
    public class UniformBlock
    {
        private int UniformBuffer;
        private int BindingPoint;
        private int SizeInBytes;

        /// <summary>
        /// creates a uniform block with data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="BindingPoint"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static UniformBlock FromData<T>(int BindingPoint, T Data) where T : struct, IUniformBufferStruct
        {
            UniformBlock B = new UniformBlock(BindingPoint, new T().SizeInBytes, 1);
            B.Set(Data);
            return B;
        }
        /// <summary>
        /// creates a uniform block with array data set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="BindingPoint"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static UniformBlock FromData<T>(int BindingPoint, T[] Data) where T : struct, IUniformBufferStruct
        {
            UniformBlock B = new UniformBlock(BindingPoint, new T().SizeInBytes, Data.Length);
            for(int i = 0; i < Data.Length; i++) B.Set(Data[i], i);
            return B;
        }
        /// <summary>
        /// Create Uniform Block To contain struct
        /// </summary>
        /// <typeparam name="T">the struct uniform block shall contain</typeparam>
        /// <param name="BindingPoint">the block base this buffer shall bind to</param>
        /// <param name="Count">the length of the array of T</param>
        /// <returns></returns>
        public static UniformBlock For<T>(int BindingPoint, int Count = 1) where T : struct, IUniformBufferStruct => new UniformBlock(BindingPoint, new T().SizeInBytes, Count);
        public UniformBlock(int BindingPoint, int SizeInBytes, int Count)
        {
            this.SizeInBytes = SizeInBytes;
            this.BindingPoint = BindingPoint;
            this.UniformBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, UniformBuffer);
            GL.BufferData(BufferTarget.UniformBuffer, SizeInBytes * Count, (IntPtr)null, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }
        /// <summary>
        /// Set the values in the uniform buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Data"></param>
        /// <param name="Index"></param>
        public void Set<T>(T Data, int Index = 0) where T : struct, IUniformBufferStruct => GL.NamedBufferSubData(UniformBuffer, (IntPtr)(Index * SizeInBytes), SizeInBytes, ref Data);
        /// <summary>
        /// Set indivudual values in the uniform buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Offset"></param>
        /// <param name="Data"></param>
        /// <param name="Index"></param>
        unsafe public void Set<T>(int Offset, T Data, int Index = 0) where T : struct => GL.NamedBufferSubData(UniformBuffer, (IntPtr)(Index * SizeInBytes + Offset), Marshal.SizeOf(typeof(T)), ref Data);
        /// <summary>
        /// Gets the value stored in the uniform buffer.
        /// </summary>
        /// <returns></returns>
        public T Get<T>() where T : struct, IUniformBufferStruct
        {
            T Data = new T();
            GL.GetNamedBufferSubData(UniformBuffer, IntPtr.Zero, SizeInBytes, ref Data);
            return Data;
        }
        /// <summary>
        /// Gets the value stored in the uniform buffer.
        /// </summary>
        /// <returns></returns>
        unsafe public T Get<T>(int Offset) where T : unmanaged
        {
            T Data = new T();
            GL.GetNamedBufferSubData(UniformBuffer, (IntPtr)Offset, Marshal.SizeOf(typeof(T)), ref Data);
            return Data;
        }
        public float[] Get(int Size)
        {
            float[] Data = new float[Size];
            GL.GetNamedBufferSubData(UniformBuffer, IntPtr.Zero, Size * 4, Data);
            return Data;
        }

        /// <summary>
        /// binds uniform block to its index for use
        /// </summary>
        public void Bind() => GL.BindBufferBase(BufferRangeTarget.UniformBuffer, BindingPoint, UniformBuffer);
    }
}
