using HxGLTF;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Survival.Rendering;

namespace TestRender
{
    public class GameMeshPrimitives
    {
        public VertexBuffer VertexBuffer;
        public int VertexCount;
        public IndexBuffer IndexBuffer;
        public int IndexCount;
        public bool IsIndexed;
        public GameMaterial Material;

       public static GameMeshPrimitives From(GraphicsDevice graphicsDevice, GLTFFile file, MeshPrimitive primitive)
        {
            GameMeshPrimitives result = new GameMeshPrimitives();

            result.Material = GameMaterial.From(graphicsDevice, file, primitive.Material);

            var position = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();

            
            foreach (var attribute in primitive.Attributes)
            {
                Console.WriteLine(attribute.Key);
                Console.WriteLine(attribute.Value.Type.Id);

                var dataAccessor = attribute.Value;
                var elementCount = dataAccessor.Count;
                var numberOfComponents = dataAccessor.Type.NumberOfComponents;

                float[] data;

                if (primitive.HasIndices)
                {
                    data = AccessorReader.ReadDataIndexed(dataAccessor, primitive.Indices);

                    Console.WriteLine(attribute.Key);

                    if (attribute.Key == "POSITION" && dataAccessor.Type.Id == "VEC3")
                    {
                        for (var x = 0; x < data.Length; x += 3)
                        {
                            position.Add(new Vector3(data[x], data[x + 1], data[x + 2]));
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

                    if (attribute.Key == "COLOR_0" && dataAccessor.Type.Id == "VEC3")
                    {
                        
                    }
                }
                else
                {
                    data = AccessorReader.ReadData(dataAccessor);

                    Console.WriteLine(attribute.Key);

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
                        else if (attribute.Key == "COLOR_0" && dataAccessor.Type.Id == "VEC3")
                        {
                            
                        }
                        else if (attribute.Key == "TEXCOORD_0" && dataAccessor.Type.Id == "VEC2")
                        {
                            uvs.Add(new Vector2(
                                data[i * numberOfComponents + 0],
                                data[i * numberOfComponents + 1]
                            ));
                        }
                    }
                }
            }

            var vertexBufferDummy = new List<VertexPositionNormalColorTexture>();

            // Hier fügen wir die gesammelten Positionen, Normalen und Texturkoordinaten in die _positions-Liste ein
            for (int i = 0; i < position.Count; i++)
            {
                vertexBufferDummy.Add(new VertexPositionNormalColorTexture(
                    position[i],
                    Color.White,
                    normals.Count > i ? normals[i] : Vector3.Up,
                    uvs.Count > i ? uvs[i] : Vector2.Zero
                ));
            }

            // Setzen des VertexBuffers
            var vertexArray = vertexBufferDummy.ToArray();
            VertexPositionNormalColorTexture.CalculateNormals(vertexArray);
            result.VertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionNormalColorTexture.VertexDeclaration, vertexBufferDummy.Count, BufferUsage.WriteOnly);
            result.VertexBuffer.SetData(vertexArray);

            return result;
        }
    }

    public class GameMesh
    {
        public GameMeshPrimitives[] Primitives;

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
