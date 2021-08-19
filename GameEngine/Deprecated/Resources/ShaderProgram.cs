using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameEngine.Resources
{
    [Obsolete]
    public class ShaderProgram
    {
        private const int MaxUnits = 32;

        public readonly int Handle;
        // uniform management
        private readonly Dictionary<string, int> UniformLocation; // uniform lookup
        private readonly Dictionary<string, int> UniformBlockLocation; // uniform block lookup
        private Dictionary<string, Func<dynamic>> UpdatingUniforms; // uniform updater functions

        
        #region Static Constructors
        public static ShaderProgram From(string vertexcode, string geometrycode, string fragmentcode)
        {
            return new ShaderProgram(new Dictionary<ShaderType, string>()
            {
                { ShaderType.FragmentShader, fragmentcode },
                { ShaderType.GeometryShader, geometrycode},
                { ShaderType.VertexShader, vertexcode },
            });
        }
        public static ShaderProgram From(string vertexcode, string fragmentcode)
        {
            return new ShaderProgram(new Dictionary<ShaderType, string>()
            {
                { ShaderType.FragmentShader, fragmentcode },
                { ShaderType.VertexShader, vertexcode },
            });
        }
        public static ShaderProgram ReadFrom(string vertexpath, string geometrypath, string fragmentpath)
        {
            return new ShaderProgram(new Dictionary<ShaderType, string>()
            {
                { ShaderType.FragmentShader, File.ReadAllText(fragmentpath) },
                { ShaderType.GeometryShader, File.ReadAllText(geometrypath) },
                { ShaderType.VertexShader, File.ReadAllText(vertexpath) },
            });
        }
        public static ShaderProgram ReadFrom(string vertexpath, string fragmentpath)
        {
            return new ShaderProgram(new Dictionary<ShaderType, string>()
            {
                { ShaderType.FragmentShader, File.ReadAllText(fragmentpath) },
                { ShaderType.VertexShader, File.ReadAllText(vertexpath) },
            });
        }
        #endregion

        /// <summary>
        /// Compiles shaders in openGL
        /// </summary>
        public ShaderProgram(Dictionary<ShaderType, string> Shaders)
        {
            // create dictionaries
            UniformLocation = new Dictionary<string, int>(); // for uniform location look up
            UniformBlockLocation = new Dictionary<string, int>(); // for uniform location look up
            UpdatingUniforms = new Dictionary<string, Func<dynamic>>(); // for updating uniforms

            // initiate program
            Handle = GL.CreateProgram();

            // loads each shader and stores IDs in array
            int[] ShaderIDs = Shaders.Select(s => LoadShader(s.Key, s.Value)).ToArray();
            
            // link shaders together
            GL.LinkProgram(Handle);

            // error checking
            if (!string.IsNullOrWhiteSpace(GL.GetProgramInfoLog(Handle))) // check for error linking shaders to program
                throw new Exception($"Program failed to compile.{GL.GetProgramInfoLog(Handle)}");
            
            // deletes object when shader program stops using them
            foreach (int Shader in ShaderIDs)
            {
                GL.DetachShader(Handle, Shader);
                GL.DeleteShader(Shader);
            }

            // iterates over uniforms in program
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var NumOfUniforms);
            for (int i = 0; i < NumOfUniforms; i++)
            {
                // 32 characters is maximum length of Name
                // Because. I could make it more but no, I dont think I will.

                GL.GetActiveUniform(Handle, i, 32, out _, out _, out ActiveUniformType Type, out string Name);
                int Location = GL.GetUniformLocation(Handle, Name);

                if (Type == ActiveUniformType.Sampler2D)  // for each uniform sampler2D add default texture to unit
                    AssignTextureUnit(Texture.Sampler("Resources/Textures/Missing.png"), out _);
                
                if (!Name.Contains(".")) 
                    UniformLocation[Name] = Location; // add location lookup
            
            }

            // same as above but for uniform blocks
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniformBlocks, out var NumOfUniformBlocks);
            for (int Location = 0; Location < NumOfUniformBlocks; Location++)
            {
                GL.GetActiveUniformBlockName(Handle, Location, 32, out _, out string Name);
                UniformBlockLocation[Name] = Location; // add lookup location
            }
        }

        #region Set Uniform Functions
        #region Updating Uniform
        /// <summary>
        /// Adds Uniform that automatically updates before Program Use.
        /// </summary>
        /// <param name="Name">the name of the uniform.</param>
        /// <param name="UniformGetter">a function to get the uniform</param>
        public void SetUpdatingUniform(string Name, Func<dynamic> UniformGetter) => UpdatingUniforms[Name] = UniformGetter;
        public void RemoveUpdatingUniform(string Name) => UpdatingUniforms.Remove(Name);
        #endregion
        #region Block Uniform
        /// <summary>
        /// Binds program to use Uniform block
        /// </summary>
        /// <param name="Name">the name of the block</param>
        /// <param name="BlockBinding">the ID of the buffer</param>
        public void SetUniformBlock(string Name, int BlockBinding)
        {
            if (UniformBlockLocation.TryGetValue(Name, out int Location))
                GL.UniformBlockBinding(Handle, Location, BlockBinding);
            else
                Console.WriteLine($"Failed to set Uniform block '{Name}'");
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
        // int & bool
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
        // texture unit management
        private int[] ProgTex = new int[MaxUnits].Select(i => -1).ToArray(); // texture unit
        private int[] SamplerUseCount = new int[MaxUnits]; // number of times texture used
        private static int[] SamplerUnits = new int[MaxUnits]; // bound texture units
        /// <summary>
        /// sets The uniform 'Name' to the texture found at 'TexPath'
        /// </summary>
        public void SetUniformSampler2D(string Name, string TexPath)
        {
            int Tex = Texture.Sampler(TexPath);
            SetUniformSampler(Name, Tex);
        }
        /// <summary>
        /// sets the Uniform 'Name' to the Sampler 'ID'
        /// </summary>
        public void SetUniformSampler(string Name, int ID)
        {
            if (!UniformLocation.TryGetValue(Name, out int Location)) return; // doesnt throw error cos compiler can remove textures
            UnAssignsTextureUnit(Location);
            AssignTextureUnit(ID, out int Unit);
            GL.ProgramUniform1(Handle, Location, Unit);
        }
        /// <summary>
        /// If texture not used for this unit, allows it to be re assigned.
        /// </summary>
        /// <param name="Name">the name of the uniform</param>
        /// <param name="Location">Index of texture thats being unassigned</param>
        private void UnAssignsTextureUnit(int Location)
        {
            GL.GetUniform(Handle, Location, out int Unit); // read uniform
            if (--SamplerUseCount[Unit] == 0)// deincrement UnitUseCount
                ProgTex[Unit] = -1; // if Texture no longer in use, remove texture from program textures
            
        }
        /// <summary>
        /// assigns Texture to unit only if not already assigned to previous unit.
        /// </summary>
        /// <param name="Tex">The new texture.</param>
        /// <param name="Unit">The unit the texture is assigned to.</param>
        private void AssignTextureUnit(int Tex, out int Unit)
        {
            Unit = -1;
            if (ProgTex.Contains(Tex)) // if texture already in program textures
            {
                while (ProgTex[++Unit] != Tex); // linear search for program texture
            }
            else // if texture is not already in program textures
            {
                while (SamplerUseCount[++Unit] != 0) ; // linear search for unused program texture
                ProgTex[Unit] = Tex;
            }

            SamplerUseCount[Unit]++;
        }
        #endregion
        #endregion

        /// <summary>
        /// Uses this program Binds necessary Textures to textures associated with this program into texture units
        /// </summary>
        public void Use()
        {
            GL.UseProgram(Handle); // tell openGL to use this object

            for(int Unit = 0; Unit < MaxUnits; Unit++)
            {
                int TexID = ProgTex[Unit];
                if (SamplerUnits[Unit] != TexID) // if not bound
                {
                    GL.BindTextureUnit(Unit, TexID); // bind texture into unit
                    ProgTex[Unit] = TexID;
                }
                // else already bound no action needed
            }
                
            foreach (string Uniform in UpdatingUniforms.Keys) 
                SetUniform(Uniform, UpdatingUniforms[Uniform]());
        }
        
        /// <summary>
        /// Writes all active uniforms' data to the console
        /// </summary>
        public void DebugUniforms()
        {
            Console.WriteLine($"\nProgram {Handle}");
                       
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var NumOfUniforms);
            for (int i = 0; i < NumOfUniforms; i++)
            {
                GL.GetActiveUniform(Handle, i, 32, out int _, out int Size, out ActiveUniformType UniformType, out string Name);
                int Location = GL.GetUniformLocation(Handle, Name);
                if (Location != -1)
                {
                    switch (UniformType)
                    {
                        // float stuff
                        case ActiveUniformType.Float:       FloatPrint(i, Location, Name, UniformType, 1); break;
                        case ActiveUniformType.FloatVec2:   FloatPrint(i, Location, Name, UniformType, 2); break;
                        case ActiveUniformType.FloatVec3:   FloatPrint(i, Location, Name, UniformType, 3); break;
                        case ActiveUniformType.FloatVec4:   FloatPrint(i, Location, Name, UniformType, 4); break;
                        
                        case ActiveUniformType.FloatMat2:   FloatPrint(i, Location, Name, UniformType, 4); break;
                        case ActiveUniformType.FloatMat2x3: FloatPrint(i, Location, Name, UniformType, 6); break;
                        case ActiveUniformType.FloatMat2x4: FloatPrint(i, Location, Name, UniformType, 8); break;
                        
                        case ActiveUniformType.FloatMat3:   FloatPrint(i, Location, Name, UniformType, 9); break;
                        case ActiveUniformType.FloatMat3x4: FloatPrint(i, Location, Name, UniformType, 12); break;
                        case ActiveUniformType.FloatMat3x2: FloatPrint(i, Location, Name, UniformType, 6); break;
                        
                        case ActiveUniformType.FloatMat4:   FloatPrint(i, Location, Name, UniformType, 16); break;
                        case ActiveUniformType.FloatMat4x2: FloatPrint(i, Location, Name, UniformType, 8); break;
                        case ActiveUniformType.FloatMat4x3: FloatPrint(i, Location, Name, UniformType, 12); break;
                        
                            //samplers
                        case ActiveUniformType.Sampler1D:               SamplerPrint(i, Location, Name, UniformType); break;
                        case ActiveUniformType.Sampler1DArray:          SamplerPrint(i, Location, Name, UniformType); break;
                        case ActiveUniformType.Sampler1DArrayShadow:    SamplerPrint(i, Location, Name, UniformType); break;
                        case ActiveUniformType.Sampler1DShadow:         SamplerPrint(i, Location, Name, UniformType); break;

                        case ActiveUniformType.Sampler2D:               SamplerPrint(i, Location, Name, UniformType); break;
                        case ActiveUniformType.Sampler2DArray:          SamplerPrint(i, Location, Name, UniformType); break;
                        case ActiveUniformType.Sampler2DArrayShadow:    SamplerPrint(i, Location, Name, UniformType); break;
                        case ActiveUniformType.Sampler2DShadow:         SamplerPrint(i, Location, Name, UniformType); break;
                        
                        case ActiveUniformType.Sampler3D:               SamplerPrint(i, Location, Name, UniformType); break;

                        default: throw new Exception("Go write more debuggy stuff");
                    }
                    
                }
                
            }
            // same as above but for uniform blocks
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniformBlocks, out var NumOfUniformBlocks);
            for (int Location = 0; Location < NumOfUniformBlocks; Location++)
            {
                GL.GetActiveUniformBlockName(Handle, Location, 32, out _, out string Name);
                GL.GetActiveUniformBlock(Handle, Location, ActiveUniformBlockParameter.UniformBlockBinding, out int Binding);
                GL.GetActiveUniformBlock(Handle, Location, ActiveUniformBlockParameter.UniformBlockDataSize, out int Size);
                Console.WriteLine($"Uniform Block - {Location}: {Name} -> {Binding} of size {Size}");

            }
            // show texture units
            Console.Write($"TextureUnits : ");
            ProgTex.Select(I => { string S = I.ToString(); if (I != -1) Console.Write($"{S}, "); return S; }).ToArray();
            Console.Write("\n");


        }
        private void SamplerPrint(int i, int Location, string Name, ActiveUniformType UniformType)
        {
            GL.GetUniform(Handle, Location, out int Unit); // for some reason this fucked up and set i to 0 when it couldnt read the value??? which did an infinite loop
            Console.WriteLine($"Uniform - {i}: {Location} = {UniformType} {Name} in {Unit} -> Texture {ProgTex[Unit]}");
        }
        private void FloatPrint(int i, int Location, string Name, ActiveUniformType UniformType, int DataLength)
        {
            float[] Data = new float[DataLength].Select(i => -1f).ToArray();
            GL.GetUniform(Handle, Location, Data);
            Console.Write($"Uniform - {i}: {Location} = {UniformType} {Name} -> ");
            Data.Select((f) => { Console.Write($"{f}, "); return f; }).ToArray();
            Console.Write("\n");
        }
        /// <summary>
        /// creates a new shader in OpenGL
        /// </summary>
        /// <param name="Type">shader Type</param>
        /// <returns>Shader ID</returns>
        private int LoadShader(ShaderType Type, string code)
        {
            int NewShader = GL.CreateShader(Type); // initiate new shader
            
            GL.ShaderSource(NewShader, code); // attaches shader and code           
            GL.CompileShader(NewShader); // compiles shader code

            // get info to check for errors
            string info = GL.GetShaderInfoLog(NewShader);
            if (!string.IsNullOrWhiteSpace(info))
                throw new Exception($"{Type} failed to compile.\r\nGo fix your code numb nuts.\n\n{info}");

            GL.AttachShader(Handle, NewShader);
            return NewShader;
        }
    }
}
