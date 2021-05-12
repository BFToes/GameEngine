﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Graphics.Shaders;
using OpenTK.Graphics.OpenGL4;

namespace Graphics
{
    public class UniformBlock
    {
        private int Handle;
        private int BindingPoint;
        private int SizeInBytes;

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
            this.Handle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, Handle);
            GL.BufferData(BufferTarget.UniformBuffer, SizeInBytes * Count, (IntPtr)null, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }
        /// <summary>
        /// Set the values in the uniform buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Data"></param>
        /// <param name="Index"></param>
        public void Set<T>(T Data, int Index = 0) where T : struct, IUniformBufferStruct => GL.NamedBufferSubData(Handle, (IntPtr)(Index * SizeInBytes), SizeInBytes, ref Data);
        /// <summary>
        /// Set indivudual values in the uniform buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Offset"></param>
        /// <param name="Data"></param>
        /// <param name="Index"></param>
        unsafe public void Set<T>(int Offset, T Data, int Index = 0) where T : struct => GL.NamedBufferSubData(Handle, (IntPtr)(Index * SizeInBytes + Offset), Marshal.SizeOf(typeof(T)), ref Data);
        /// <summary>
        /// Gets the value stored in the uniform buffer.
        /// </summary>
        /// <returns></returns>
        public T[] Get<T>(int Length = 1)where T : struct
        {
            T[] Array;
            GL.GetNamedBufferSubData(Handle, IntPtr.Zero, SizeInBytes, Array = new T[Length]);
            return Array;
        }
        /// <summary>
        /// Gets the value stored in the uniform buffer.
        /// </summary>
        /// <returns></returns>
        public T Get<T>(int Offset, int Size) where T : struct
        {
            T Data = new T();
            GL.GetNamedBufferSubData(Handle, (IntPtr)Offset, Size, ref Data);
            return Data;
        }
        /// <summary>
        /// binds uniform block to its index for use
        /// </summary>
        public void Bind() => GL.BindBufferBase(BufferRangeTarget.UniformBuffer, BindingPoint, Handle);
    }
}