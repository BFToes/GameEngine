using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Entities.Lighting
{
    interface IOccluder
    {
        public void Occlude(ILight Light);
    }
}
