using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ListExtensions;
namespace Graphics.Shaders
{
    /* THINGS TO DO:
     * 
     * uniform block/buffer support for faster swapping of uniform values
     * uniform array support ???
     *  - Uniform[1,2,3,4][f,i]() Maybe DONE???
     * uniform structs support ???
     * setting Uniform TextureUnit
     * 
     * 
     * 
     */


    sealed public class ShaderProgram
    {
        private int Handle;
        private static int MissingTexture = TextureManager.Texture("Resources/Textures/Missing.png");
        private int[] Textures = new int[32];
        private int[] TexUseCount = new int[32];
        //public ShaderProgram2(string vertexpath, string geometrypath, string fragmentpath) { }
        public ShaderProgram(string vertexpath, string fragmentpath)
        {
            // creates new program
            Handle = GL.CreateProgram();
            // compile new shaders
            int Vert = LoadShader(ShaderType.VertexShader, vertexpath);
            int Frag = LoadShader(ShaderType.FragmentShader, fragmentpath);

            // attach new shaders
            GL.AttachShader(Handle, Vert);
            GL.AttachShader(Handle, Frag);

            // link new shaders
            GL.LinkProgram(Handle);

            // check for error linking shaders to program
            string info = GL.GetProgramInfoLog(Handle);
            if (!string.IsNullOrWhiteSpace(info))
            {
                Console.WriteLine(info);
                throw new Exception("Program failed to compile. Fucked if I know. It's probably the ins and outs things. Basically this is your fuck up not mine.");
            }



            // detach and delete both shaders
            GL.DetachShader(Handle, Vert);
            GL.DetachShader(Handle, Frag);
            GL.DeleteShader(Vert);
            GL.DeleteShader(Frag);

            Textures.Fill(-1);
            

            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var NumOfUniforms);

            for (int i = 0; i < NumOfUniforms; i++)
            {
                GL.GetActiveUniform(
                    Handle, i, 32,
                    out _,
                    out _,
                    out ActiveUniformType Type,
                    out string Name
                );
                if (Type == ActiveUniformType.Sampler2D)
                {
                    AssignTextureUnit(MissingTexture, out int Unit);
                    TexUseCount[Unit]++; // should always be 0
                }
                    
            }
        }
        public void Use() 
        {
            GL.UseProgram(Handle); // tell openGL to use this object
            // bind textures into units
            for (int Unit = 0; TexUseCount[Unit] != 0; Unit++) GL.BindTextureUnit(Unit, Textures[Unit]);


        }
        /// <summary>
        /// sets uniform in shader to use uniform block
        /// </summary>
        /// <param name="Name">the name of the uniform block</param>
        /// <param name="UBO">the Uniform Buffer object's ID</param>
        public void SetUniformBlock<T>(string Name, int UBO)
        {
            var BlockIndex = GL.GetUniformBlockIndex(Handle, Name);
            GL.UniformBlockBinding(Handle, BlockIndex, UBO);
        }
        #region Sampler Uniforms

        public void SetUniformSampler2D(string Name, string TexPath)
        {
            int Tex = TextureManager.Texture(TexPath);
            SetUniformSampler2D(Name, Tex);
        }
        public void SetUniformSampler2D(string Name, int Tex)
        {
            int Index = GL.GetUniformLocation(Handle, Name);
            if (Index == -1) return;
            CheckTextureUnitUse(Name, Index);
            AssignTextureUnit(Tex, out int Unit);
            GL.Uniform1(Index, Unit);
            Console.WriteLine($"Program: {Handle} Assigning Texture {Tex} to Unit {Unit} for Uniform {Name} at Index {Index}");
        }
        private void CheckTextureUnitUse(string Name, int Index)
        {
            GL.GetUniform(Handle, Index, out int Unit);
            TexUseCount[Unit]--;
            if (TexUseCount[Unit] == 0) 
                Textures[Unit] = -1;
        }
        private void AssignTextureUnit(int newTex, out int Unit)
        {
            
            if (Textures.Contains(newTex))
            {
                Unit = -1;
                for (int i = 0; i < 32; i++)
                    if (Textures[i] == newTex)
                    {
                        Unit = i;
                        break;
                    }
            }
            else
            {
                Unit = -1;
                for (int i = 0; i < 32; i++)
                    if (TexUseCount[i] == 0)
                    {
                        Unit = i;
                        break;
                    }
                        
                if (Unit == -1) throw new Exception("");
                Textures[Unit] = newTex;
            }
            
            TexUseCount[Unit]++;
        }
        #endregion
        #region Typed Set Uniform
        #region single value
        // int
        public void SetUniform(string Name, bool Param) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform1(Index, Param ? 1 : 0); }
        public void SetUniform(string Name, int Param) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform1(Index, Param); }
        public void SetUniform(string Name, Vector2i Param) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform2(Index, Param); }
        public void SetUniform(string Name, Vector3i Param) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform3(Index, Param); }
        public void SetUniform(string Name, Vector4i Param) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform4(Index, Param); }

        //doubles
        public void SetUniform(string Name, double Param) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform1(Index, Param); }
        public void SetUniform(string Name, Vector2d Param) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform2(Index, Param.X, Param.Y); }
        public void SetUniform(string Name, Vector3d Param) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform3(Index, Param.X, Param.Y, Param.Z); }
        public void SetUniform(string Name, Vector4d Param) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform4(Index, Param.X, Param.Y, Param.Z, Param.W); }

        // floats
        public void SetUniform(string Name, float Param) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform1(Index, Param); }
        public void SetUniform(string Name, Vector2 Param) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform2(Index, Param); }
        public void SetUniform(string Name, Vector3 Param) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform3(Index, Param); }
        public void SetUniform(string Name, Vector4 Param) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform4(Index, Param); }

        //matrices
        public void SetUniform(string Name, Matrix2 Param) { int Index = GL.GetUniformLocation(Handle, Name); GL.UniformMatrix2(Index, true, ref Param); }
        public void SetUniform(string Name, Matrix3 Param) { int Index = GL.GetUniformLocation(Handle, Name); GL.UniformMatrix3(Index, true, ref Param); }
        public void SetUniform(string Name, Matrix4 Param) { int Index = GL.GetUniformLocation(Handle, Name); GL.UniformMatrix4(Index, false, ref Param); }
        #endregion
        #region array values
        // int
        public void SetUniform(string Name, IEnumerable<bool> ParamArray) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform1(Index, ParamArray.Count(), ParamArray.Select((Bl) => Bl? 1:0).ToArray()); }
        public void SetUniform(string Name, IEnumerable<int> ParamArray) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform1(Index, ParamArray.Count(), ParamArray.ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector2i> ParamArray) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform2(Index, ParamArray.Count(), ParamArray.SelectMany((V) => new int[] { V.X, V.Y }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector3i> ParamArray) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform3(Index, ParamArray.Count(), ParamArray.SelectMany((V) => new int[] { V.X, V.Y, V.Z }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector4i> ParamArray) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform4(Index, ParamArray.Count(), ParamArray.SelectMany((V) => new int[] { V.X, V.Y, V.Z, V.W }).ToArray()); }

        //doubles
        public void SetUniform(string Name, IEnumerable<double> ParamArray) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform1(Index, ParamArray.Count(), ParamArray.ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector2d> ParamArray) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform2(Index, ParamArray.Count(), ParamArray.SelectMany((V) => new double[] { V.X, V.Y }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector3d> ParamArray) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform3(Index, ParamArray.Count(), ParamArray.SelectMany((V) => new double[] { V.X, V.Y, V.Z }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector4d> ParamArray) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform4(Index, ParamArray.Count(), ParamArray.SelectMany((V) => new double[] { V.X, V.Y, V.Z, V.W }).ToArray()); }

        // floats
        public void SetUniform(string Name, IEnumerable<float> ParamArray) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform1(Index, ParamArray.Count(), ParamArray.ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector2> ParamArray) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform2(Index, ParamArray.Count(), ParamArray.SelectMany((V) => new float[] { V.X, V.Y }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector3> ParamArray) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform3(Index, ParamArray.Count(), ParamArray.SelectMany((V) => new float[] { V.X, V.Y, V.Z }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector4> ParamArray) { int Index = GL.GetUniformLocation(Handle, Name); GL.Uniform4(Index, ParamArray.Count(), ParamArray.SelectMany((V) => new float[] { V.X, V.Y, V.Z, V.W }).ToArray()); }

        //matrices
        // these matrice arrays are probably wrong
        public void SetUniform(string Name, IEnumerable<Matrix2> ParamArray) { int Index = GL.GetUniformLocation(Handle, Name); GL.UniformMatrix2(Index, ParamArray.Count(), false, ParamArray.SelectMany((M) => new float[] { M.M11, M.M12, M.M21, M.M22 }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Matrix3> ParamArray) { int Index = GL.GetUniformLocation(Handle, Name); GL.UniformMatrix3(Index, ParamArray.Count(), false, ParamArray.SelectMany((M) => new float[] { M.M11, M.M12, M.M13, M.M21, M.M22, M.M23, M.M31, M.M32, M.M33, }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Matrix4> ParamArray) { int Index = GL.GetUniformLocation(Handle, Name); GL.UniformMatrix4(Index, ParamArray.Count(), false, ParamArray.SelectMany((M) => new float[] { M.M11, M.M12, M.M13, M.M14, M.M21, M.M22, M.M23, M.M24, M.M31, M.M32, M.M33, M.M34 , M.M41, M.M42, M.M43, M.M44 }).ToArray()); }
        #endregion
        #endregion

        /// <summary>
        /// creates a new shader in OpenGl
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private int LoadShader(ShaderType Type, string path)
        {
            int NewShader = GL.CreateShader(Type); // initiate new shader
            string code = File.ReadAllText(path); // get code
            GL.ShaderSource(NewShader, code); // attaches shader and code           
            GL.CompileShader(NewShader); // compiles shader code

            // get info to check for errors
            string info = GL.GetShaderInfoLog(NewShader);
            if (!string.IsNullOrWhiteSpace(info))
            {
                Console.WriteLine(info);
                throw new Exception($"{Type} failed to compile. Go fix your code numb nuts.");
            }

            return NewShader;
        }

        public static implicit operator int(ShaderProgram Program) => Program.Handle;

    }

    public class UniformBlock<TypeStruct>
    {
        private int UBO;

        public UniformBlock(int Program, string Name)
        {
            int BlockIndex = GL.GetUniformBlockIndex(Program, Name);

            // allocate space for the buffer
            GL.GetActiveUniformBlock(Program, BlockIndex, ActiveUniformBlockParameter.UniformBlockDataSize, out int BlockSize);

            //GL.GetActiveUniformBlock(Program, BlockIndex, ActiveUniformBlockParameter.UniformBlockActiveUniformIndices, );

        }
        public UniformBlock(TypeStruct Block) 
        {
            UBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, UBO);
            // Need to work out block size and
            //GL.BufferData(BufferTarget.UniformBuffer, BlockSize, (float[])null, BufferUsageHint.DynamicDraw);

            // Need to work out binding point
            //GL.BindBufferRange(BufferRangeTarget.UniformBuffer, BindingPoint, UBO, 0, BlockSize);

        }
        public UniformBlock(IEnumerable<TypeStruct> Block) 
        {
            UBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, UBO);
            //GL.BufferData(BufferTarget.UniformBuffer, MemoryAllocation, (float[])null, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }
        public void Set(TypeStruct Block) 
        {
            GL.BindBuffer(BufferTarget.UniformBuffer, UBO);
           
        }
        public void Set(IEnumerable<TypeStruct> Block) 
        { 
            
        }
        public static implicit operator int(UniformBlock<TypeStruct> UniformBlock) => UniformBlock.UBO;
    }


    

}
