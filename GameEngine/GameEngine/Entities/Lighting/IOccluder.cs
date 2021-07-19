using GameEngine.Entities.Culling;
namespace GameEngine.Entities.Lighting
{
    interface IOccluder : ICullable<Sphere>
    {
        public void Occlude(IVolumeLight Light);
    }
}
