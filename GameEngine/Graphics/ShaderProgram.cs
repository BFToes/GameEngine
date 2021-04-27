using System;
using System.IO;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections;

namespace Graphics.Shaders
{

    sealed public class ShaderProgram
    {
        public Dictionary<string, Func<dynamic>> Uniforms;
        
        private int Handle;

        public ShaderProgram(string vertexpath, string fragmentpath)
        {
            UpdateUniforms = () => { };
            Uniforms = new Dictionary<string, Func<dynamic>>();
            CompileProgram(vertexpath, fragmentpath);
        }
        public ShaderProgram(string vertexpath, string geometrypath, string fragmentpath)
        {
            UpdateUniforms = () => { };
            Uniforms = new Dictionary<string, Func<dynamic>>();
            CompileProgram(vertexpath, geometrypath, fragmentpath);
        }
        
        private event Action UpdateUniforms;

        /// <summary>
        /// set openGl to use this shader program
        /// </summary>
        public void Use()
        {
            TextureManager.TexturesLoaded = 0; // each time a shader program is used the texture units are forgotten ie allows overwriting of textures
            GL.UseProgram(Handle); // tell openGL to use this object
            UpdateUniforms(); // update the uniforms in the shaders
        }
        /// <summary>
        /// breakdown shader scripts to find uniform types and names and add corresponding update function to UpdateUniforms()
        /// </summary>
        private void GetUniforms()
        {
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var NumOfUniforms);
            for (int i = 0; i < NumOfUniforms; i++)
            {
                ActiveUniformType Type; string Name;
                string key = GL.GetActiveUniform(Handle, i, out _, out Type);
                GL.GetActiveUniform(Handle, i, 16, out _, out _, out _, out Name);

                int LocalIndex = i;

                switch (Type)
                {
                    case ActiveUniformType.Bool:
                        Uniforms[Name] = () => false;
                        UpdateUniforms += () =>
                        {
                            int param = Uniforms[Name]();
                            GL.Uniform1(LocalIndex, param);
                        };
                        break;

                    case ActiveUniformType.BoolVec2:
                        Uniforms[Name] = () => new Vector2i();
                        UpdateUniforms += () =>
                        {
                            Vector2 param = (Vector2i)Uniforms[Name]();
                            GL.Uniform2(LocalIndex, param.X, param.Y);
                        };
                        break;

                    case ActiveUniformType.BoolVec3:
                        Uniforms[Name] = () => new Vector3i();
                        UpdateUniforms += () =>
                        {
                            Vector3 param = (Vector3i)Uniforms[Name]();
                            GL.Uniform3(LocalIndex, param.X, param.Y, param.Z);
                        };
                        break;

                    case ActiveUniformType.BoolVec4:
                        Uniforms[Name] = () => new Vector4i();
                        UpdateUniforms += () =>
                        {
                            Vector4i param = (Vector4i)Uniforms[Name]();
                            GL.Uniform4(LocalIndex, param.W, param.X, param.Y, param.Z);
                        };
                        break;

                    case ActiveUniformType.Double:
                        Uniforms[Name] = () => 0d;
                        UpdateUniforms += () =>
                        {
                            double param = (double)Uniforms[Name]();
                            GL.Uniform1(LocalIndex, param);
                        };
                        break;

                    case ActiveUniformType.DoubleVec2:
                        Uniforms[Name] = () => new Vector2d();
                        UpdateUniforms += () =>
                        {
                            Vector2d param = (Vector2d)Uniforms[Name]();
                            GL.Uniform2(LocalIndex, param.X, param.Y);
                        };
                        break;

                    case ActiveUniformType.DoubleVec3:
                        Uniforms[Name] = () => new Vector3d();
                        UpdateUniforms += () =>
                        {
                            Vector3d param = (Vector3d)Uniforms[Name]();
                            GL.Uniform3(LocalIndex, param.X, param.Y, param.Z);
                        };
                        break;
                    case ActiveUniformType.DoubleVec4:
                        Uniforms[Name] = () => new Vector4d();
                        UpdateUniforms += () =>
                        {
                            Vector4d param = (Vector4d)Uniforms[Name]();
                            GL.Uniform4(LocalIndex, param.W, param.X, param.Y, param.Z);
                        };
                        break;
                    case ActiveUniformType.Float:
                        Uniforms[Name] = () => 0f;
                        UpdateUniforms += () =>
                        {
                            float param = (float)Uniforms[Name]();
                            GL.Uniform1(LocalIndex, param);
                        };
                        break;
                    case ActiveUniformType.FloatVec2:
                        Uniforms[Name] = () => new Vector2();
                        UpdateUniforms += () =>
                        {
                            Vector2 param = (Vector2)Uniforms[Name]();
                            GL.Uniform2(LocalIndex, param.X, param.Y);
                        };
                        break;
                    case ActiveUniformType.FloatVec3:
                        Uniforms[Name] = () => new Vector3();
                        UpdateUniforms += () =>
                        {
                            Vector3 param = (Vector3)Uniforms[Name]();
                            GL.Uniform3(LocalIndex, param.X, param.Y, param.Z);
                        };
                        break;
                    case ActiveUniformType.FloatVec4:
                        Uniforms[Name] = () => new Vector4();
                        UpdateUniforms += () =>
                        {
                            Vector4 param = (Vector4)Uniforms[Name]();
                            GL.Uniform4(LocalIndex, param.W, param.X, param.Y, param.Z);
                        };
                        break;

                    case ActiveUniformType.FloatMat2:
                        Uniforms[Name] = () => Matrix2.Identity;
                        UpdateUniforms += () =>
                        {
                            Matrix2 param = (Matrix2)Uniforms[Name]();
                            GL.UniformMatrix2(LocalIndex, true, ref param);
                        };
                        break;

                    case ActiveUniformType.FloatMat3:
                        Uniforms[Name] = () => Matrix3.Identity;
                        UpdateUniforms += () =>
                        {
                            Matrix3 param = (Matrix3)Uniforms[Name]();
                            GL.UniformMatrix3(LocalIndex, true, ref param);
                        };
                        break;

                    case ActiveUniformType.FloatMat4:
                        Uniforms[Name] = () => Matrix4.Identity;
                        UpdateUniforms += () =>
                        {
                            Matrix4 param = (Matrix4)Uniforms[Name]();
                            GL.UniformMatrix4(LocalIndex, false, ref param);
                        };
                        break;

                    case ActiveUniformType.Int:
                        Uniforms[Name] = () => (int)0;
                        UpdateUniforms += () =>
                        {
                            int param = (int)Uniforms[Name]();
                            GL.Uniform1(LocalIndex, param);
                        };
                        break;
                    case ActiveUniformType.IntVec2:
                        Uniforms[Name] = () => new Vector2i();
                        UpdateUniforms += () =>
                        {
                            Vector2i param = (Vector2i)Uniforms[Name]();
                            GL.Uniform2(LocalIndex, param.X, param.Y);
                        };
                        break;
                    case ActiveUniformType.IntVec3:
                        Uniforms[Name] = () => new Vector3i();
                        UpdateUniforms += () =>
                        {
                            Vector3i param = (Vector3i)Uniforms[Name]();
                            GL.Uniform3(LocalIndex, param.X, param.Y, param.Z);
                        };
                        break;
                    case ActiveUniformType.IntVec4:
                        Uniforms[Name] = () => new Vector4i();
                        UpdateUniforms += () =>
                        {
                            Vector4i param = (Vector4i)Uniforms[Name]();
                            GL.Uniform4(LocalIndex, param.W, param.X, param.Y, param.Z);
                        };
                        break;
                    case ActiveUniformType.Sampler2D:
                        Uniforms[Name] = () => "Resources/Textures/Missing.png";
                        UpdateUniforms += () =>
                        {
                            int unit = TextureManager.TexturesLoaded++;
                            GL.BindTextureUnit(unit, TextureManager.Texture((string)Uniforms[Name]())); // texture is bound before use
                            GL.ActiveTexture((TextureUnit)unit);
                            GL.Uniform1(LocalIndex, (int)unit);
                        };
                        break;

                    /*
                    // problematic uniforms with no nice functions only ugly irritating functions
                    case ActiveUniformType.FloatMat3x2: 
                        AddUniform(Name, new Matrix3x2()); 
                        break;
                    case ActiveUniformType.FloatMat3x4: 
                        AddUniform(Name, new Matrix3x4()); 
                        break;

                    case ActiveUniformType.FloatMat4x2: 
                        AddUniform(Name, new Matrix4x2());
                        break;
                    case ActiveUniformType.FloatMat4x3: 
                        AddUniform(Name, new Matrix4x3()); 
                        break;

                    case ActiveUniformType.FloatMat2x3: 
                        AddUniform(Name, new Matrix2x3());
                        break;
                    case ActiveUniformType.FloatMat2x4: 
                        AddUniform(Name, new Matrix2x4()); 
                        break;

                        there are more in ActiveUniformType.[...]
                    */

                    default: throw new Exception($"Well shit.. wtf is the this. A {Type}? what do i do with this?? cry maybe. yh i suggest crying.");
                }
            }
        }
        /// <summary>
        /// Compile Program with vertex and fragment shader
        /// </summary>
        /// <param name="vertexpath">The path to the shader -> "Resources/.../shader.vert"</param>
        /// <param name="fragmentpath">The path to the shader -> "Resources/.../shader.frag"</param>
        private void CompileProgram(string vertexpath, string fragmentpath)
        {
            // creates new program
            Handle = GL.CreateProgram();
            // compile new shaders
            int Vert = CompileShader(ShaderType.VertexShader, vertexpath);
            int Frag = CompileShader(ShaderType.FragmentShader, fragmentpath);

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

            GetUniforms();

            // detach and delete both shaders
            GL.DetachShader(Handle, Vert);
            GL.DetachShader(Handle, Frag);
            GL.DeleteShader(Vert);
            GL.DeleteShader(Frag);
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
            Handle = GL.CreateProgram();
            // compile new shaders
            int Vert = CompileShader(ShaderType.VertexShader, vertexpath);
            
            int Geom = CompileShader(ShaderType.GeometryShader, geometrypath);
            
            int Frag = CompileShader(ShaderType.FragmentShader, fragmentpath);

            // attach new shaders
            GL.AttachShader(Handle, Vert);
            GL.AttachShader(Handle, Geom);
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

            GetUniforms();

            // detach and delete both shaders
            GL.DetachShader(Handle, Vert);
            GL.DetachShader(Handle, Geom);
            GL.DetachShader(Handle, Frag);
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
    }
}
