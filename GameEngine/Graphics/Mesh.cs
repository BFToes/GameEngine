using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Graphics
{
    public class Mesh<Vertex> where Vertex : struct, IVertex
    {
        private int VAO;
        private int VBO;
        public int Length { get; private set; }

        public static Mesh<Vert> From<Vert>(float[] Data) where Vert : struct, IVertex
        {
            Mesh<Vert> M = new Mesh<Vert>();
            GL.NamedBufferData(M.VBO, Data.Length, Data, BufferUsageHint.StaticDraw);
            M.Length = Data.Length / new Vert().SizeInBytes;
            return M;
            
        }
        /// <summary>
        /// creates mesh from file
        /// </summary>
        public static Mesh<Vert> From<Vert>(string File) where Vert : struct, IVertex => throw new Exception("Havent implemented loading model from file");
        /// <summary>
        /// creates mesh from vertex data
        /// </summary>
        public static Mesh<Vert> From<Vert>(Vert[] VertexArray) where Vert : struct, IVertex
        {
            Mesh<Vert> M = new Mesh<Vert>();
            M.Set(VertexArray);
            return M;
        }

        public Mesh()
        {
            VAO = GL.GenVertexArray(); GL.BindVertexArray(VAO);
            VBO = GL.GenBuffer(); GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            // add vertex attributes in openGl and Material
            int Location = 0, ByteOffset = 0;
            foreach (FieldInfo Field in new Vertex().GetType().GetFields())
            {

                switch (Field.FieldType.Name)
                {
                    case "Float": LoadBufferAttribute<float>(ref Location, ref ByteOffset); break; // untested
                    case "Vector2": LoadBufferAttribute<Vector2>(ref Location, ref ByteOffset); break;
                    case "Vector3": LoadBufferAttribute<Vector3>(ref Location, ref ByteOffset); break;
                    case "Vector4": LoadBufferAttribute<Vector4>(ref Location, ref ByteOffset); break;

                    case "Vector2i": LoadBufferAttribute<Vector2i>(ref Location, ref ByteOffset); break;
                    case "Vector3i": LoadBufferAttribute<Vector3i>(ref Location, ref ByteOffset); break;
                    case "Vector4i": LoadBufferAttribute<Vector4i>(ref Location, ref ByteOffset); break;

                    case "Color4": LoadBufferAttribute<Vector4>(ref Location, ref ByteOffset); break;
                    case "Matrix2": LoadBufferAttribute<Matrix2>(ref Location, ref ByteOffset); break;
                    case "Matrix3": LoadBufferAttribute<Matrix3>(ref Location, ref ByteOffset); break;
                    case "Matrix4": LoadBufferAttribute<Matrix4>(ref Location, ref ByteOffset); break;
                    default: throw new Exception(Field.FieldType.ToString());
                }
            }
            GL.VertexArrayVertexBuffer(VAO, 0, VBO, IntPtr.Zero, new Vertex().SizeInBytes); // assigns vertice data
        }
        /// <summary>
        /// set vertices data to new 'Data'
        /// </summary>
        /// <param name="Data">new vertex data</param>
        private void Set(Vertex[] Data)
        {
            // updates vertex buffer object
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO); // use this buffer array
            GL.BufferData(BufferTarget.ArrayBuffer, new Vertex().SizeInBytes * Data.Length, Data, BufferUsageHint.StaticDraw);
            Length = Data.Length;
        }
        /// <summary>
        /// reads vertex buffer and return vertex array
        /// </summary>
        /// <returns></returns>
        public Vertex[] Get()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            Vertex[] Data = new Vertex[] { };
            GL.GetBufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, new Vertex().SizeInBytes * Length, Data);
            return Data;
        }

        /// <summary>
        /// recognises a new attribute in an array when passed in from buffer.
        /// </summary>
        /// <param name="Location">The parameter index when delivered to the shader. increments on return.</param>
        /// <param name="ByteLocation">The memory index when delivered to shader. Adds size in bytes of 'T' on return.</param>
        /// <typeparam name="T">The type of the attribute. used for the shader program.</typeparam>
        private void LoadBufferAttribute<T>(ref int Location, ref int ByteLocation) where T : unmanaged
        {
            int ByteSize; // the size of this type in bytes
            unsafe { ByteSize = sizeof(T); }

            GL.VertexArrayAttribBinding(VAO, Location, 0); // generates a new attribute binding to location in vertex buffer array
            GL.EnableVertexArrayAttrib(VAO, Location); // enables the attribute binding to location
            GL.VertexArrayAttribFormat(VAO, Location, ByteSize / 4, VertexAttribType.Float, false, ByteLocation); // defines attribute location, ByteSize/4 = FloatSize

            Location++; // increments Location
            ByteLocation += ByteSize; // Adds ByteSize to ByteLocation
        }

        public static implicit operator int(Mesh<Vertex> VA) => VA.VAO;
    }
}
