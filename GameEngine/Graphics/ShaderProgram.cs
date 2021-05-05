using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Graphics.Shaders
{
    /* THINGS TO DO:
     * 
     * uniform block/buffer support for faster swapping of uniform values
     * uniform array support ???
     *  - Uniform[1,2,3,4][f,i]() Maybe DONE???
     * uniform structs support ???
     * 
     * Every uniform currently updated every frame -> because I'm dumb partly
     * 
     * 
     * 
     */


    sealed public class ShaderProgram2
    {
        private int Program;
        private int[] Units = new int[32]; // texture units
        private int[] UnitUsed = new int[32];
        //public ShaderProgram2(string vertexpath, string geometrypath, string fragmentpath) { }
        public ShaderProgram2(string vertexpath, string fragmentpath) 
        {
            // creates new program
            Program = GL.CreateProgram();
            // compile new shaders
            int Vert = LoadShader(ShaderType.VertexShader, vertexpath);
            int Frag = LoadShader(ShaderType.FragmentShader, fragmentpath);

            // attach new shaders
            GL.AttachShader(Program, Vert);
            GL.AttachShader(Program, Frag);

            // link new shaders
            GL.LinkProgram(Program);

            // check for error linking shaders to program
            string info = GL.GetProgramInfoLog(Program);
            if (!string.IsNullOrWhiteSpace(info))
            {
                Console.WriteLine(info);
                throw new Exception("Program failed to compile. Fucked if I know. It's probably the ins and outs things. Basically this is your fuck up not mine.");
            }



            // detach and delete both shaders
            GL.DetachShader(Program, Vert);
            GL.DetachShader(Program, Frag);
            GL.DeleteShader(Vert);
            GL.DeleteShader(Frag);

            GL.GetProgram(Program, GetProgramParameterName.ActiveUniforms, out int NumOfUniform);

            for (int i = 0; i < NumOfUniform; i++)
            {
                GL.GetActiveUniform(Program, i, 32, out _, out _, out ActiveUniformType Type, out string Name);

                if (Type == (ActiveUniformType.Sampler2D | ActiveUniformType.Sampler2DArray | ActiveUniformType.Sampler2DArrayShadow |
                    ActiveUniformType.Sampler2DMultisample | ActiveUniformType.Sampler2DMultisampleArray | ActiveUniformType.Sampler2DRect |
                    ActiveUniformType.Sampler2DRect | ActiveUniformType.Sampler2DRectShadow | ActiveUniformType.Sampler2DShadow))
                {
                    
                }
            }



        }
        
        public void Use() 
        {
            GL.UseProgram(Program); // tell openGL to use this object
            for (int i = 0; UnitUsed[i] != 0; i++) GL.BindTextureUnit(i, Units[i]);


        }       
        //textures
        
        
        /// <summary>
        /// sets uniform in shader to use uniform block
        /// </summary>
        /// <param name="Name">the name of the uniform block</param>
        /// <param name="UBO">the Uniform Buffer object's ID</param>
        public void SetUniformBlock<T>(string Name, int UBO)
        {
            var BlockIndex = GL.GetUniformBlockIndex(Program, Name);
            GL.UniformBlockBinding(Program, BlockIndex, UBO);
        }
        #region Sampler Uniforms

        public void SetUniformSampler2D(string Name, int Tex)
        {
            AssignTextureUnit(Name, Tex, out int Unit);
            GL.BindTextureUnit(Unit, Tex); // bind new texture to unit
        }
        public void SetUniformSampler2D(string Name, string TexPath)
        {
            int Tex = TextureManager.Texture(TexPath);
            AssignTextureUnit(Name, Tex, out int Unit);
            GL.BindTextureUnit(Unit, Tex); // bind new texture to unit
        }

        private void AssignTextureUnit(string Name, int newTex, out int Unit)
        {
            int Index = GL.GetUniformLocation(Program, Name); // find uniform index
            GL.GetUniform(Program, Index, out Unit); // get uniform value ie the texture unit
            if (--UnitUsed[Unit] > 0) // if texture still in use
            {
                Unit = UnitUsed.First((Count) => Count == 0);
                if (Unit == -1) throw new Exception("Texture Units all Full(assuming max 32)");
            }
            UnitUsed[Unit]++;
            Units[Unit] = newTex; // assign new texture to unit
        }
        #endregion
        #region Typed Set Uniform
        #region single value
        // int
        public void SetUniform(string Name, bool Param) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform1(Index, Param ? 1 : 0); }
        public void SetUniform(string Name, int Param) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform1(Index, Param); }
        public void SetUniform(string Name, Vector2i Param) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform2(Index, Param); }
        public void SetUniform(string Name, Vector3i Param) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform3(Index, Param); }
        public void SetUniform(string Name, Vector4i Param) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform4(Index, Param); }

        //doubles
        public void SetUniform(string Name, double Param) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform1(Index, Param); }
        public void SetUniform(string Name, Vector2d Param) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform2(Index, Param.X, Param.Y); }
        public void SetUniform(string Name, Vector3d Param) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform3(Index, Param.X, Param.Y, Param.Z); }
        public void SetUniform(string Name, Vector4d Param) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform4(Index, Param.X, Param.Y, Param.Z, Param.W); }

        // floats
        public void SetUniform(string Name, float Param) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform1(Index, Param); }
        public void SetUniform(string Name, Vector2 Param) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform2(Index, Param); }
        public void SetUniform(string Name, Vector3 Param) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform3(Index, Param); }
        public void SetUniform(string Name, Vector4 Param) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform4(Index, Param); }

        //matrices
        public void SetUniform(string Name, Matrix2 Param) { int Index = GL.GetUniformLocation(Program, Name); GL.UniformMatrix2(Index, true, ref Param); }
        public void SetUniform(string Name, Matrix3 Param) { int Index = GL.GetUniformLocation(Program, Name); GL.UniformMatrix3(Index, true, ref Param); }
        public void SetUniform(string Name, Matrix4 Param) { int Index = GL.GetUniformLocation(Program, Name); GL.UniformMatrix4(Index, false, ref Param); }
        #endregion
        #region array values
        // int
        public void SetUniform(string Name, IEnumerable<bool> ParamArray) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform1(Index, ParamArray.Count(), ParamArray.Select((Bl) => Bl? 1:0).ToArray()); }
        public void SetUniform(string Name, IEnumerable<int> ParamArray) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform1(Index, ParamArray.Count(), ParamArray.ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector2i> ParamArray) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform2(Index, ParamArray.Count(), ParamArray.SelectMany((V) => new int[] { V.X, V.Y }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector3i> ParamArray) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform3(Index, ParamArray.Count(), ParamArray.SelectMany((V) => new int[] { V.X, V.Y, V.Z }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector4i> ParamArray) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform4(Index, ParamArray.Count(), ParamArray.SelectMany((V) => new int[] { V.X, V.Y, V.Z, V.W }).ToArray()); }

        //doubles
        public void SetUniform(string Name, IEnumerable<double> ParamArray) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform1(Index, ParamArray.Count(), ParamArray.ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector2d> ParamArray) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform2(Index, ParamArray.Count(), ParamArray.SelectMany((V) => new double[] { V.X, V.Y }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector3d> ParamArray) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform3(Index, ParamArray.Count(), ParamArray.SelectMany((V) => new double[] { V.X, V.Y, V.Z }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector4d> ParamArray) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform4(Index, ParamArray.Count(), ParamArray.SelectMany((V) => new double[] { V.X, V.Y, V.Z, V.W }).ToArray()); }

        // floats
        public void SetUniform(string Name, IEnumerable<float> ParamArray) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform1(Index, ParamArray.Count(), ParamArray.ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector2> ParamArray) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform2(Index, ParamArray.Count(), ParamArray.SelectMany((V) => new float[] { V.X, V.Y }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector3> ParamArray) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform3(Index, ParamArray.Count(), ParamArray.SelectMany((V) => new float[] { V.X, V.Y, V.Z }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Vector4> ParamArray) { int Index = GL.GetUniformLocation(Program, Name); GL.Uniform4(Index, ParamArray.Count(), ParamArray.SelectMany((V) => new float[] { V.X, V.Y, V.Z, V.W }).ToArray()); }

        //matrices
        // these matrice arrays are probably wrong
        public void SetUniform(string Name, IEnumerable<Matrix2> ParamArray) { int Index = GL.GetUniformLocation(Program, Name); GL.UniformMatrix2(Index, ParamArray.Count(), false, ParamArray.SelectMany((M) => new float[] { M.M11, M.M12, M.M21, M.M22 }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Matrix3> ParamArray) { int Index = GL.GetUniformLocation(Program, Name); GL.UniformMatrix3(Index, ParamArray.Count(), false, ParamArray.SelectMany((M) => new float[] { M.M11, M.M12, M.M13, M.M21, M.M22, M.M23, M.M31, M.M32, M.M33, }).ToArray()); }
        public void SetUniform(string Name, IEnumerable<Matrix4> ParamArray) { int Index = GL.GetUniformLocation(Program, Name); GL.UniformMatrix4(Index, ParamArray.Count(), false, ParamArray.SelectMany((M) => new float[] { M.M11, M.M12, M.M13, M.M14, M.M21, M.M22, M.M23, M.M24, M.M31, M.M32, M.M33, M.M34 , M.M41, M.M42, M.M43, M.M44 }).ToArray()); }
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


    }

    public class UniformBlock<TypeStruct>
    {
        private int UBO;
        public UniformBlock(TypeStruct Block) 
        {
            UBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, UBO);




            //int BlockSize, BlockBuffer;
            //GL.BufferData(BufferTarget.UniformBuffer, BlockSize, BlockBuffer, BufferUsageHint.DynamicDraw);

        }
        public UniformBlock(IEnumerable<TypeStruct> Block) 
        {
            UBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, UBO);
        }
        public void Set(TypeStruct Block) 
        { 
        
        }
        public void Set(IEnumerable<TypeStruct> Block) 
        { 
            
        }
        public static implicit operator int(UniformBlock<TypeStruct> UniformBlock) => UniformBlock.UBO;
    }


    sealed public class ShaderProgram
    {
        public Dictionary<string, Func<dynamic>> Uniforms;

        private int Program;

        public ShaderProgram(string vertexpath, string fragmentpath)
        {
            SetUniforms = () => { };
            Uniforms = new Dictionary<string, Func<dynamic>>();
            CompileProgram(vertexpath, fragmentpath);
        }
        public ShaderProgram(string vertexpath, string geometrypath, string fragmentpath)
        {
            SetUniforms = () => { };
            Uniforms = new Dictionary<string, Func<dynamic>>();
            CompileProgram(vertexpath, geometrypath, fragmentpath);
        }
        

        /// <summary>
        /// used to gather all uniforms and place them into the correct indexes and loading texture units
        /// </summary>
        private event Action SetUniforms;

        /// <summary>
        /// set openGl to use this shader program
        /// </summary>
        public void Use()
        {
            GL.UseProgram(Program); // tell openGL to use this object
            TextureManager.TexturesLoaded = 0; // each time a shader program is used the texture units are forgotten ie allows overwriting of textures
            SetUniforms(); // update the uniforms in the shaders
        }

        #region Update Uniform Types
        //ints and bool
        private void SetUniformInt(int Index, string Name) { int param = Uniforms[Name](); GL.Uniform1(Index, param); }
        private void SetUniformIntVec2(int Index, string Name) { Vector2i param = (Vector2i)Uniforms[Name](); GL.Uniform2(Index, param.X, param.Y); }
        private void SetUniformIntVec3(int Index, string Name) { Vector3i param = (Vector3i)Uniforms[Name](); GL.Uniform3(Index, param.X, param.Y, param.Z); }
        private void SetUniformIntVec4(int Index, string Name) { Vector4i param = (Vector4i)Uniforms[Name](); GL.Uniform4(Index, param.W, param.X, param.Y, param.Z); }

        //doubles
        private void SetUniformDouble(int Index, string Name) { double param = (double)Uniforms[Name](); GL.Uniform1(Index, param); }
        private void SetUniformDoubleVec2(int Index, string Name) { Vector2d param = (Vector2d)Uniforms[Name](); GL.Uniform2(Index, param.X, param.Y); }
        private void SetUniformDoubleVec3(int Index, string Name) { Vector3d param = (Vector3d)Uniforms[Name](); GL.Uniform3(Index, param.X, param.Y, param.Z); }
        private void SetUniformDoubleVec4(int Index, string Name) { Vector4d param = (Vector4d)Uniforms[Name](); GL.Uniform4(Index, param.W, param.X, param.Y, param.Z); }

        // floats
        private void SetUniformFloat(int Index, string Name) { double param = (double)Uniforms[Name](); GL.Uniform1(Index, param); }
        private void SetUniformFloatVec2(int Index, string Name) { Vector2 param = (Vector2)Uniforms[Name](); GL.Uniform2(Index, param.X, param.Y); }
        private void SetUniformFloatVec3(int Index, string Name) { Vector3 param = (Vector3)Uniforms[Name](); GL.Uniform3(Index, param.X, param.Y, param.Z); }
        private void SetUniformFloatVec4(int Index, string Name) { Vector4 param = (Vector4)Uniforms[Name](); GL.Uniform4(Index, param.W, param.X, param.Y, param.Z); }

        //matrices
        private void SetUniformFloatMat2(int Index, string Name) { Matrix2 param = (Matrix2)Uniforms[Name](); GL.UniformMatrix2(Index, true, ref param); }
        private void SetUniformFloatMat3(int Index, string Name) { Matrix3 param = (Matrix3)Uniforms[Name](); GL.UniformMatrix3(Index, true, ref param); }
        private void SetUniformFloatMat4(int Index, string Name) { Matrix4 param = (Matrix4)Uniforms[Name](); GL.UniformMatrix4(Index, false, ref param); }
        
        //textures
        private void SetUniformSampler2D(int Index, string Name) 
        {
            int unit = TextureManager.TexturesLoaded++;
            dynamic param = Uniforms[Name]();
            switch (param)
            {
                case string TextureStringID:
                    GL.BindTextureUnit(unit, TextureManager.Texture(TextureStringID));
                    break;
                case int IntID:
                    GL.BindTextureUnit(unit, IntID);
                    break;
            }
            //GL.ActiveTexture((TextureUnit)unit);
            GL.Uniform1(Index, (int)unit);
        }

        #endregion

        #region Program Compilation
        /// <summary>
        /// Compile Program with vertex and fragment shader
        /// </summary>
        /// <param name="vertexpath">The path to the shader -> "Resources/.../shader.vert"</param>
        /// <param name="fragmentpath">The path to the shader -> "Resources/.../shader.frag"</param>
        private void CompileProgram(string vertexpath, string fragmentpath)
        {
            // creates new program
            Program = GL.CreateProgram();
            // compile new shaders
            int Vert = CompileShader(ShaderType.VertexShader, vertexpath);
            int Frag = CompileShader(ShaderType.FragmentShader, fragmentpath);

            // attach new shaders
            GL.AttachShader(Program, Vert);
            GL.AttachShader(Program, Frag);

            // link new shaders
            GL.LinkProgram(Program);

            // check for error linking shaders to program
            string info = GL.GetProgramInfoLog(Program);
            if (!string.IsNullOrWhiteSpace(info))
            {
                Console.WriteLine(info);
                throw new Exception("Program failed to compile. Fucked if I know. It's probably the ins and outs things. Basically this is your fuck up not mine.");
            }

            

            // detach and delete both shaders
            GL.DetachShader(Program, Vert);
            GL.DetachShader(Program, Frag);
            GL.DeleteShader(Vert);
            GL.DeleteShader(Frag);

            GetUniforms();
        }
        /// <summary>
        /// Compile Program with vertex, geometry and fragment shader
        /// </summary>
        /// <param name="vertexpath">The path to the vertex shader -> "Resources/.../shader.vert"</param>
        /// <param name="geometrypath">The path to the geometry shader -> "Resources/.../shader.geom"</param>
        /// <param name="fragmentpath">The path to the fragment shader -> "Resources/.../shader.frag"</param>
        private void CompileProgram(string vertexpath, string geometrypath, string fragmentpath)
        {
            // creates new program
            Program = GL.CreateProgram();
            // compile new shaders
            int Vert = CompileShader(ShaderType.VertexShader, vertexpath);
            
            int Geom = CompileShader(ShaderType.GeometryShader, geometrypath);
            
            int Frag = CompileShader(ShaderType.FragmentShader, fragmentpath);

            // attach new shaders
            GL.AttachShader(Program, Vert);
            GL.AttachShader(Program, Geom);
            GL.AttachShader(Program, Frag);

            // link new shaders
            GL.LinkProgram(Program);

            // check for error linking shaders to program
            string info = GL.GetProgramInfoLog(Program);
            if (!string.IsNullOrWhiteSpace(info))
            {
                Console.WriteLine(info);
                throw new Exception("Program failed to compile. Fucked if I know. It's probably the ins and outs things. Basically this is your fuck up not mine.");
            }

            GetUniforms();

            // detach and delete both shaders
            GL.DetachShader(Program, Vert);
            GL.DetachShader(Program, Geom);
            GL.DetachShader(Program, Frag);
            GL.DeleteShader(Vert);
            GL.DeleteShader(Geom);
            GL.DeleteShader(Frag);
        }
        /// <summary>
        /// creates a new shader in OpenGl
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private int CompileShader(ShaderType Type, string path) 
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
        /// loops through uniforms and adds function to assign uniform into Update uniforms event
        /// </summary>
        private void GetUniforms()
        {

            Console.WriteLine($"Program {Program}:");
           
            GL.GetProgram(Program, GetProgramParameterName.ActiveUniformBlocks, out int NumOfUniformBlocks);
            
            for (int i = 0; i < NumOfUniformBlocks; i++)
            {
                GL.GetActiveUniformBlock(Program, i, ActiveUniformBlockParameter.UniformBlockDataSize, out int BlockSize);
                GL.GetActiveUniformBlockName(Program, i, 32, out int Length, out string Name);
                Console.WriteLine($"Uniform BLock Name: {Name} and Length: {Length}");

            }

            GL.GetProgram(Program, GetProgramParameterName.ActiveUniforms, out var NumOfUniforms);

            for (int i = 0; i < NumOfUniforms; i++)
            {
                GL.GetActiveUniform(
                    Program, i, 32, 
                    out _,
                    out _,
                    out ActiveUniformType Type, 
                    out string Name
                );

                int Location = GL.GetUniformLocation(Program, Name);

                Console.WriteLine($"{i}. Uniform {Type} {Name} Location {Location}");// Size {Size} BufSize {BufSize}");


                switch (Type)
                {
                    case ActiveUniformType.Bool:
                        Uniforms[Name] = () => false;
                        SetUniforms += () => SetUniformInt(Location, Name);
                        break;

                    case ActiveUniformType.BoolVec2:
                        Uniforms[Name] = () => new Vector2i();
                        SetUniforms += () => SetUniformIntVec2(Location, Name);
                        break;

                    case ActiveUniformType.BoolVec3:
                        Uniforms[Name] = () => new Vector3i();
                        SetUniforms += () => SetUniformIntVec3(Location, Name);
                        break;

                    case ActiveUniformType.BoolVec4:
                        Uniforms[Name] = () => new Vector4i();
                        SetUniforms += () => SetUniformIntVec4(Location, Name);
                        break;

                    case ActiveUniformType.Double:
                        Uniforms[Name] = () => 0d;
                        SetUniforms += () => SetUniformDouble(Location, Name);
                        break;

                    case ActiveUniformType.DoubleVec2:
                        Uniforms[Name] = () => new Vector2d();
                        SetUniforms += () => SetUniformDoubleVec2(Location, Name);
                        break;

                    case ActiveUniformType.DoubleVec3:
                        Uniforms[Name] = () => new Vector3d();
                        SetUniforms += () => SetUniformDoubleVec3(Location, Name);
                        break;
                    case ActiveUniformType.DoubleVec4:
                        Uniforms[Name] = () => new Vector4d();
                        SetUniforms += () => SetUniformDoubleVec4(Location, Name);
                        break;
                    case ActiveUniformType.Float:
                        Uniforms[Name] = () => 0f;
                        SetUniforms += () => SetUniformFloat(Location, Name);
                        break;
                    case ActiveUniformType.FloatVec2:
                        Uniforms[Name] = () => new Vector2();
                        SetUniforms += () => SetUniformFloatVec2(Location, Name);
                        break;
                    case ActiveUniformType.FloatVec3:
                        Uniforms[Name] = () => new Vector3();
                        SetUniforms += () => SetUniformFloatVec3(Location, Name);
                        break;
                    case ActiveUniformType.FloatVec4:
                        Uniforms[Name] = () => new Vector4();
                        SetUniforms += () => SetUniformFloatVec4(Location, Name);
                        break;

                    case ActiveUniformType.FloatMat2:
                        Uniforms[Name] = () => Matrix2.Identity;
                        SetUniforms += () => SetUniformFloatMat2(Location, Name);
                        break;

                    case ActiveUniformType.FloatMat3:
                        Uniforms[Name] = () => Matrix3.Identity;
                        SetUniforms += () => SetUniformFloatMat3(Location, Name);
                        break;

                    case ActiveUniformType.FloatMat4:
                        Uniforms[Name] = () => Matrix4.Identity;
                        SetUniforms += () => SetUniformFloatMat4(Location, Name);
                        break;

                    case ActiveUniformType.Int:
                        Uniforms[Name] = () => (int)0;
                        SetUniforms += () => SetUniformInt(Location, Name);
                        break;

                    case ActiveUniformType.IntVec2:
                        Uniforms[Name] = () => new Vector2i();
                        SetUniforms += () => SetUniformIntVec2(Location, Name);
                        break;

                    case ActiveUniformType.IntVec3:
                        Uniforms[Name] = () => new Vector3i();
                        SetUniforms += () => SetUniformIntVec3(Location, Name);
                        break;
                    case ActiveUniformType.IntVec4:
                        Uniforms[Name] = () => new Vector4i();
                        SetUniforms += () => SetUniformIntVec4(Location, Name);
                        break;
                    case ActiveUniformType.Sampler2D:
                        Uniforms[Name] = () => "Resources/Textures/Missing.png";
                        SetUniforms += () => SetUniformSampler2D(Location, Name);
                        break;
                    /*
                    // some more the samplers(not even all.. yh its pain)
                    case ActiveUniformType.Sampler1D: break;
                    case ActiveUniformType.Sampler1DArray: break;
                    case ActiveUniformType.Sampler1DArrayShadow: break;
                    //case ActiveUniformType.Sampler2D: break;
                    case ActiveUniformType.Sampler2DArray: break;
                    case ActiveUniformType.Sampler2DArrayShadow: break;
                    case ActiveUniformType.Sampler2DMultisample: break;
                    case ActiveUniformType.Sampler2DMultisampleArray: break;
                    case ActiveUniformType.Sampler2DRect: break;
                    case ActiveUniformType.Sampler2DRectShadow: break;
                    case ActiveUniformType.Sampler2DShadow: break;
                    case ActiveUniformType.Sampler3D: break;
                    case ActiveUniformType.SamplerBuffer: break;
                    case ActiveUniformType.SamplerCube: break;
                    case ActiveUniformType.SamplerCubeMapArray: break;
                    case ActiveUniformType.SamplerCubeMapArrayShadow: break;
                    case ActiveUniformType.SamplerCubeShadow: break;
                    */

                    /*
                    // skipped because awkward w no convenient matrices
                    case ActiveUniformType.FloatMat3x2: break;
                    case ActiveUniformType.FloatMat3x4: break;
                    case ActiveUniformType.FloatMat4x2: break;
                    case ActiveUniformType.FloatMat4x3: break;
                    case ActiveUniformType.FloatMat2x3: break;
                    case ActiveUniformType.FloatMat2x4: break;

                    */


                    default: 
                        throw new Exception($"Well shit.. wtf is the this. A {Type}? what do i do with this?? cry maybe. yh i suggest crying.");
                }
            
            }
        }
        #endregion

    }

}
