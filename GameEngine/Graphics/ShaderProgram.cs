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
     * uniform structs support ???
     * setting Uniform TextureUnit
     * 
     * Compiles but nothing shows:
     *      Matrices all 0?
     *      Colour Texture Not pass X
     *      
     * 
     */


    sealed public class ShaderProgram
    {
        public int Handle;
        private Dictionary<string, int> Uniforms;
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

            Uniforms = new Dictionary<string, int>();
            Textures.Fill(-1);
            
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var NumOfUniforms);
            for (int i = 0; i < NumOfUniforms; i++)
            {
                GL.GetActiveUniform(Handle, i, 32, out _, out _, out ActiveUniformType Type, out string Name);
                // for each uniform sampler2D add Texture to unit
                if (Type == ActiveUniformType.Sampler2D) AssignTextureUnit(TextureManager.Texture("Resources/Textures/Missing.png"), out _);
                Uniforms[Name] = i; // add location lookup
            }
        }

        private int LocationLookUp(string Name)
        {
            if (Uniforms.TryGetValue(Name, out int Location)) return Location;
            else return -1;
        }

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


        /// <summary>
        /// Binds Textures to textures associated with this program into texture units
        /// </summary>
        public void Use() 
        {
            GL.UseProgram(Handle); // tell openGL to use this object
            // bind textures into units
            for (int Unit = 0; TexUseCount[Unit] != 0; Unit++) 
                GL.BindTextureUnit(Unit, Textures[Unit]);

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
            if (Index == -1)
            {
                Console.WriteLine($"Tried To Set Uniform '{Name}'. Its either inactive or isnt in this program.");
                return;
            }
            CheckTextureUnitUse(Name, Index);
            AssignTextureUnit(Tex, out int Unit);
            GL.Uniform1(Index, Unit);
            Console.WriteLine($"In program {Handle}, Set Texture {Tex} to Unit {Unit} for Uniform {Name} at Index {Index}");
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
                        
                if (Unit == -1) throw new Exception("Too many textures used on this programs. No Texture Unit Available.");
                Textures[Unit] = newTex;
            }
            
            TexUseCount[Unit]++;
        }
        #endregion
        #region Typed Set Uniform
        #region single value
        // int
        public void SetUniform(string Name, bool Param) { GL.Uniform1(LocationLookUp(Name), Param ? 1 : 0); }
        public void SetUniform(string Name, int Param) { GL.Uniform1(LocationLookUp(Name), Param); }
        public void SetUniform(string Name, Vector2i Param) { GL.Uniform2(LocationLookUp(Name), Param); }
        public void SetUniform(string Name, Vector3i Param) { GL.Uniform3(LocationLookUp(Name), Param); }
        public void SetUniform(string Name, Vector4i Param) { GL.Uniform4(LocationLookUp(Name), Param); }

        //doubles
        public void SetUniform(string Name, double Param) { GL.Uniform1(LocationLookUp(Name), Param); }
        public void SetUniform(string Name, Vector2d Param) { GL.Uniform2(LocationLookUp(Name), Param.X, Param.Y); }
        public void SetUniform(string Name, Vector3d Param) { GL.Uniform3(LocationLookUp(Name), Param.X, Param.Y, Param.Z); }
        public void SetUniform(string Name, Vector4d Param) { GL.Uniform4(LocationLookUp(Name), Param.X, Param.Y, Param.Z, Param.W); }

        // floats
        public void SetUniform(string Name, float Param) { GL.Uniform1(LocationLookUp(Name), Param); }
        public void SetUniform(string Name, Vector2 Param) { GL.Uniform2(LocationLookUp(Name), Param); }
        public void SetUniform(string Name, Vector3 Param) { GL.Uniform3(LocationLookUp(Name), Param); }
        public void SetUniform(string Name, Vector4 Param) { GL.Uniform4(LocationLookUp(Name), Param); }

        //matrices
        public void SetUniform(string Name, Matrix2 Param) { GL.UniformMatrix2(LocationLookUp(Name), 1, false, new float[4] { Param.M11, Param.M12, Param.M21, Param.M22 } ); }
        public void SetUniform(string Name, Matrix3 Param) { GL.UniformMatrix3(LocationLookUp(Name), 1, false, new float[9] { Param.M11, Param.M12, Param.M13, Param.M21, Param.M22, Param.M23, Param.M31, Param.M32, Param.M33, }); }
        public void SetUniform(string Name, Matrix4 Param) { GL.UniformMatrix4(LocationLookUp(Name), 1, false, new float[16] { Param.M11, Param.M12, Param.M13, Param.M14, Param.M21, Param.M22, Param.M23, Param.M24, Param.M31, Param.M32, Param.M33, Param.M34, Param.M41, Param.M42, Param.M43, Param.M44 }); }
        #endregion
        #region array values
        // int
        public void SetUniform(string Name, IEnumerable<bool> ParamArray) { GL.Uniform1(LocationLookUp(Name), ParamArray.Count(), ParamArray.Select((Bl) => Bl? 1:0).ToArray()); }
        public void SetUniform(string Name, IEnumerable<int> ParamArray) { GL.Uniform1(LocationLookUp(Name), ParamArray.Count(), ParamArray.ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector2i> ParamArray) { GL.Uniform2(LocationLookUp(Name), ParamArray.Count(), ParamArray.SelectMany((V) => new int[] { V.X, V.Y }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector3i> ParamArray) { GL.Uniform3(LocationLookUp(Name), ParamArray.Count(), ParamArray.SelectMany((V) => new int[] { V.X, V.Y, V.Z }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector4i> ParamArray) { GL.Uniform4(LocationLookUp(Name), ParamArray.Count(), ParamArray.SelectMany((V) => new int[] { V.X, V.Y, V.Z, V.W }).ToArray()); }

        //doubles
        public void SetUniform(string Name, IEnumerable<double> ParamArray) { GL.Uniform1(LocationLookUp(Name), ParamArray.Count(), ParamArray.ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector2d> ParamArray) { GL.Uniform2(LocationLookUp(Name), ParamArray.Count(), ParamArray.SelectMany((V) => new double[] { V.X, V.Y }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector3d> ParamArray) { GL.Uniform3(LocationLookUp(Name), ParamArray.Count(), ParamArray.SelectMany((V) => new double[] { V.X, V.Y, V.Z }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector4d> ParamArray) { GL.Uniform4(LocationLookUp(Name), ParamArray.Count(), ParamArray.SelectMany((V) => new double[] { V.X, V.Y, V.Z, V.W }).ToArray()); }

        // floats
        public void SetUniform(string Name, IEnumerable<float> ParamArray) { GL.Uniform1(LocationLookUp(Name), ParamArray.Count(), ParamArray.ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector2> ParamArray) { GL.Uniform2(LocationLookUp(Name), ParamArray.Count(), ParamArray.SelectMany((V) => new float[] { V.X, V.Y }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector3> ParamArray) { GL.Uniform3(LocationLookUp(Name), ParamArray.Count(), ParamArray.SelectMany((V) => new float[] { V.X, V.Y, V.Z }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector4> ParamArray) { GL.Uniform4(LocationLookUp(Name), ParamArray.Count(), ParamArray.SelectMany((V) => new float[] { V.X, V.Y, V.Z, V.W }).ToArray()); }

        //matrices
        // these matrice arrays are probably wrong
        public void SetUniform(string Name, IEnumerable<Matrix2> ParamArray) { GL.UniformMatrix2(LocationLookUp(Name), ParamArray.Count(), false, ParamArray.SelectMany((M) => new float[] { M.M11, M.M12, M.M21, M.M22 }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Matrix3> ParamArray) { GL.UniformMatrix3(LocationLookUp(Name), ParamArray.Count(), false, ParamArray.SelectMany((M) => new float[] { M.M11, M.M12, M.M13, M.M21, M.M22, M.M23, M.M31, M.M32, M.M33, }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Matrix4> ParamArray) { GL.UniformMatrix4(LocationLookUp(Name), ParamArray.Count(), false, ParamArray.SelectMany((M) => new float[] { M.M11, M.M12, M.M13, M.M14, M.M21, M.M22, M.M23, M.M24, M.M31, M.M32, M.M33, M.M34 , M.M41, M.M42, M.M43, M.M44 }).ToArray()); }
        #endregion
        #endregion
    }
}
