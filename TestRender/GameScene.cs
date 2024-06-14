using HxGLTF;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TestRender
{
    public class GameScene
    {
        public int[] Nodes;

        public static GameScene From(GraphicsDevice graphicsDevice, GLTFFile file, Scene scene)
        {
            var result = new GameScene();

            if (scene.HasNodes)
            {
                result.Nodes = scene.NodesIndices;
            }

            return result;
        }

    }
}