using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System;
using System.IO;
using System.Linq;
namespace GameEngine.Resources
{
    [Obsolete]
    static class Texture
    {
        private static Dictionary<string, int> SamplerDict = new Dictionary<string, int>();

        public static int Sampler(string path)
        {
            if (SamplerDict.ContainsKey(path))
                return SamplerDict[path];
            else
            {
                switch ((Path.GetExtension(path)))
                {
                    case ".png": return Add_Sampler(path);
                    default: throw new Exception("File extensiion");
                }

            }
        }
        /// <summary>
        /// opens image file and creates texture from contents
        /// </summary>
        /// <param name="path">The path of the texture file.</param>
        /// <returns>The texture handle ID for OpenGL.</returns>
        public static int Add_Sampler(string path, TextureMinFilter MinifyFilter = TextureMinFilter.Nearest, TextureMagFilter MagnifyFilter = TextureMagFilter.Nearest, TextureWrapMode WrapMode = TextureWrapMode.ClampToBorder, int Mipmap = 1)
        {
            int width, height;
            float[] data = ReadFile(out width, out height, path);

            int Handle = Create_Sampler(data, width, height, MinifyFilter, MagnifyFilter, WrapMode, Mipmap);

            if (SamplerDict.ContainsKey(path)) GL.DeleteTexture(SamplerDict[path]); // if texture path already exists overwrite it
            SamplerDict[path] = Handle;

            

            return Handle;
        }
        /// <summary>
        /// opens image file and creates texture from contents
        /// </summary>
        /// <param name="path">The path of the texture file.</param>
        /// <returns>The texture handle ID for OpenGL.</returns>
        public static int Create_Sampler(float[] data, int width, int height, TextureMinFilter MinifyFilter = TextureMinFilter.Nearest, TextureMagFilter MagnifyFilter = TextureMagFilter.Nearest, TextureWrapMode WrapMode = TextureWrapMode.ClampToBorder, int Mipmap = 0)
        {
            int Handle = GL.GenTexture();
            
            if (height > 1)
            {
                GL.BindTexture(TextureTarget.Texture2D, Handle); // bind texture to slot
                GL.TextureStorage2D(Handle, Mipmap, SizedInternalFormat.Rgba32f, width, height);
                GL.TextureSubImage2D(Handle, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.Float, data);

                if (Mipmap > 1) GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)MinifyFilter); // minify filter mode
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)MagnifyFilter); // magnify filter mode
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)WrapMode);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)WrapMode);

                
            }
            else
            {
                GL.BindTexture(TextureTarget.Texture1D, Handle); // bind texture to slot
                GL.TextureStorage1D(Handle, 1, SizedInternalFormat.Rgba32f, width);
                GL.TextureSubImage1D(Handle, 0, 0, width, PixelFormat.Rgba, PixelType.Float, data);
                
                GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMinFilter, (int)MinifyFilter); // minify filter mode
                GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMagFilter, (int)MagnifyFilter); // magnify filter mode
                GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureWrapS, (int)WrapMode);              
            }
            return Handle;
        }
        /// <summary>
        /// Serializes image read from file for openGl buffer.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="path">The path to the image.</param>
        /// <returns>The serialised data read from the file in rgba format.</returns>
        private static float[] ReadFile(out int width, out int height, string path)
        {
            Bitmap BMP = (Bitmap)Image.FromFile(path);

            width = BMP.Width;
            height = BMP.Height;
            float[] Serialized_Data = new float[width * height * 4];
            int index = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color P = BMP.GetPixel(x, y);
                    Serialized_Data[index++] = P.R / 255f;
                    Serialized_Data[index++] = P.G / 255f;
                    Serialized_Data[index++] = P.B / 255f;
                    Serialized_Data[index++] = P.A / 255f;
                }
            }

            return Serialized_Data;
        }

        public static void SaveSampler(string path, int Sampler, TextureTarget Target, System.Drawing.Imaging.ImageFormat ImageFormat)
        {
            Byte[] Data = GetSamplerData(Sampler, Target, out _, out _).Select(F => (Byte)F).ToArray();

            using (Image image = Image.FromStream(new MemoryStream(Data)))
            {
                image.Save(path, ImageFormat);
            }

        }

        public static float[] GetSamplerData(int Sampler, TextureTarget Target, out int Width, out int Height)
        {
            GL.BindTexture(Target, Sampler);
            GL.GetTexLevelParameter(Target, 0, GetTextureParameter.TextureWidth, out Width);
            GL.GetTexLevelParameter(Target, 0, GetTextureParameter.TextureHeight, out Height);
            
            float[] Data = new float[Width * Height * 4];
            GL.GetTexImage(Target, 0, PixelFormat.Rgba, PixelType.Float, Data);
            return Data;
        }
        
        private static void DebugSamplerdataInput(int Handle, int width, int height, float[] data)
        {
            float[] Data = GetSamplerData(Handle, TextureTarget.Texture1D, out int W, out int H);

            Console.Write($"in:  width: {width}, height: {height} data: [");
            foreach (float F in data)
                Console.Write($"{F}, ");
            Console.Write($"]\nout: width: {W}, height: {H} data: [");
            foreach (float F in Data)
                Console.Write($"{F}, ");
            Console.Write("]\n");
        }
    }
}
