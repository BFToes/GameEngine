using System;
using System.Collections.Generic;
using System.Text;
using Graphics.Shaders;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Graphics
{
    public class UniformBuffer<T> where T : struct, UniformBufferStruct
    {
        //static int TotalBufferBases = 0;
        public int UBO { get; private set; }
        public int BindingPoint { get; private set; }
        public int SizeInBytes { get; private set; }
        public UniformBuffer()
        {
            SizeInBytes = new T().SizeInBytes;
            UBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, UBO);
            GL.BufferData(BufferTarget.UniformBuffer, SizeInBytes, (IntPtr)null, BufferUsageHint.StaticDraw);    
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, BindingPoint = 1, UBO); // currently just binds to 0 for testing
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            /*
                +--Init Uniform Buffer Block-------+
                glGenBuffers(1 &ubo);
                glBindBuffer(GL_UNIFORM_BUFFER, ubo);
                glBufferData(GL_UNIFORM_BUFFER, 2 * sizeof(glm::mat4), NULL, GL_STATIC_DRAW); // pre-allocate
                
                GLuint pv1_index = glGetUniformBlockIndex(program1, "PV");   
                glUniformBlockBinding(program1, pv1_index, 0);
                GLuint pv2_index = glGetUniformBlockIndex(program2, "PV");
                glUniformBlockBinding(program2, pv2_index, 0);

                glBindBufferBase(GL_UNIFORM_BUFFER, 0, ubo);
                glBindBuffer(GL_UNIFORM_BUFFER, 0);
                +----------------------------------+
             */
        }
        public void Set(T Data)
        {
            GL.BindBuffer(BufferTarget.UniformBuffer, UBO);
            GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, SizeInBytes, ref Data);
            // This Worked
            //Matrix4[] M = new Matrix4[2];
            //GL.GetBufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, SizeInBytes, M);

            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

        }
        public void Set<T1>(int Offset, int Size, T1 Data) where T1 : struct
        {
            GL.BindBuffer(BufferTarget.UniformBuffer, UBO);
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)Offset, Size, ref Data);

            //Matrix4[] M = new Matrix4[1];
            //GL.GetBufferSubData(BufferTarget.UniformBuffer, (IntPtr)Offset, Size, M);

            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }
        public static implicit operator int(UniformBuffer<T> Block) => Block.BindingPoint;
    }
}
