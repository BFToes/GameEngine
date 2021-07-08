using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace Graphics.Resources
{


    /* MESH SKELETAL ANIM ->    like squish w armatures an stuff? ASIMP is a word? opgdev has a tutorial
     * MESH NORMALIZATION ->    for bounding box on frustrum culling
     * MESH SIMPLIFICATION ->   for occluder objects.edge colapse algorithm.
     * MESH MANAGEMENT ->       like with sampler2Ds and it would be good to like idk merge that kinda resource
     *                          management. hard to know if this is a good idea.
     */
    public abstract class Mesh
    {
        public int VertexCount { get; private set; }
        protected int VAO; // vertex array
        protected int VBO; // vertex buffer

        #region static Constructors
        /// <summary>
        /// creates mesh from vertex data
        /// </summary>
        public static Mesh<Vert> From<Vert>(Vert[] Data, PrimitiveType Type = PrimitiveType.Triangles) where Vert : struct, IVertex
        {
            Mesh<Vert> M = new Mesh<Vert>(Type); // empty mesh of type vert
            GL.NamedBufferData(M.VBO, new Vert().SizeInBytes * Data.Length, Data, BufferUsageHint.StaticDraw); // add data
            M.VertexCount = Data.Length;
            return M;
        }
        /// <summary>
        /// creates mesh from float array
        /// </summary>
        public static Mesh<Vert> From<Vert>(float[] Data, PrimitiveType Type = PrimitiveType.Triangles) where Vert : struct, IVertex
        {
            Mesh<Vert> M = new Mesh<Vert>(Type); // empty mesh of type vert
            GL.NamedBufferData(M.VBO, 4 * Data.Length, Data, BufferUsageHint.StaticDraw); // add data
            M.VertexCount = Data.Length * 4 / new Vert().SizeInBytes; // number of floats / (SizeInBytes / 4 = size in floats)
            return M;
        }
        /// <summary>
        /// creates mesh from file
        /// </summary>
        public static Mesh<Vert> Construct<Vert>(string path, Func<Vector3, Vector3, Vector2, Vert> VertexBuilder, 
            PrimitiveType Type = PrimitiveType.Triangles, bool Normalize = false, float Simplify = 0)
            where Vert : struct, IVertex
            => From(LoadObj(path, VertexBuilder, Type), Type);
        #endregion

        #region Preset Meshes
        public static Mesh<Simple2D> Screen = From<Simple2D>(new float[8] { -1, -1, 1, -1, 1, 1, -1, 1 }, PrimitiveType.TriangleFan);
        public static Mesh<Simple3D> Sphere = Construct("Resources/Meshes/Sphere.obj", (p, n, t) => new Simple3D(p));
        public static Mesh<Vertex3D> Cube = Construct("Resources/Meshes/Cube.obj", (p, n, t) => new Vertex3D(p, n, t));
        #endregion

        /// <summary>
        /// renders vertex parameters
        /// </summary>
        public abstract void Draw(PolygonMode RenderMode = PolygonMode.Fill);
        /// <summary>
        /// binds and set parameters without rendering vertex array
        /// </summary>
        public abstract void Use(PolygonMode RenderMode = PolygonMode.Fill);

        private static Vert[] LoadObj<Vert>(string path, Func<Vector3, Vector3, Vector2, Vert> VertexPacker, PrimitiveType Primitive)
        {
            List<Vert> Vertices = new List<Vert>();
            List<Vector3> Positions = new List<Vector3>(); // there will be one position for every vertice what ever else
            List<Vector2> Texels = new List<Vector2>();
            List<Vector3> Normals = new List<Vector3>();
            List<Vector3i[]> Faces = new List<Vector3i[]>();

            string file = File.ReadAllText(path);
            string[] Lines = file.Split('\n');
            foreach (string Line in Lines)
            {
                string[] Parameters = Line.Split(' ');
                switch (Parameters[0])
                {
                    case "p": // point
                        break;
                    case "v":
                        float x = float.Parse(Parameters[1]);
                        float y = float.Parse(Parameters[2]);
                        float z = float.Parse(Parameters[3]);
                        Positions.Add(new Vector3(x, y, z));
                        break;
                    case "vt":
                        float u = float.Parse(Parameters[1]);
                        float v = float.Parse(Parameters[2]);
                        Texels.Add(new Vector2(u, 1 - v)); // inverts y axis to match texture in this program
                        break;
                    case "vn":
                        float xn = float.Parse(Parameters[1]);
                        float yn = float.Parse(Parameters[2]);
                        float zn = float.Parse(Parameters[3]);
                        Normals.Add(new Vector3(xn, yn, zn));
                        break;
                    case "f":
                        Vector3i[] Face = new Vector3i[Parameters.Length - 1];
                        for (int i = 0; i < Parameters.Length - 1; i++)
                        {
                            string[] Parts = Parameters[i + 1].Split('/');
                            Face[i].X = Parts[0] != "" ? int.Parse(Parts[0]) - 1 : -1;
                            Face[i].Y = Parts[1] != "" ? int.Parse(Parts[1]) - 1 : -1;
                            Face[i].Z = Parts[2] != "" ? int.Parse(Parts[2]) - 1 : -1;
                        }
                        Faces.Add(Face);
                        break;
                }
            }
            switch (Primitive)
            {
                case PrimitiveType.Triangles:
                    foreach (Vector3i[] F in Faces) // for each face
                    {
                        foreach (Vector3i V in F) // for each vertex in face
                        {
                            Vector3 p = V.X != -1 ? Positions[V.X] : new Vector3(0);
                            Vector2 t = V.Y != -1 ? Texels[V.Y] : new Vector2(0);
                            Vector3 n = V.Z != -1 ? Normals[V.Z] : new Vector3(0);
                            Vertices.Add(VertexPacker(p, n, t));
                        }
                    }
                    break;
                case PrimitiveType.TrianglesAdjacency:
                    Dictionary<Tuple<int, int>, int[]> Edge = new Dictionary<Tuple<int, int>, int[]>();
                    Vector3i[] LookUp = new Vector3i[3 * Faces.Count];

                    for (int Fi = 0; Fi < Faces.Count; Fi++) // for each face
                    {
                        
                        for (int Vi = 0; Vi < 3; Vi++) // for each vertex in face
                        {
                            int pi1 = Faces[Fi][Vi].X; // position index
                            int pi2 = Faces[Fi][(Vi + 1) % 3].X;
                            int pi3 = Faces[Fi][(Vi + 2) % 3].X;
                            

                            Tuple<int, int> Key = pi1 > pi2 ? new Tuple<int, int>(pi1, pi2) : new Tuple<int, int>(pi2, pi1);

                            if (Edge.ContainsKey(Key))
                                Edge[Key] = new int[] { Edge[Key][0], pi3 };
                            else
                                Edge[Key] = new int[] { pi3 };

                            LookUp[pi1] = Faces[Fi][Vi];
                        } 
                    }

                    for (int Fi = 0; Fi < Faces.Count; Fi++) // for each face
                    {
                        Vector3i[] Face = Faces[Fi];
                        for (int Vi = 0; Vi < 3; Vi++) // for each vertex in face
                        {
                            int pi1 = Faces[Fi][Vi].X; // position index
                            int pi2 = Faces[Fi][(Vi + 1) % 3].X;
                            int pi3 = Faces[Fi][(Vi + 2) % 3].X;

                            Vector3i V1 = LookUp[pi1];
                            Vertices.Add(VertexPacker(
                                V1.X != -1 ? Positions[V1.X] : new Vector3(),
                                V1.Z != -1 ? Normals[V1.Z] : new Vector3(),
                                V1.Y != -1 ? Texels[V1.Y] : new Vector2()));

                            // find adjacent vertice
                            Vector3i V2;
                            Tuple<int, int> Key = pi1 > pi2 ? new Tuple<int, int>(pi1, pi2) : new Tuple<int, int>(pi2, pi1); 
                            if (Edge[Key][0] == pi3)
                                V2 = LookUp[Edge[Key][1]];
                            else
                                V2 = LookUp[Edge[Key][0]];

                            Vertices.Add(VertexPacker(
                                V2.X != -1 ? Positions[V2.X] : new Vector3(),
                                V2.Z != -1 ? Normals[V2.Z] : new Vector3(),
                                V2.Y != -1 ? Texels[V2.Y] : new Vector2()));
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
            
            return Vertices.ToArray();


        }



    }

    public class Mesh<Vertex> : Mesh where Vertex : struct, IVertex
    {
        protected PrimitiveType RenderType;


        public Mesh(PrimitiveType Type)
        {
            RenderType = Type;
            
            VAO = GL.GenVertexArray(); GL.BindVertexArray(VAO);
            VBO = GL.GenBuffer(); GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            // add vertex attributes in openGl
            int Location = 0, ByteOffset = 0;
            foreach (FieldInfo Field in new Vertex().GetType().GetFields())
            {
                //Console.WriteLine($"Name: {Field.Name} Type: {Field.FieldType}");
                switch (Field.FieldType.Name)
                {
                    case "Float": LoadBufferAttribute<float>(ref Location, ref ByteOffset); break;
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
                    default: throw new NotImplementedException(Field.FieldType.ToString());
                }
            }
            GL.VertexArrayVertexBuffer(VAO, 0, VBO, IntPtr.Zero, new Vertex().SizeInBytes);
        }

        /// <summary>
        /// bind, set parameters and render
        /// </summary>
        public override void Draw(PolygonMode RenderMode = PolygonMode.Fill)
        {
            GL.BindVertexArray(VAO); // use this object's mesh
            GL.PolygonMode(MaterialFace.FrontAndBack, RenderMode); // use this programs rendering modes
            GL.DrawArrays(RenderType, 0, VertexCount); // draw vertices in triangles, 0 to the number of vertices
        }
        /// <summary>
        /// bind and set parameters
        /// </summary>
        public override void Use(PolygonMode RenderMode = PolygonMode.Fill)
        {
            GL.BindVertexArray(VAO); // use this object's mesh
            GL.PolygonMode(MaterialFace.FrontAndBack, RenderMode); // use this programs rendering modes
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
