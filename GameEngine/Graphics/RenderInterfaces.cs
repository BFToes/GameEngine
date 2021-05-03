using System;
using System.Collections.Generic;
using System.Text;

namespace Graphics
{
    interface IRenderable
    {
        public event Action<bool> Set_Visible;
        public bool Visible { get; set; }
        public void OnRender();
    }
    interface IRenderObject : IRenderable
    {
        public void LightRender();

    }
    interface IRenderLight : IRenderable
    {

    }
}
