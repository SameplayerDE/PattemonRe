using HxGLTF;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TestRender
{
    public class GameModel
    {
        public GameNode Root;

        public static GameModel From(GraphicsDevice graphicsDevice, GLTFFile file)
        {
            var scene = file.Scenes[0];
            GameModel result = null;

            if (scene.HasNodes)
            {
                if (scene.Nodes.Length > 1)
                {
                    throw new Exception();
                }

                var node = scene.Nodes[0];

                if (!node.HasMesh)
                {
                    throw new Exception();
                }
                var rootNode = GameNode.From(graphicsDevice, file, node);

                result = new GameModel();
            }

            return result;
        }
    }
}
