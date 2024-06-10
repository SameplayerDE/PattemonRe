using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework;

namespace Survival
{
    public class MeshParser
    {
        public static (List<Vector3> vertices, List<int> indices) Parse(string filepath)
        {
            var vertices = new List<Vector3>();
            var indices = new List<int>();

            using (var reader = new StreamReader(filepath))
            {
                string line;
                bool header = true;
                bool readingVertices = false;
                bool readingFaces = false;
                int vertexCount = 0;
                int faceCount = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    if (header)
                    {
                        if (line.StartsWith("element vertex"))
                        {
                            vertexCount = int.Parse(line.Split(' ')[2]);
                        }
                        else if (line.StartsWith("element face"))
                        {
                            faceCount = int.Parse(line.Split(' ')[2]);
                        }
                        else if (line.StartsWith("end_header"))
                        {
                            header = false;
                            readingVertices = true;
                        }
                    }
                    else if (readingVertices)
                    {
                        if (vertexCount > 0)
                        {
                            var parts = line.Split(' ');
                            var vertex = new Vector3(
                                float.Parse(parts[0], CultureInfo.InvariantCulture),
                                float.Parse(parts[1], CultureInfo.InvariantCulture),
                                float.Parse(parts[2], CultureInfo.InvariantCulture)
                            );
                            vertices.Add(vertex);
                            vertexCount--;
                        }
                        if (vertexCount == 0)
                        {
                            readingVertices = false;
                            readingFaces = true;
                        }
                    }
                    else if (readingFaces)
                    {
                        if (faceCount > 0)
                        {
                            var parts = line.Split(' ');
                            int vertexPerFace = int.Parse(parts[0]);

                            if (vertexPerFace == 3)
                            {
                                indices.Add(int.Parse(parts[1]));
                                indices.Add(int.Parse(parts[2]));
                                indices.Add(int.Parse(parts[3]));
                            }
                            faceCount--;
                        }
                    }
                }
            }

            return (vertices, indices);
        }
    }
}
