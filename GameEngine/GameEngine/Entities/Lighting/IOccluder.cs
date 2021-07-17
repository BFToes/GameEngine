namespace GameEngine.Entities.Lighting
{
    interface IOccluder
    {
        public void Occlude(ILight Light);
    }
}
