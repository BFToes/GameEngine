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
    public struct Simple2D : IVertex
    {
        public int SizeInBytes { get => 8; }

        public Vector2 Position; // 2 floats = 8 bytes
        public Simple2D(Vector2 Position)
        {
            this.Position = Position;
        }
        public Simple2D(float x, float y)
        {
            this.Position = new Vector2(x, y);
        }
        public override string ToString() =>  $"{Position.X}, {Position.Y}";
    }
    public struct Simple3D : IVertex
    {
        public int SizeInBytes { get => 12; }
        public Vector3 Position;
        public Simple3D(Vector3 Position)
        {
            this.Position = Position;
        }
        public Simple3D(float x, float y, float z)
        {
            this.Position = new Vector3(x, y, z);
        }
        public override string ToString() => $"{Position.X}, {Position.Y}, {Position.Z}";
    }
    public struct Vertex3D : IVertex
    {
        public int SizeInBytes { get => 32; }

        public Vector3 Position; // 3 floats = 12 bytes
        public Vector3 Normal; // 3 float = 12 bytes
        public Vector2 UV; // 2 floats = 8 bytes

        public Vertex3D(Vector3 Position, Vector3 Normal, Vector2 TextureUV)
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
