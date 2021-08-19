using GameEngine.Entities.Culling;
using System;

namespace GameEngine.Entities.Lighting
{
    [Obsolete]
    interface IOccluder : ICullable<Sphere>
    {
        public void Occlude(IVolumeLight Light);
    }
}
