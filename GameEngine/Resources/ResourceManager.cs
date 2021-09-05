using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Drawing;
using GameEngine.Components;

namespace GameEngine.Resources
{
    internal class ResourceManager
    {
        public readonly Dictionary<string, Sampler> samplerDict;
        public readonly Dictionary<string, Mesh> meshDict;

        public Mesh GetMesh(string filePath)
        {
            if (meshDict.ContainsKey(filePath)) return meshDict[filePath];
            throw new Exception("needs to load mesh from file");
        }

        public Sampler GetSampler(string filePath)
        {
            if (!samplerDict.ContainsKey(filePath))
            {
                float[] data = ReadSamplerFile(out int width, out int height, filePath);

                if (height == 1)
                    samplerDict[filePath] = new Sampler1D(data, width, 4);
                else
                    samplerDict[filePath] = new Sampler2D(data, width, height, 4);
            }
            return samplerDict[filePath];
        }

        //public Material GetMaterial(string filePath) { }

        private static float[] ReadSamplerFile(out int width, out int height, string path)
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



        private static void RegisterBufferAttribute<T>(int VAO, ref int Location, ref int ByteLocation) where T : unmanaged
        {
            int ByteSize; // the size of this type in bytes
            unsafe { ByteSize = sizeof(T); }

            GL.VertexArrayAttribBinding(VAO, Location, 0); // generates a new attribute binding to location in vertex buffer array
            GL.EnableVertexArrayAttrib(VAO, Location); // enables the attribute binding to location
            GL.VertexArrayAttribFormat(VAO, Location, ByteSize / 4, VertexAttribType.Float, false, ByteLocation); // defines attribute location, ByteSize/4 = FloatSize

            Location++; // increments Location
            ByteLocation += ByteSize; // Adds ByteSize to ByteLocation
        }
    }
}
