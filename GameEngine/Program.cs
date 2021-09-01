using System;
using ECS;
using OpenTK.Graphics.OpenGL4;
namespace GameEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            var window = RenderWindow.New(null, true, 700, 700, "a title of funny names");
            window.Run();
        }
    }
}