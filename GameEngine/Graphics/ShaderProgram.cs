using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ListExtensions;
namespace Graphics.Shaders
{
    /* THINGS TO DO:
     * 
     * uniform block/buffer support for faster setting of uniform values
     * uniform array support [Maybe Done idk]???
     * uniform structs support ???
     * setting Uniform TextureUnit
     * 
     * Uniform Values not being stored???
     *      WHHHHHHHHY...
     *      trying to get the uniform also has this quirky lil' habit of just crashing
     *      
     * 
     */


    sealed public class ShaderProgram
    {
        public int Handle;
        private Dictionary<string, int> UniformLookUp;
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
            // but doesnt actually because theyre still bound to the program
            // when the program gets deleted it these will follow
            // https://stackoverflow.com/questions/18736773/why-it-is-necessary-to-detach-and-delete-shaders-after-creating-them-on-opengl
            GL.DetachShader(Handle, Vert);
            GL.DetachShader(Handle, Frag);
            GL.DeleteShader(Vert);
            GL.DeleteShader(Frag);

            UniformLookUp = new Dictionary<string, int>();
            Textures.Fill(-1);
            
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var NumOfUniforms);
            for (int i = 0; i < NumOfUniforms; i++)
            {
                GL.GetActiveUniform(Handle, i, 32, out _, out _, out ActiveUniformType Type, out string Name);
                // for each uniform sampler2D add Texture to unit
                if (Type == ActiveUniformType.Sampler2D) AssignTextureUnit(TextureManager.Texture("Resources/Textures/Missing.png"), out _);
                UniformLookUp[Name] = i; // add location lookup
            }
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
        /// Uses this program Binds necessaryTextures to textures associated with this program into texture units
        /// </summary>
        /// <param name="TexStartIndex">For optimisation, if texture shared across multiple objects can skip reloading Texture Unit</param>
        /// <param name="BufStartIndex">For optimisation, if Buffer shared across multiple objects can skip setting Buffer Binding Point</param>
        public void Use(int TexStartIndex = 0, int BufStartIndex = 0)
        {
            GL.UseProgram(Handle); // tell openGL to use this object
            for (int Unit = TexStartIndex; TexUseCount[Unit] != 0; Unit++) GL.BindTextureUnit(Unit, Textures[Unit]); // bind textures into units
        }
        #region Set Uniform Functions

        /// <summary>
        /// sets uniform in shader to use uniform block
        /// </summary>
        /// <param name="Name">the name of the uniform block.</param>
        /// <param name="UBO">the Uniform Buffer object's ID</param>
        public void SetUniformBlock<T>(string Name, int UBO)
        {
            var BlockIndex = GL.GetUniformBlockIndex(Handle, Name);
            GL.UniformBlockBinding(Handle, BlockIndex, UBO);
        }

        #region Single Uniforms
        // int
        public void SetUniform(string Name, bool Param) { UniformActive(Name, out int Location); GL.Uniform1(Location, Param ? 1 : 0); }
        public void SetUniform(string Name, int Param) { UniformActive(Name, out int Location); GL.Uniform1(Location, Param); }
        public void SetUniform(string Name, Vector2i Param) { UniformActive(Name, out int Location); GL.Uniform2(Location, Param); }
        public void SetUniform(string Name, Vector3i Param) { UniformActive(Name, out int Location); GL.Uniform3(Location, Param); }
        public void SetUniform(string Name, Vector4i Param) { UniformActive(Name, out int Location); GL.Uniform4(Location, Param); }

        //doubles
        public void SetUniform(string Name, double Param) { UniformActive(Name, out int Location); GL.Uniform1(Location, Param); }
        public void SetUniform(string Name, Vector2d Param) { UniformActive(Name, out int Location); GL.Uniform2(Location, Param.X, Param.Y); }
        public void SetUniform(string Name, Vector3d Param) { UniformActive(Name, out int Location); GL.Uniform3(Location, Param.X, Param.Y, Param.Z); }
        public void SetUniform(string Name, Vector4d Param) { UniformActive(Name, out int Location); GL.Uniform4(Location, Param.X, Param.Y, Param.Z, Param.W); }

        // floats
        public void SetUniform(string Name, float Param) { UniformActive(Name, out int Location); GL.Uniform1(Location, Param); }
        public void SetUniform(string Name, Vector2 Param) { UniformActive(Name, out int Location); GL.Uniform2(Location, Param); }
        public void SetUniform(string Name, Vector3 Param) { UniformActive(Name, out int Location); GL.Uniform3(Location, Param); }
        public void SetUniform(string Name, Vector4 Param) { UniformActive(Name, out int Location); GL.Uniform4(Location, Param); }

        //matrices
        public void SetUniform(string Name, Matrix2 Param) { UniformActive(Name, out int Location); GL.UniformMatrix2(Location, 1, false, new float[4] { Param.M11, Param.M12, Param.M21, Param.M22 } ); }
        public void SetUniform(string Name, Matrix3 Param) { UniformActive(Name, out int Location); GL.UniformMatrix3(Location, 1, false, new float[9] { Param.M11, Param.M12, Param.M13, Param.M21, Param.M22, Param.M23, Param.M31, Param.M32, Param.M33, }); }
        public void SetUniform(string Name, Matrix4 Param) { UniformActive(Name, out int Location); GL.UniformMatrix4(Location, 1, false, new float[16] { Param.M11, Param.M12, Param.M13, Param.M14, Param.M21, Param.M22, Param.M23, Param.M24, Param.M31, Param.M32, Param.M33, Param.M34, Param.M41, Param.M42, Param.M43, Param.M44 }); }
        #endregion
        #region Array Uniforms
        // int
        public void SetUniform(string Name, IEnumerable<bool> ParamArray) { UniformActive(Name, out int Location); GL.Uniform1(Location, ParamArray.Count(), ParamArray.Select((Bl) => Bl? 1:0).ToArray()); }
        public void SetUniform(string Name, IEnumerable<int> ParamArray) { UniformActive(Name, out int Location); GL.Uniform1(Location, ParamArray.Count(), ParamArray.ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector2i> ParamArray) { UniformActive(Name, out int Location); GL.Uniform2(Location, ParamArray.Count(), ParamArray.SelectMany((V) => new int[] { V.X, V.Y }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector3i> ParamArray) { UniformActive(Name, out int Location); GL.Uniform3(Location, ParamArray.Count(), ParamArray.SelectMany((V) => new int[] { V.X, V.Y, V.Z }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector4i> ParamArray) { UniformActive(Name, out int Location); GL.Uniform4(Location, ParamArray.Count(), ParamArray.SelectMany((V) => new int[] { V.X, V.Y, V.Z, V.W }).ToArray()); }

        //doubles
        public void SetUniform(string Name, IEnumerable<double> ParamArray) { UniformActive(Name, out int Location); GL.Uniform1(Location, ParamArray.Count(), ParamArray.ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector2d> ParamArray) { UniformActive(Name, out int Location); GL.Uniform2(Location, ParamArray.Count(), ParamArray.SelectMany((V) => new double[] { V.X, V.Y }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector3d> ParamArray) { UniformActive(Name, out int Location); GL.Uniform3(Location, ParamArray.Count(), ParamArray.SelectMany((V) => new double[] { V.X, V.Y, V.Z }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector4d> ParamArray) { UniformActive(Name, out int Location); GL.Uniform4(Location, ParamArray.Count(), ParamArray.SelectMany((V) => new double[] { V.X, V.Y, V.Z, V.W }).ToArray()); }

        // floats
        public void SetUniform(string Name, IEnumerable<float> ParamArray) { UniformActive(Name, out int Location); GL.Uniform1(Location, ParamArray.Count(), ParamArray.ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector2> ParamArray) { UniformActive(Name, out int Location); GL.Uniform2(Location, ParamArray.Count(), ParamArray.SelectMany((V) => new float[] { V.X, V.Y }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector3> ParamArray) { UniformActive(Name, out int Location); GL.Uniform3(Location, ParamArray.Count(), ParamArray.SelectMany((V) => new float[] { V.X, V.Y, V.Z }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector4> ParamArray) { UniformActive(Name, out int Location); GL.Uniform4(Location, ParamArray.Count(), ParamArray.SelectMany((V) => new float[] { V.X, V.Y, V.Z, V.W }).ToArray()); }

        //matrices
        // these matrice arrays are probably wrong
        public void SetUniform(string Name, IEnumerable<Matrix2> ParamArray) { UniformActive(Name, out int Location); GL.UniformMatrix2(Location, ParamArray.Count(), false, ParamArray.SelectMany((M) => new float[] { M.M11, M.M12, M.M21, M.M22 }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Matrix3> ParamArray) { UniformActive(Name, out int Location); GL.UniformMatrix3(Location, ParamArray.Count(), false, ParamArray.SelectMany((M) => new float[] { M.M11, M.M12, M.M13, M.M21, M.M22, M.M23, M.M31, M.M32, M.M33, }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Matrix4> ParamArray) { UniformActive(Name, out int Location);  GL.UniformMatrix4(Location, ParamArray.Count(), false, ParamArray.SelectMany((M) => new float[] { M.M11, M.M12, M.M13, M.M14, M.M21, M.M22, M.M23, M.M24, M.M31, M.M32, M.M33, M.M34 , M.M41, M.M42, M.M43, M.M44 }).ToArray()); }
        #endregion
        #region Sampler Uniforms

        public void SetUniformSampler2D(string Name, string TexPath)
        {
            int Tex = TextureManager.Texture(TexPath);
            SetUniformSampler2D(Name, Tex);
        }
        public void SetUniformSampler2D(string Name, int Tex)
        {
            if (!UniformActive(Name, out int Index)) return;

            UnAssignsTextureUnit(Name, Index);
            AssignTextureUnit(Tex, out int Unit);
            
            GL.Uniform1(Index, Unit);
            Console.WriteLine($"In program {Handle}, Set Texture {Tex} to Unit {Unit} for Uniform {Name} at Index {Index}");
        }
        /// <summary>
        /// If texture not used for this unit, allows it to be re assigned.
        /// </summary>
        /// <param name="Name">the name of the uniform</param>
        /// <param name="Index">Index of texture thats being unassigned</param>
        private void UnAssignsTextureUnit(string Name, int Index)
        {
            GL.GetUniform(Handle, Index, out int Unit);
            TexUseCount[Unit]--;
            if (TexUseCount[Unit] == 0)
                Textures[Unit] = -1;
        }
        /// <summary>
        /// assigns Texture to unit only if not already assigned to previous unit.
        /// </summary>
        /// <param name="Tex">The new texture.</param>
        /// <param name="Unit">The unit the texture is assigned to.</param>
        private void AssignTextureUnit(int Tex, out int Unit)
        {
            if (Textures.Contains(Tex))
            {
                Unit = -1;
                for (int i = 0; i < 32; i++)
                    if (Textures[i] == Tex)
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
                Textures[Unit] = Tex;
            }

            TexUseCount[Unit]++;
        }
        #endregion
        /// <summary>
        /// Finds the location of the uniform by Name.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns>the location of the Uniform</returns>
        private bool UniformActive(string Name, out int Location) => UniformLookUp.TryGetValue(Name, out Location);

        #endregion
    }

}
