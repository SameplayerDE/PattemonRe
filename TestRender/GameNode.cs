using HxGLTF;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TestRender
{
    public class GameNode
    {
        public GameNode[] Children;
        public bool HasChildren;
        public GameMesh Mesh;
        public Matrix Matrix;
        public Vector3 Translation;
        public Vector3 Scale;
        public Quaternion Rotation;

        public static GameNode From(GraphicsDevice graphicsDevice, GLTFFile file, Node node)
        {
            var result = new GameNode();

            result.Translation = node.Translation;
            result.Rotation = node.Rotation;
            result.Scale = node.Scale;
            result.Matrix = node.Matrix;

            if (node.HasMesh)
            {
                result.Mesh = GameMesh.From(graphicsDevice, file, node.Mesh);
            }

            if (node.HasChildren)
            {
                result.HasChildren = true;
                result.Children = new GameNode[node.Children.Length];
                for (int i = 0; i < node.Children.Length; i++)
                {
                    var child = node.Children[i];
                    result.Children[i] = From(graphicsDevice, file, child);
                }
            }

            return result;
        }

    }
}
