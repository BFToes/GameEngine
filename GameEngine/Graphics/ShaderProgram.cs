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
     * uniform structs support ???
     * setting Uniform TextureUnit [DONE]
     * set get standard uniform [DONE]
     * uniform array support [Maybe Done idk]???
     * 
     * This Real fuckin messy 
     * 
     */

    sealed public class ShaderProgram
    {
        public int Handle { get; private set; }

        private Dictionary<string, int> UniformLocation;
        private Dictionary<string, int> UniformBlockLocation;
        private Dictionary<string, Func<dynamic>> UpdatingUniforms;
        private int[] Textures = new int[32].Fill(-1);
        private int[] TexUseCount = new int[32].Fill(0);


        public ShaderProgram(string vertexpath, string fragmentpath)
        {
            UniformLocation = new Dictionary<string, int>(); // for uniform location look up
            UniformBlockLocation = new Dictionary<string, int>(); // for uniform location look up
            UpdatingUniforms = new Dictionary<string, Func<dynamic>>(); // for updating uniforms
            
            Compile(new Dictionary<ShaderType, string>()
            {
                { ShaderType.FragmentShader, fragmentpath },
                { ShaderType.VertexShader, vertexpath },
            });
        }
        
        
        /// <summary>
        /// Uses this program Binds necessaryTextures to textures associated with this program into texture units
        /// </summary>
        /// <param name="TexStartIndex">For optimisation, if texture shared across multiple objects can skip reloading Texture Unit</param>
        /// <param name="BufStartIndex">For optimisation, if Buffer shared across multiple objects can skip setting Buffer Binding Point</param>
        public void Use(int TexStartIndex = 0)
        {
            GL.UseProgram(Handle); // tell openGL to use this object

            for(int Unit = TexStartIndex; TexUseCount[Unit] != 0; Unit++) 
                GL.BindTextureUnit(Unit, Textures[Unit]); // bind textures into units

            foreach (string Uniform in UpdatingUniforms.Keys) 
                SetUniform(Uniform, UpdatingUniforms[Uniform]());
        }

        #region Set Uniform Functions
        #region Updating Uniform
        /// <summary>
        /// Updates Uniform before Program Use. To remove updating uniform set Getter to null.
        /// </summary>
        /// <param name="Name">the name of the uniform.</param>
        /// <param name="UniformGetter">a function to get the uniform</param>
        public void SetUpdatingUniform(string Name, Func<dynamic>? UniformGetter) => UpdatingUniforms[Name] = UniformGetter;
        #endregion
        #region Block Uniform
        /// <summary>
        /// Binds buffer to Uniform block
        /// </summary>
        /// <param name="Name">the name of the block</param>
        /// <param name="BlockBinding">the ID of the buffer</param>
        public void SetUniformBlock(string Name, int BlockBinding) 
        {
            if (UniformBlockLocation.TryGetValue(Name, out int Location))
                GL.UniformBlockBinding(Handle, Location, BlockBinding);
            /*
            else 
                throw new Exception($"Uniform BLock {Name} Does not appear in Program {Handle}");
            */
        }
        #endregion
        #region Single Uniforms
        // int & bool
        public void SetUniform(string Name, bool Param) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform1(Handle, Location, Param ? 1 : 0); }
        public void SetUniform(string Name, int Param) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform1(Handle, Location, Param); }
        public void SetUniform(string Name, Vector2i Param) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform2(Handle, Location, Param); }
        public void SetUniform(string Name, Vector3i Param) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform3(Handle, Location, Param); }
        public void SetUniform(string Name, Vector4i Param) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform4(Handle, Location, Param); }

        //doubles
        public void SetUniform(string Name, double Param) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform1(Handle, Location, Param); }
        public void SetUniform(string Name, Vector2d Param) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform2(Handle, Location, Param.X, Param.Y); }
        public void SetUniform(string Name, Vector3d Param) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform3(Handle, Location, Param.X, Param.Y, Param.Z); }
        public void SetUniform(string Name, Vector4d Param) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform4(Handle, Location, Param.X, Param.Y, Param.Z, Param.W); }

        // floats
        public void SetUniform(string Name, float Param) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform1(Handle, Location, Param); }
        public void SetUniform(string Name, Vector2 Param) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform2(Handle, Location, Param); }
        public void SetUniform(string Name, Vector3 Param) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform3(Handle, Location, Param); }
        public void SetUniform(string Name, Vector4 Param) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform4(Handle, Location, Param); }

        //matrices
        public void SetUniform(string Name, Matrix2 Param) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniformMatrix2(Handle, Location, 1, false, new float[4] { Param.M11, Param.M12, Param.M21, Param.M22 }); }
        public void SetUniform(string Name, Matrix3 Param) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniformMatrix3(Handle, Location, 1, false, new float[9] { Param.M11, Param.M12, Param.M13, Param.M21, Param.M22, Param.M23, Param.M31, Param.M32, Param.M33, }); }
        public void SetUniform(string Name, Matrix4 Param) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniformMatrix4(Handle, Location, 1, false, new float[16] { Param.M11, Param.M12, Param.M13, Param.M14, Param.M21, Param.M22, Param.M23, Param.M24, Param.M31, Param.M32, Param.M33, Param.M34, Param.M41, Param.M42, Param.M43, Param.M44 }); }
        #endregion
        #region Array Uniforms
        // int
        public void SetUniform(string Name, IEnumerable<bool> ParamArray) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform1(Handle, Location, ParamArray.Count(), ParamArray.Select((Bl) => Bl ? 1 : 0).ToArray()); }
        public void SetUniform(string Name, IEnumerable<int> ParamArray) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform1(Handle, Location, ParamArray.Count(), ParamArray.ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector2i> ParamArray) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform2(Handle, Location, ParamArray.Count(), ParamArray.SelectMany((V) => new int[] { V.X, V.Y }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector3i> ParamArray) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform3(Handle, Location, ParamArray.Count(), ParamArray.SelectMany((V) => new int[] { V.X, V.Y, V.Z }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector4i> ParamArray) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform4(Handle, Location, ParamArray.Count(), ParamArray.SelectMany((V) => new int[] { V.X, V.Y, V.Z, V.W }).ToArray()); }

        //doubles
        public void SetUniform(string Name, IEnumerable<double> ParamArray) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform1(Handle, Location, ParamArray.Count(), ParamArray.ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector2d> ParamArray) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform2(Handle, Location, ParamArray.Count(), ParamArray.SelectMany((V) => new double[] { V.X, V.Y }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector3d> ParamArray) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform3(Handle, Location, ParamArray.Count(), ParamArray.SelectMany((V) => new double[] { V.X, V.Y, V.Z }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector4d> ParamArray) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform4(Handle, Location, ParamArray.Count(), ParamArray.SelectMany((V) => new double[] { V.X, V.Y, V.Z, V.W }).ToArray()); }

        // floats
        public void SetUniform(string Name, IEnumerable<float> ParamArray) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform1(Handle, Location, ParamArray.Count(), ParamArray.ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector2> ParamArray) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform2(Handle, Location, ParamArray.Count(), ParamArray.SelectMany((V) => new float[] { V.X, V.Y }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector3> ParamArray) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform3(Handle, Location, ParamArray.Count(), ParamArray.SelectMany((V) => new float[] { V.X, V.Y, V.Z }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector4> ParamArray) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniform4(Handle, Location, ParamArray.Count(), ParamArray.SelectMany((V) => new float[] { V.X, V.Y, V.Z, V.W }).ToArray()); }

        //matrices
        // these matrice arrays are probably wrong
        public void SetUniform(string Name, IEnumerable<Matrix2> ParamArray) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniformMatrix2(Handle, Location, ParamArray.Count(), false, ParamArray.SelectMany((M) => new float[] { M.M11, M.M12, M.M21, M.M22 }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Matrix3> ParamArray) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniformMatrix3(Handle, Location, ParamArray.Count(), false, ParamArray.SelectMany((M) => new float[] { M.M11, M.M12, M.M13, M.M21, M.M22, M.M23, M.M31, M.M32, M.M33, }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Matrix4> ParamArray) { if (UniformLocation.TryGetValue(Name, out int Location)) GL.ProgramUniformMatrix4(Handle, Location, ParamArray.Count(), false, ParamArray.SelectMany((M) => new float[] { M.M11, M.M12, M.M13, M.M14, M.M21, M.M22, M.M23, M.M24, M.M31, M.M32, M.M33, M.M34, M.M41, M.M42, M.M43, M.M44 }).ToArray()); }
        #endregion
        #region Sampler Uniforms

        /// <summary>
        /// sets The uniform 'Name' to the texture found at 'TexPath'
        /// </summary>
        public void SetUniformSampler2D(string Name, string TexPath)
        {
            int Tex = TextureManager.Texture(TexPath);
            SetUniformSampler2D(Name, Tex);
        }

        /// <summary>
        /// sets the uniform 'Name' to the Texture ID 'Tex'
        /// </summary>
        public void SetUniformSampler2D(string Name, int Tex)
        {
            if (!UniformLocation.TryGetValue(Name, out int Location)) return;
            UnAssignsTextureUnit(Name, Location);
            AssignTextureUnit(Tex, out int Unit);
            GL.ProgramUniform1(Handle, Location, Unit);
        }

        /// <summary>
        /// If texture not used for this unit, allows it to be re assigned.
        /// </summary>
        /// <param name="Name">the name of the uniform</param>
        /// <param name="Index">Index of texture thats being unassigned</param>
        private void UnAssignsTextureUnit(string Name, int Index)
        {
            GL.GetUniform(Handle, Index, out int Unit); // read uniform
            if (TexUseCount[Unit]-- == 0) // deincrement UnitUseCount, if Texture no longer in use
                Textures[Unit] = -1; // remove texture from unit
        }

        /// <summary>
        /// assigns Texture to unit only if not already assigned to previous unit.
        /// </summary>
        /// <param name="Tex">The new texture.</param>
        /// <param name="Unit">The unit the texture is assigned to.</param>
        private void AssignTextureUnit(int Tex, out int Unit)
        {
            Unit = -1;
            if (Textures.Contains(Tex)) // if texture already bound
            {
                while (Textures[++Unit] != Tex) ; // linear search for texture unit
            }  
            else // if texture is not already bound
            {
                while(TexUseCount[++Unit] != 0); // linear search for unused unit
                Textures[Unit] = Tex;
            }

            TexUseCount[Unit]++;
        }
        #endregion
        #endregion

        #region Compile Functions
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
            if (!string.IsNullOrWhiteSpace(info)) throw new Exception($"{Type} failed to compile. Go fix your code numb nuts.\n\n{info}");
            
            GL.AttachShader(Handle, NewShader);
            return NewShader;
        }
        /// <summary>
        /// Compiles shaders in openGL
        /// </summary>
        private void Compile(Dictionary<ShaderType, string> Shaders)
        {
            // creates new program
            Handle = GL.CreateProgram();
            int[] ShaderIDs = Shaders.Select(s => LoadShader(s.Key, s.Value)).ToArray();
            GL.LinkProgram(Handle);

            if (!string.IsNullOrWhiteSpace(GL.GetProgramInfoLog(Handle))) // check for error linking shaders to program
                throw new Exception($"Program failed to compile.{GL.GetProgramInfoLog(Handle)}");

            foreach(int Shader in ShaderIDs)
            {
                GL.DetachShader(Handle, Shader);
                GL.DeleteShader(Shader);
            }

            Console.WriteLine($"Program {Handle}");
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var NumOfUniforms);
            for (int Location = 0; Location < NumOfUniforms; Location++)
            {
                // 32 characters is maximum length of Name
                // Because. I could make it more but no, I dont think I will.
                GL.GetActiveUniform(Handle, Location, 32, out _, out _, out ActiveUniformType Type, out string Name);
                UniformLocation[Name] = Location; // add location lookup
                // for each uniform sampler2D add default texture to unit
                if (Type == ActiveUniformType.Sampler2D) 
                    AssignTextureUnit(TextureManager.Texture("Resources/Textures/Missing.png"), out _);
                Console.WriteLine($"Uniform - {Location}: {Type} {Name}");
            }

            // same as above but for uniform blocks blocks
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniformBlocks, out var NumOfUniformBlocks);
            for (int Location = 0; Location < NumOfUniformBlocks; Location++)
            { 
                GL.GetActiveUniformBlockName(Handle, Location, 32, out _, out string Name);
                GL.GetActiveUniformBlock(Handle, Location, ActiveUniformBlockParameter.UniformBlockBinding, out int Binding);
                UniformBlockLocation[Name] = Location;
                Console.WriteLine($"Uniform Block - {Location}: {Name} -> {Binding}");
            }
        }
        #endregion
    }
}
