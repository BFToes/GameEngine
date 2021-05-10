using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using Graphics.Shaders;
using System.Reflection;

namespace Graphics
{


    /// <summary>
    /// An Object that renders onto the screen.
    /// </summary>
    abstract class RenderObject<Vertex> : IRenderable where Vertex : struct, IVertex
    {
        public ShaderProgram Material;
        public ITransform Transform;

        private int VertexArrayHandle; // vertex array object handle
        private int VertexBufferHandle; // vertex buffer object handle
        
        // used in Render() to determine how this object renders
        protected PrimitiveType RenderingType = PrimitiveType.Triangles;
        protected PolygonMode PolygonMode = PolygonMode.Fill;

        private Vertex[] vertexarray;

        private void BaseConstructor(Scene Canvas, Vertex[] Vertices)
        {
            this.Transform = new Transform();


            //Material.SetUniform("Projection", Canvas.Camera.ProjMat);
            //Material.SetUpdatingUniform("View", () => Canvas.Camera.Matrix);

            Material.SetUpdatingUniform("Model", () => Transform.Matrix);
            
            Material.SetUniformBlock("Camera", Canvas.Camera.UniformBlock);
            GL.UniformBlockBinding(Material.Handle, 0, 0);

            // Buffer array is the buffer that stores the vertices. this requires shaderprogram to be initiated because it adds in the shader parameters of the vertices
            Init_BufferArray(out VertexArrayHandle, out VertexBufferHandle, Vertices);
        }
        public RenderObject(Scene Canvas, Vertex[] Vertices, string VertexShader, string FragmentShader)
        {
            this.Material = new ShaderProgram(VertexShader, FragmentShader);
            BaseConstructor(Canvas, Vertices);
            Canvas.Add(this);
        }

        /// <summary>
        /// individual vertices belonging to this render object.
        /// Its best to set vertice in one go as it reduces the number of times it must be bound and new data passed in
        /// </summary>
        public virtual Vertex[] VertexArray
        {
            get => vertexarray;
            set
            {
                vertexarray = value;
                // updates vertex buffer object
                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle); // use this buffer array
                GL.BufferData(BufferTarget.ArrayBuffer, new Vertex().SizeInBytes * vertexarray.Length, vertexarray, BufferUsageHint.StreamDraw); // sets and creates buffer data
            }
        }

        #region Methods
        /// <summary>
        /// recognises a new attribute in an array when passed in from buffer.
        /// </summary>
        /// <param name="Location">The parameter index when delivered to the shader. increments on return.</param>
        /// <param name="ByteLocation">The memory index when delivered to shader. Adds size in bytes of 'T' on return.</param>
        /// <param name="ArrayHandle">The OpenGL Handle ID of Vertex Array Attribute.</param>
        /// <param name="Name">The name of this parameter. Must match shader script.</param>
        /// <typeparam name="T">The type of the attribute. used for the shader program.</typeparam>
        private void LoadBufferAttribute<T>(ref int Location, ref int ByteLocation, int ArrayHandle) where T : unmanaged
        {
            int ByteSize; // the size of this type in bytes
            unsafe { ByteSize = sizeof(T); }

            GL.VertexArrayAttribBinding(ArrayHandle, Location, 0); // generates a new attribute binding to location in vertex buffer array
            GL.EnableVertexArrayAttrib(ArrayHandle, Location); // enables the attribute binding to location
            GL.VertexArrayAttribFormat(ArrayHandle, Location, ByteSize / 4, VertexAttribType.Float, false, ByteLocation); // defines attribute location, ByteSize/4 = FloatSize

            Location++; // increments Location
            ByteLocation += ByteSize; // Adds ByteSize to ByteLocation
        }
        /// <summary>
        /// Creates an array and buffer in openGl such that the data in the vertices can be unpacked.
        /// </summary>
        /// <param name="VAO">The OpenGL Handle ID for the array.</param>
        /// <param name="VBO">The OpenGL Handle ID for the buffer.</param>
        private void Init_BufferArray(out int VAO, out int VBO, Vertex[] Vertices)
        {

            VAO = GL.GenVertexArray(); // generate vertex array object
            VBO = GL.GenBuffer();  // generate vertex buffer object
            GL.BindVertexArray(VAO); // uses this vertex array
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO); // uses this buffer

            //Console.WriteLine(Vertices[0].GetType());

            // add vertex attributes in openGl and Material
            int Location = 0, ByteOffset = 0;
            foreach(FieldInfo Field in new Vertex().GetType().GetFields())
            {
                //Console.WriteLine(Field.ToString());
                switch (Field.FieldType.ToString())
                {
                    case "OpenTK.Mathematics.Vector2": LoadBufferAttribute<Vector2>(ref Location, ref ByteOffset, VAO); break;
                    case "OpenTK.Mathematics.Vector3": LoadBufferAttribute<Vector3>(ref Location, ref ByteOffset, VAO); break;
                    case "OpenTK.Mathematics.Vector4": LoadBufferAttribute<Vector4>(ref Location, ref ByteOffset, VAO); break;
                    case "OpenTK.Mathematics.Color4" : LoadBufferAttribute<Vector4>(ref Location, ref ByteOffset, VAO); break;
                    case "OpenTK.Mathematics.Matrix2": LoadBufferAttribute<Matrix2>(ref Location, ref ByteOffset, VAO); break;
                    case "OpenTK.Mathematics.Matrix3": LoadBufferAttribute<Matrix3>(ref Location, ref ByteOffset, VAO); break;
                    case "OpenTK.Mathematics.Matrix4": LoadBufferAttribute<Matrix4>(ref Location, ref ByteOffset, VAO); break;
                    default: throw new Exception(Field.FieldType.ToString());
                }
            }
            GL.VertexArrayVertexBuffer(VAO, 0, VBO, IntPtr.Zero, new Vertex().SizeInBytes); // assigns vertice data
            VertexArray = Vertices; // sets array attribute to use buffers after buffer has been set
        }
        /// <summary>
        /// Show this object in the viewport
        /// </summary>
        public void Render()
        {
            Material.Use(); // tell openGL to use this objects program
            GL.BindVertexArray(VertexArrayHandle); // use current vertex array
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode); // use this programs rendering modes
            GL.DrawArrays(RenderingType, 0, VertexArray.Length); // draw these vertices in triangles, 0 to the number of vertices
        }
        #endregion
    }
}
