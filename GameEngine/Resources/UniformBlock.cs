using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace GameEngine.Resources
{
    public class UniformBlock
    {
        private readonly int UniformBufferID;
        private readonly int BindingPoint;
        private readonly int SizeInBytes;

        /// <summary>
        /// creates a uniform block with data
        /// </summary>
        public static UniformBlock FromData<T>(int BindingPoint, T Data) where T : struct
        {
            int SizeInBytes = Marshal.SizeOf(typeof(T));
            UniformBlock B = new UniformBlock(BindingPoint, SizeInBytes, 1);
            B.Set(Data);
            return B;
        }
        /// <summary>
        /// creates a uniform block with array data set
        /// </summary>
        public static UniformBlock FromData<T>(int BindingPoint, T[] Data) where T : struct
        {
            int SizeInBytes = Marshal.SizeOf(typeof(T));
            UniformBlock B = new UniformBlock(BindingPoint, SizeInBytes, Data.Length);
            for (int i = 0; i < Data.Length; i++) B.Set(Data[i], i);
            return B;
        }
        /// <summary>
        /// Create Uniform Block To contain struct
        /// </summary>
        public static UniformBlock For<T>(int BindingPoint, int Count = 1) where T : struct
        {
            return new UniformBlock(BindingPoint, Marshal.SizeOf(typeof(T)), Count);
        }

        private UniformBlock(int BindingPoint, int SizeInBytes, int Count)
        {
            this.SizeInBytes = SizeInBytes;
            this.BindingPoint = BindingPoint;
            this.UniformBufferID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, UniformBufferID);
            GL.BufferData(BufferTarget.UniformBuffer, SizeInBytes * Count, (IntPtr)null, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }
        
        
        
        /// <summary>
        /// Set the values in the uniform buffer
        /// </summary>
        public void Set<T>(T Data, int Index = 0) where T : struct
        {
            GL.NamedBufferSubData(UniformBufferID, (IntPtr)(Index * SizeInBytes), SizeInBytes, ref Data);
        }
        /// <summary>
        /// Set indivudual values in the uniform buffer
        /// </summary>
        unsafe public void Set<T>(int Offset, T Data, int Index = 0) where T : struct
        {
            GL.NamedBufferSubData(UniformBufferID, (IntPtr)(Index * SizeInBytes + Offset), Marshal.SizeOf(typeof(T)), ref Data);
        }
        /// <summary>
        /// Gets the value stored in the uniform buffer.
        /// </summary>
        public T Get<T>() where T : struct
        {
            T Data = new T();
            GL.GetNamedBufferSubData(UniformBufferID, IntPtr.Zero, SizeInBytes, ref Data);
            return Data;
        }
        /// <summary>
        /// Gets the value stored in the uniform buffer.
        /// </summary>
        public T Get<T>(int Offset) where T : unmanaged
        {
            T Data = new T();
            GL.GetNamedBufferSubData(UniformBufferID, (IntPtr)Offset, Marshal.SizeOf(typeof(T)), ref Data);
            return Data;
        }

        /// <summary>
        /// Gets the value stored in the uniform buffer.
        /// </summary>
        public float[] Get(int Size)
        {
            float[] Data = new float[Size];
            GL.GetNamedBufferSubData(UniformBufferID, IntPtr.Zero, Size * 4, Data);
            return Data;
        }

        /// <summary>
        /// binds uniform block to its index for use
        /// </summary>
        public void Bind() => GL.BindBufferBase(BufferRangeTarget.UniformBuffer, BindingPoint, UniformBufferID);
    }
}
