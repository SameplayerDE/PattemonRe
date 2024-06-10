using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Survival
{
    public class Mesh
    {
        public VertexPositionTexture[] Vertices;
        public int[] Indices;

        public Mesh(List<Vector3> vertices, List<int> indices)
        {
            Vertices = new VertexPositionTexture[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                Vertices[i] = new VertexPositionTexture(vertices[i], Vector2.Zero);
            }

            Indices = indices.ToArray();
        }

    }
}
