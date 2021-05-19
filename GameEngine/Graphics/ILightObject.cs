using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Graphics.Shaders;
namespace Graphics
{
    interface ILightObject
    {
        public float Intensity { get; set; }
        public Vector3 Colour { get; set; }
        public void Render();
    }
    class PntLight : ILightObject
    {
        private Matrix4 Projection;
        private Transform Transform = new Transform();
        public Vector3 Position { get => Transform.Position; set => Transform.Position = value; }
        public float Intensity { get; set; }
        public Vector3 Colour { get; set; }

        public PntLight(Vector3 Position, Vector3 Colour, float Intensity, float CurveExp, float CurveLin, float CurveCon)
        {
            this.Colour = Colour;
            this.Intensity = Intensity;

            Transform.Scale = new Vector3(CalcScale(Colour, CurveExp, CurveLin, CurveCon) * Intensity);
            this.Position = Position; // also sets model matrix uniform 
            
        }
        public void Render() 
        {
            

        }

        private float CalcScale(Vector3 Colour, float CurveExp, float CurveLin, float CurveCon)
        {
            float MaxChannel = MathF.Max(MathF.Max(Colour.X, Colour.Y), Colour.Z);
            return (-CurveLin + MathF.Sqrt(CurveLin * CurveLin - 4 * CurveExp * (CurveCon - 256 * MaxChannel * Intensity))) / 2 / CurveExp;
        }
    }
}
