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
            var result = new GameMeshPrimitives();

            result.Material = GameMaterial.From(graphicsDevice, file, primitive.Material);

            var position = new List<Vector3>();
            var normals = new List<Vector3>();
            var colors = new List<Color>();
            var uvs = new List<Vector2>();
            var joints = new List<Vector4>();
            var w = new List<Vector4>();

   
            
            foreach (var attribute in primitive.Attributes)
            {
               //Console.WriteLine(attribute.Key);
               //Console.WriteLine(attribute.Value.Type.Id);

                var dataAccessor = attribute.Value;
                var elementCount = dataAccessor.Count;
                var numberOfComponents = dataAccessor.Type.NumberOfComponents;

                float[] data;

                
                
                if (primitive.HasIndices)
                {
                    data = AccessorReader.ReadDataIndexed(dataAccessor, primitive.Indices);

                    //Console.WriteLine("Key: " + attribute.Key + ", Type: " + dataAccessor.Type.Id);

                    if (attribute.Key == "POSITION" && dataAccessor.Type.Id == "VEC3")
                    {
                        for (var x = 0; x < data.Length; x += 3)
                        {
                            position.Add(new Vector3(data[x], data[x + 1], data[x + 2]));
                        }
                    }
                    
                    if (attribute.Key == "COLOR_0" && dataAccessor.Type.Id == "VEC4")
                    {
                        for (var x = 0; x < data.Length; x += 4)
                        {
                            var color = new Color(data[x] / 255f, data[x + 1] / 255f, data[x + 2] / 255f, data[x + 3] / 255f);
                            colors.Add(color);
                        }
                    }
                    
                    if (attribute.Key == "COLOR_0" && dataAccessor.Type.Id == "VEC3")
                    {
                        for (var x = 0; x < data.Length; x += 3)
                        {
                            var color = new Color(data[x] / 255f, data[x + 1] / 255f, data[x + 2] / 255f, 1);
                            colors.Add(color);
                        }
                    }

                    if (attribute.Key == "NORMAL" && dataAccessor.Type.Id == "VEC3")
                    {
                        for (var x = 0; x < data.Length; x += 3)
                        {
                            normals.Add(new Vector3(data[x], data[x + 1], data[x + 2]));
                        }
                    }

                    if (attribute.Key == "TEXCOORD_0" && dataAccessor.Type.Id == "VEC2")
                    {
                        for (var x = 0; x < data.Length; x += 2)
                        {
                            uvs.Add(new Vector2(data[x], data[x + 1]));
                        }
                    }
                    
                    if (attribute.Key == "JOINTS_0" && dataAccessor.Type.Id == "VEC4")
                    {
                        for (var x = 0; x < data.Length; x += dataAccessor.Type.NumberOfComponents)
                        {
                            joints.Add(new Vector4(data[x], data[x + 1], data[x + 2], data[x + 3]));
                        }
                    }
                    
                    if (attribute.Key == "WEIGHTS_0" && dataAccessor.Type.Id == "VEC4")
                    {
                        for (var x = 0; x < data.Length; x += 4)
                        {
                            var vector = new Vector4(
                                data[x + 0],
                                data[x + 1],
                                data[x + 2],
                                data[x + 3]
                            );
                            w.Add(vector);
                        }
                    }

                    if (attribute.Key == "COLOR_0" && dataAccessor.Type.Id == "VEC3")
                    {
                        
                    }
                }
                else
                {
                    data = AccessorReader.ReadData(dataAccessor);

                    //Console.WriteLine("Key: " + attribute.Key + ", Type: " + dataAccessor.Type.Id);
                    
                    for (var i = 0; i < dataAccessor.Count; i++)
                    {
                        if (attribute.Key == "POSITION" && dataAccessor.Type.Id == "VEC3")
                        {
                            position.Add(new Vector3(
                                data[i * numberOfComponents + 0],
                                data[i * numberOfComponents + 1],
                                data[i * numberOfComponents + 2]
                            ));
                        }
                        else if (attribute.Key == "NORMAL" && dataAccessor.Type.Id == "VEC3")
                        {
                            normals.Add(new Vector3(
                                data[i * numberOfComponents + 0],
                                data[i * numberOfComponents + 1],
                                data[i * numberOfComponents + 2]
                            ));
                        }
                        else if (attribute.Key == "COLOR_0" && dataAccessor.Type.Id == "VEC4")
                        {
                            for (var x = 0; x < data.Length; x += 4)
                            {
                                var color = new Color(
                                    data[i * numberOfComponents + 0] / 255f,
                                    data[i * numberOfComponents + 1] / 255f,
                                    data[i * numberOfComponents + 2] / 255f,
                                    data[i * numberOfComponents + 3] / 255f);
                                colors.Add(color);
                            }
                        }
                        else if (attribute.Key == "COLOR_0" && dataAccessor.Type.Id == "VEC3")
                        {
                            for (var x = 0; x < data.Length; x += 3)
                            {
                                var color = new Color(
                                    data[i * numberOfComponents + 0] / 255f,
                                    data[i * numberOfComponents + 1] / 255f,
                                    data[i * numberOfComponents + 2] / 255f,
                                    1);
                                colors.Add(color);
                            }
                        }
                        else if (attribute.Key == "TEXCOORD_0" && dataAccessor.Type.Id == "VEC2")
                        {
                            uvs.Add(new Vector2(
                                data[i * numberOfComponents + 0],
                                data[i * numberOfComponents + 1]
                            ));
                        }
                        else if (attribute.Key == "JOINTS_0" && dataAccessor.Type.Id == "VEC4")
                        {
                            joints.Add(new Vector4(
                                data[i * numberOfComponents + 0],
                                data[i * numberOfComponents + 1],
                                data[i * numberOfComponents + 2],
                                data[i * numberOfComponents + 3]
                            ));
                        }
                        else if (attribute.Key == "WEIGHTS_0" && dataAccessor.Type.Id == "VEC4")
                        {
                            var vector = new Vector4(
                                data[i * numberOfComponents + 0],
                                data[i * numberOfComponents + 1],
                                data[i * numberOfComponents + 2],
                                data[i * numberOfComponents + 3]
                            );
                            w.Add(vector);
                        }
                    }
                }
            }

            var vertexBufferDummy = new List<VertexPositionColorNormalTextureBlend>();

            // Hier fügen wir die gesammelten Positionen, Normalen und Texturkoordinaten in die _positions-Liste ein
            for (int i = 0; i < position.Count; i++)
            {
                vertexBufferDummy.Add(new VertexPositionColorNormalTextureBlend(
                    position[i],
                    colors.Count > i ? colors[i] : Color.White,
                    normals.Count > i ? normals[i] : Vector3.Up,
                    uvs.Count > i ? uvs[i] : Vector2.Zero,
                    joints.Count > i ? joints[i] : Vector4.Zero,
                    w.Count > i ?  w[i] : Vector4.UnitX)
                );
            }

            // Setzen des VertexBuffers
            var vertexArray = vertexBufferDummy.ToArray();
            //VertexPositionColorNormalTextureBlend.CalculateNormals(vertexArray);
            result.VertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionColorNormalTextureBlend.VertexDeclaration, vertexBufferDummy.Count, BufferUsage.WriteOnly);
            result.VertexBuffer.SetData(vertexArray);

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
