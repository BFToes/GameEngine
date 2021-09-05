using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
namespace GameEngine.Components
{
    struct Mesh
    {
        public readonly int VAO; // vertex array object
        public readonly int VBO; // vertex buffer object
        public readonly AttributesTypes Attributes;
        internal Mesh(int vao, int vbo, AttributesTypes attributes)
        {
            VAO = vao;
            VBO = vbo;
            Attributes = attributes;
        }

        public Mesh(float[] data, AttributesTypes attributes)
        {
            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();
            Attributes = attributes;

            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            int Number = 0, Offset = 0, vertexSize = 0;
            if ((Attributes & AttributesTypes.PositionData) != 0)
                LoadBufferAttribute<Vector3>(ref Number, ref Offset, ref vertexSize);

            if ((Attributes & AttributesTypes.NormalData) != 0)
                LoadBufferAttribute<Vector3>(ref Number, ref Offset, ref vertexSize);

            if ((Attributes & AttributesTypes.UVData) != 0)
                LoadBufferAttribute<Vector2>(ref Number, ref Offset, ref vertexSize);

            if ((Attributes & AttributesTypes.BoneData) != 0)
            {
                LoadBufferAttribute<Vector4i>(ref Number, ref Offset, ref vertexSize);
                LoadBufferAttribute<Vector4>(ref Number, ref Offset, ref vertexSize);
            }

            GL.VertexArrayVertexBuffer(VAO, 0, VBO, IntPtr.Zero, vertexSize);
        }

        private void LoadBufferAttribute<T>(ref int AttributeIndex, ref int AttributeOffset, ref int VertexSize) where T : unmanaged
        {
            int AttributeSize; // the size of this type in bytes
            unsafe { AttributeSize = sizeof(T); }

            GL.VertexArrayAttribBinding(VAO, AttributeIndex, 0);    // generates a new attribute binding to index in vertex buffer array
            GL.EnableVertexArrayAttrib(VAO, AttributeIndex);        // enables the attribute binding to index
            GL.VertexArrayAttribFormat(VAO, AttributeIndex, AttributeSize / 4, VertexAttribType.Float, false, AttributeOffset);

            AttributeIndex++;                           // increments Location
            AttributeOffset += AttributeSize;           // increases offset by size of new attribute
            VertexSize += 16 * (AttributeSize / 16);    // increases vertex size padded to length of 16 bytes
        }

        public enum AttributesTypes
        {
            // means boolean operators work, max 32 in a enum
            PositionData = 0b00000001, // vec3
            NormalData   = 0b00000010, // vec3
            UVData       = 0b00000100, // vec2
            BoneData     = 0b00001000, // ivec4 & vec4
        }
    }
}
