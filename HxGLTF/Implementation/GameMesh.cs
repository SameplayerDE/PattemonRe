using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HxGLTF.Implementation
{
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
        
        public float DistanceToCamera(Vector3 cameraPosition, Matrix worldMatrix)
        {
            // Transformiere die lokale Position der Primitive in den Welt-Raum
            var worldPosition = Vector3.Transform(LocalPosition, worldMatrix);

            // Berechne die Distanz zur Kamera
            return Vector3.Distance(worldPosition, cameraPosition);
        }
        
        public static GameMeshPrimitives From(GraphicsDevice graphicsDevice, GLTFFile file, MeshPrimitive primitive)
        {

            var result = new GameMeshPrimitives
            {
                Material = GameMaterial.From(graphicsDevice, file, primitive.Material)
            };

            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var colors = new List<Color>();
            var uvs = new List<Vector2>();
            var joints = new List<Vector4>();
            var weights = new List<Vector4>();
            
            foreach (var attribute in primitive.Attributes)
            {
                var accessor = attribute.Value;
                var data = AccessorReader.ReadData(accessor);
                var compCount = accessor.Type.NumberOfComponents;

                for (var i = 0; i < accessor.Count; i++)
                {
                    var baseIndex = i * compCount;

                    switch (attribute.Key)
                    {
                        case "POSITION":
                            positions.Add(new Vector3(data[baseIndex], data[baseIndex + 1], data[baseIndex + 2]));
                            break;
                        case "NORMAL":
                            normals.Add(new Vector3(data[baseIndex], data[baseIndex + 1], data[baseIndex + 2]));
                            break;
                        case "TEXCOORD_0":
                            uvs.Add(new Vector2(data[baseIndex], data[baseIndex + 1]));
                            break;
                        case "COLOR_0":
                            colors.Add(compCount == 4
                                ? new Color(data[baseIndex], data[baseIndex + 1], data[baseIndex + 2], data[baseIndex + 3])
                                : new Color(data[baseIndex], data[baseIndex + 1], data[baseIndex + 2], 1f));
                            break;
                        case "JOINTS_0":
                            joints.Add(new Vector4(data[baseIndex], data[baseIndex + 1], data[baseIndex + 2], data[baseIndex + 3]));
                            break;
                        case "WEIGHTS_0":
                            weights.Add(new Vector4(data[baseIndex], data[baseIndex + 1], data[baseIndex + 2], data[baseIndex + 3]));
                            break;
                    }
                }
            }

            if (primitive.HasIndices)
            {
                int[] indices = AccessorReader.ReadIndices(primitive.Indices);
                result.IndexCount = indices.Length;
                result.IsIndexed = true;

                var finalVertices = new List<VertexPositionColorNormalTextureBlend>();
                var finalIndices = new List<int>();
                var vertexMap = new Dictionary<VertexPositionColorNormalTextureBlend, int>();

                for (int i = 0; i < indices.Length; i++)
                {
                    int idx = indices[i];
                    Vector3 pos = positions[idx];
                    Vector3 norm = normals.Count > idx ? normals[idx] : Vector3.Up;
                    Vector2 uv = uvs.Count > idx ? uvs[idx] : Vector2.Zero;
                    Color col = colors.Count > idx ? colors[idx] : Color.White;
                    Vector4 joint = joints.Count > idx ? joints[idx] : Vector4.Zero;
                    Vector4 weight = weights.Count > idx ? weights[idx] : Vector4.UnitX;

                    var vertex = new VertexPositionColorNormalTextureBlend(pos, col, norm, uv, joint, weight);

                    if (!vertexMap.TryGetValue(vertex, out int vertexIndex))
                    {
                        vertexIndex = finalVertices.Count;
                        vertexMap[vertex] = vertexIndex;
                        finalVertices.Add(vertex);
                    }

                    finalIndices.Add(vertexIndex);
                }

                result.VertexBuffer = new VertexBuffer(
                    graphicsDevice,
                    VertexPositionColorNormalTextureBlend.VertexDeclaration,
                    finalVertices.Count,
                    BufferUsage.WriteOnly
                );
                result.VertexBuffer.SetData(finalVertices.ToArray());

                result.IndexBuffer = new IndexBuffer(
                    graphicsDevice,
                    IndexElementSize.ThirtyTwoBits,
                    finalIndices.Count,
                    BufferUsage.WriteOnly
                );
                result.IndexBuffer.SetData(finalIndices.ToArray());
            }
            else
            {
                result.IsIndexed = false;
                result.IndexCount = 0;

                var vertexList = new List<VertexPositionColorNormalTextureBlend>();

                for (int i = 0; i < positions.Count; i++)
                {
                    var pos = positions[i];
                    var norm = (normals.Count > i) ? normals[i] : Vector3.Up;
                    var uv = (uvs.Count > i) ? uvs[i] : Vector2.Zero;
                    var col = (colors.Count > i) ? colors[i] : Color.White;
                    var joint = (joints.Count > i) ? joints[i] : Vector4.Zero;
                    var weight = (weights.Count > i) ? weights[i] : Vector4.UnitX;

                    vertexList.Add(new VertexPositionColorNormalTextureBlend(pos, col, norm, uv, joint, weight));
                }

                result.VertexCount = vertexList.Count;
                result.VertexBuffer = new VertexBuffer(
                    graphicsDevice,
                    VertexPositionColorNormalTextureBlend.VertexDeclaration,
                    vertexList.Count,
                    BufferUsage.WriteOnly
                );
                result.VertexBuffer.SetData(vertexList.ToArray());
            }

            result.Positions = positions.ToArray();
            result.LocalPosition = positions.Aggregate(Vector3.Zero, (a, b) => a + b) / positions.Count;

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
}