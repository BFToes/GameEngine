using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Graphics.Shaders;
namespace Graphics
{
    abstract class Light : IRenderable
    {
        public virtual float Intensity { get; set; }
        public virtual Vector3 Colour { get; set; }
        public abstract void Render();
    }
    class DirLight : Light 
    {
        public override void Render()
        {

        }
    }
    class PntLight : Light
    {
        private Matrix4 Projection;
        //private static ShaderProgram LightProgram = ShaderProgram.From("$Resources/ShaderScripts/Light Shaders/Light.vert", "$Resources/ShaderScripts/Light Shaders/Light.frag");
        private Transform Transform = new Transform();
        public Vector3 Position 
        { 
            get => Transform.Position; set
            {
                Transform.Position = value;
                //Material.SetUniform("Model", Transform.Matrix);
            }
        }
        // maybe not vertex 3D dont need normal dont need texel
        private static Vertex3D[] Mesh = new Vertex3D[] { new Vertex3D(), new Vertex3D(), new Vertex3D(), new Vertex3D()};
        public PntLight(Vector3 Position, Vector3 Colour, float Intensity, Vector3 LightCurve)
        {
            this.Colour = Colour;
            this.Intensity = Intensity;

            Transform.Scale = new Vector3(CalcScale(Colour, LightCurve));
            this.Position = Position; // also sets model matrix uniform 
            
        }
        public override void Render()
        {
            
            
        }

        private float CalcScale(Vector3 Colour, Vector3 LC)
        {
            float MaxChannel = MathF.Max(MathF.Max(Colour.X, Colour.Y), Colour.Z);
            return (-LC.Y + MathF.Sqrt(LC.Y * LC.Y - 4 * LC.Z * (LC.X - 256 * MaxChannel * Intensity))) / 2 / LC.Z;
        }
    }
    class SptLight : Light 
    {
        public override void Render()
        {

        }
    }
}
