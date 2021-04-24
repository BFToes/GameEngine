using OpenTK.Mathematics;
namespace Graphics
{
    public interface IVertex
    {
        public int SizeInBytes { get; }
    }
    public struct Vertex2D : IVertex
    {
        public int SizeInBytes { get => 16; }

        public Vector2 Position; // 2 floats = 8 bytes
        public Vector2 TextureUV; // 2 floats = 8 bytes
        public Vertex2D(Vector2 Position, Vector2 TextureUV)
        {
            this.Position = Position;
            this.TextureUV = TextureUV;

        }
        public Vertex2D(float PositionX, float PositionY, float TextureU, float TextureV)
        {
            this.Position = new Vector2(PositionX, PositionY);
            this.TextureUV = new Vector2(TextureU, TextureV);
        }
    }
    public struct SimpleVertex : IVertex
    {
        public int SizeInBytes { get => 8; }

        public Vector2 Position; // 2 floats = 8 bytes
        public SimpleVertex(Vector2 Position)
        {
            this.Position = Position;
        }
        public SimpleVertex(float PositionX, float PositionY)
        {
            this.Position = new Vector2(PositionX, PositionY);
        }
    }
    public struct Vertex3D : IVertex
    {
        public int SizeInBytes { get => 32; }

        public Vector3 Position; // 3 floats = 12 bytes
        public Vector3 Normal; // 3 float = 12 bytes
        public Vector2 UV; // 2 floats = 8 bytes

        public Vertex3D(Vector3 Position, Vector3 Normal, Vector2 TextureUV, Color4 Colour)
        {
            this.Position = Position;
            this.Normal = Normal;
            this.UV = TextureUV;

        }
        public Vertex3D(float PositionX, float PositionY, float PositionZ, float NormalX, float NormalY, float NormalZ, float TextureU, float TextureV)
        {
            this.Position = new Vector3(PositionX, PositionY, PositionZ);
            this.Normal = new Vector3(NormalX, NormalY, NormalZ);
            this.UV = new Vector2(TextureU, TextureV);
        }
    }
}
