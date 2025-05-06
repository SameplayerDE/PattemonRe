using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HxGLTF.Core;

namespace HxGLTF.Monogame;

public class GameMeshPrimitives
{
    public VertexBuffer VertexBuffer;
    public int VertexCount;
    public IndexBuffer IndexBuffer;
    public int IndexCount;
    public bool IsIndexed;
    public GameMaterial Material;
    public Vector3 LocalPosition;

    // For Bounding Box Calculations
    public Vector3[] Positions;
    public Vector4[] Weights;
    public Vector4[] Joints;

    public void Dispose()
    {
        VertexBuffer?.Dispose();
        VertexBuffer = null;

        IndexBuffer?.Dispose();
        IndexBuffer = null;

        Material = null;
    }
    
    public static GameMeshPrimitives From(GraphicsDevice graphicsDevice, GLTFFile file, MeshPrimitive primitive)
    {
        var result = new GameMeshPrimitives
        {
            Material = GameMaterial.From(graphicsDevice, file, primitive.Material)
        };


        var accessorContext = new AccessorReaderContext();
        var tasks = new Dictionary<string, Task<float[]>>();

        // Starte alle Accessor-Leseoperationen parallel
        foreach (var attribute in primitive.Attributes)
        {
            var key = attribute.Key;
            var accessor = attribute.Value;
            tasks[key] = Task.Run(() => accessorContext.Read(accessor));
        }

        // Indizes synchron einlesen
        int[] indices = null;
        if (primitive.HasIndices)
        {
            indices = AccessorReader.ReadIndices(primitive.Indices);
        }

        Task.WaitAll(tasks.Values.ToArray()); // Warten auf alle Accessor-Tasks

        // Konvertiere die gelesenen Daten
        float[] posData = tasks.TryGetValue("POSITION", out var p) ? p.Result : null;
        float[] normData = tasks.TryGetValue("NORMAL", out var n) ? n.Result : null;
        float[] uvData = tasks.TryGetValue("TEXCOORD_0", out var u) ? u.Result : null;
        float[] colData = tasks.TryGetValue("COLOR_0", out var c) ? c.Result : null;
        float[] jointData = tasks.TryGetValue("JOINTS_0", out var j) ? j.Result : null;
        float[] weightData = tasks.TryGetValue("WEIGHTS_0", out var w) ? w.Result : null;

        int vertexCount = posData?.Length / 3 ?? 0;
        var vertices = new List<VertexPositionColorNormalTextureBlend>(vertexCount);
        var positions = new Vector3[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            // === Position ===
            float posX = posData[i * 3 + 0];
            float posY = posData[i * 3 + 1];
            float posZ = posData[i * 3 + 2];
            var position = new Vector3(posX, posY, posZ);

            // === Normal ===
            Vector3 normal = Vector3.Up;
            if (normData != null)
            {
                float normX = normData[i * 3 + 0];
                float normY = normData[i * 3 + 1];
                float normZ = normData[i * 3 + 2];
                normal = new Vector3(normX, normY, normZ);
            }

            // === Texture Coordinates ===
            Vector2 uv = Vector2.Zero;
            if (uvData != null)
            {
                float uvX = uvData[i * 2 + 0];
                float uvY = uvData[i * 2 + 1];
                uv = new Vector2(uvX, uvY);
            }

            // === Color ===
            Color color = Color.White;
            if (colData != null)
            {
                if (colData.Length >= (i * 4 + 4))
                {
                    float r = colData[i * 4 + 0];
                    float g = colData[i * 4 + 1];
                    float b = colData[i * 4 + 2];
                    float a = colData[i * 4 + 3];
                    color = new Color(r, g, b, a);
                }
                else if (colData.Length >= (i * 3 + 3))
                {
                    float r = colData[i * 3 + 0];
                    float g = colData[i * 3 + 1];
                    float b = colData[i * 3 + 2];
                    color = new Color(r, g, b, 1f);
                }
            }

            // === Joints ===
            Vector4 joint = Vector4.Zero;
            if (jointData != null)
            {
                joint = new Vector4(
                    jointData[i * 4 + 0],
                    jointData[i * 4 + 1],
                    jointData[i * 4 + 2],
                    jointData[i * 4 + 3]
                );
            }

            // === Weights ===
            Vector4 weight = Vector4.UnitX;
            if (weightData != null)
            {
                weight = new Vector4(
                    weightData[i * 4 + 0],
                    weightData[i * 4 + 1],
                    weightData[i * 4 + 2],
                    weightData[i * 4 + 3]
                );
            }

            var vertex = new VertexPositionColorNormalTextureBlend
            {
                Position = position,
                Normal = normal,
                TextureCoordinate = uv,
                Color = color,
                BlendIndices = joint,
                BlendWeight = weight
            };

            positions[i] = position;
            vertices.Add(vertex);
        }
        
        result.Positions = positions;
        result.LocalPosition = positions.Aggregate(Vector3.Zero, (a, b) => a + b) / positions.Length;

        if (indices != null)
        {
            result.IsIndexed = true;
            result.IndexCount = indices.Length;
            result.IndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length,
                BufferUsage.WriteOnly);
            result.IndexBuffer.SetData(indices);
        }
        else
        {
            result.IsIndexed = false;
            result.IndexCount = vertices.Count;
        }

        result.VertexBuffer = new VertexBuffer(graphicsDevice,
            VertexPositionColorNormalTextureBlend.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
        result.VertexBuffer.SetData(vertices.ToArray());

        return result;
    }
}


public class GameMesh
{
    public GameMeshPrimitives[] Primitives;
    public float[] Weights;

    public void Dispose()
    {
        if (Primitives != null)
        {
            foreach (var primitive in Primitives)
            {
                primitive.Dispose();
            }

            Primitives = null;
        }

        Weights = null;
    }

    public static GameMesh From(GraphicsDevice graphicsDevice, GLTFFile file, Mesh mesh)
    {
        if (mesh == null)
        {
            throw new ArgumentNullException(nameof(mesh));
        }

        GameMesh result = new GameMesh
        {
            Primitives = new GameMeshPrimitives[mesh.Primitives.Length]
        };

        for (int i = 0; i < mesh.Primitives.Length; i++)
        {
            result.Primitives[i] = GameMeshPrimitives.From(graphicsDevice, file, mesh.Primitives[i]);
        }

        return result;
    }
}