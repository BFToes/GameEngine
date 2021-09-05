using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.IO;
using System;

namespace GameEngine.Resources
{

    public interface Sampler
    {
        int ID { get; }
        void Save(string filepath);
    }


    public interface ISampler1D : Sampler
    {
        int Width { get; }
    }
    public class Sampler1D : ISampler1D
    {
        public int ID { get; }
        public int Width { get; }

        public Sampler1D(float[] data, int width, int mipmap,
            TextureMinFilter minFilter = TextureMinFilter.Nearest,
            TextureMagFilter magFilter = TextureMagFilter.Nearest,
            TextureWrapMode WrapMode = TextureWrapMode.ClampToBorder)
        {
            ID = GL.GenTexture();
            Width = width;

            GL.BindTexture(TextureTarget.Texture1D, ID);
            GL.TextureStorage1D(ID, mipmap, SizedInternalFormat.Rgba32f, width);
            GL.TextureSubImage1D(ID, 0, 0, Width, PixelFormat.Rgba, PixelType.Float, data);

            if (mipmap > 1) GL.GenerateMipmap(GenerateMipmapTarget.Texture1D);

            GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMinFilter, (int)minFilter); // minify filter mode
            GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMagFilter, (int)magFilter); // magnify filter mode
            GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureWrapS, (int)WrapMode);
            GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureWrapT, (int)WrapMode);
        }

        public void Save(string filepath)
        {
            GL.BindTexture(TextureTarget.Texture1D, ID);

            float[] Data = new float[Width * 4];
            GL.GetTexImage(TextureTarget.Texture1D, 0, PixelFormat.Rgba, PixelType.Float, Data);
            byte[] byteData = new byte[Data.Length * sizeof(float)];
            System.Buffer.BlockCopy(Data, 0, byteData, 0, byteData.Length);

            Image.FromStream(new MemoryStream(byteData)).Save(filepath);
        }
    }



    public interface ISampler2D : Sampler
    {
        int Width { get; }
        int Height { get; }
    }
    class Sampler2D : ISampler2D
    {
        public int ID { get; }
        public int Width { get; }
        public int Height { get; }

        public Sampler2D(float[] data, int width, int height, int mipmap, 
            TextureMinFilter minFilter = TextureMinFilter.Nearest, 
            TextureMagFilter magFilter = TextureMagFilter.Nearest, 
            TextureWrapMode WrapMode = TextureWrapMode.ClampToBorder)
        {
            ID = GL.GenTexture();
            Width = width;
            Height = height;

            GL.BindTexture(TextureTarget.Texture2D, ID);
            GL.TextureStorage2D(ID, mipmap, SizedInternalFormat.Rgba32f, width, height);
            GL.TextureSubImage2D(ID, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.Float, data);
            
            if (mipmap > 1) GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter); // minify filter mode
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter); // magnify filter mode
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)WrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)WrapMode);
        }
        
        public void Save(string filepath)
        {
            GL.BindTexture(TextureTarget.Texture1D, ID);

            float[] Data = new float[Width * Height * 4];
            GL.GetTexImage(TextureTarget.Texture1D, 0, PixelFormat.Rgba, PixelType.Float, Data);
            byte[] byteData = new byte[Data.Length * sizeof(float)];
            System.Buffer.BlockCopy(Data, 0, byteData, 0, byteData.Length);

            Image.FromStream(new MemoryStream(byteData)).Save(filepath);
        }
    }    
}
