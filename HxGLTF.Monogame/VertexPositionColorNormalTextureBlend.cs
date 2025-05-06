using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HxGLTF.Monogame;

public struct VertexPositionColorNormalTextureBlend : IVertexType
{
    public Vector3 Position;
    public Color Color;
    public Vector3 Normal;
    public Vector2 TextureCoordinate;
    public Vector4 BlendIndices;
    public Vector4 BlendWeight;
    
    public VertexPositionColorNormalTextureBlend(Vector3 position)
    {
        Position = position;
        Color = Color.White;
        Normal = Vector3.Zero;
        TextureCoordinate = Vector2.Zero;
        BlendIndices = new Vector4(0, 0, 0, 0); // Neutraler Wert
        BlendWeight = new Vector4(1, 0, 0, 0);  // Neutraler Wert
    }

    public VertexPositionColorNormalTextureBlend(Vector3 position, Color color)
    {
        Position = position;
        Color = color;
        Normal = Vector3.Zero;
        TextureCoordinate = Vector2.Zero;
        BlendIndices = new Vector4(0, 0, 0, 0); // Neutraler Wert
        BlendWeight = new Vector4(1, 0, 0, 0);  // Neutraler Wert
    }

    public VertexPositionColorNormalTextureBlend(Vector3 position, Color color, Vector3 normal)
    {
        Position = position;
        Color = color;
        Normal = normal;
        TextureCoordinate = Vector2.Zero;
        BlendIndices = new Vector4(0, 0, 0, 0); // Neutraler Wert
        BlendWeight = new Vector4(1, 0, 0, 0);  // Neutraler Wert
    }

    public VertexPositionColorNormalTextureBlend(Vector3 position, Color color, Vector3 normal, Vector2 uv)
    {
        Position = position;
        Color = color;
        Normal = normal;
        TextureCoordinate = uv;
        BlendIndices = new Vector4(0, 0, 0, 0); // Neutraler Wert
        BlendWeight = new Vector4(1, 0, 0, 0);  // Neutraler Wert
    }
    
    public VertexPositionColorNormalTextureBlend(Vector3 position, Color color, Vector3 normal, Vector2 uv, Vector4 bi, Vector4 bw)
    {
        Position = position;
        Color = color;
        Normal = normal;
        TextureCoordinate = uv;
        BlendIndices = bi; // Neutraler Wert
        BlendWeight = bw;  // Neutraler Wert
    }
    
    public VertexPositionColorNormalTextureBlend(VertexPositionColorNormalTextureBlend data, Vector3 normal)
    {
        Position = data.Position;
        Color = data.Color;
        Normal = normal;
        TextureCoordinate = data.TextureCoordinate;
        BlendIndices = data.BlendIndices; // Neutraler Wert
        BlendWeight = data.BlendWeight;  // Neutraler Wert
    }

    public static void CalculateNormals(VertexPositionColorNormalTextureBlend[] data)
    {

        for (int i = 0; i < data.Length / 3; i++)
        {
            var ab = data[i * 3].Position - data[i * 3 + 1].Position;
            var cb = data[i * 3 + 2].Position - data[i * 3 + 1].Position;

            ab.Normalize();
            cb.Normalize();

            var normal = Vector3.Cross(ab, cb);

            data[i * 3] = new VertexPositionColorNormalTextureBlend(data[i * 3], normal);
            data[i * 3 + 1] = new VertexPositionColorNormalTextureBlend(data[i * 3 + 1], normal);
            data[i * 3 + 2] = new VertexPositionColorNormalTextureBlend(data[i * 3 + 2], normal);
        }
    }

    public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
    (
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
        new VertexElement(sizeof(float) * 3 + sizeof(byte) * 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
        new VertexElement(sizeof(float) * 6 + sizeof(byte) * 4, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(sizeof(float) * 8 + sizeof(byte) * 4, VertexElementFormat.Vector4, VertexElementUsage.BlendIndices, 0),
        new VertexElement(sizeof(float) * 12 + sizeof(byte) * 4, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0)
    );
    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

}