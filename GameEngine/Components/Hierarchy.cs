using System;
using System.Collections.Generic;
using System.Text;
using ECS;
namespace GameEngine.Components
{
    struct Hierarchy : IComponent
    {
        public int Layer;
        private Entity _parent;
        public Entity parent
        {
            get => _parent;
            set
            {
                if (value != null && value.HasComponent<Hierarchy>())
                {
                    _parent = value;
                    Layer = value.GetComponent<Hierarchy>().Layer + 1;
                }
                _parent = null;
                Layer = 0;
            }
        }

        public Hierarchy(Entity parent)
        {
            if (parent != null && parent.HasComponent<Hierarchy>())
            {
                _parent = parent;
                Layer = parent.GetComponent<Hierarchy>().Layer + 1;
            }
            else
            {
                _parent = null;
                Layer = 0;
            }
        }
    }
}
