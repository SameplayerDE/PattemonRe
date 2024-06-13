using HxGLTF;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TestRender
{
    public class GameModel
    {
        public GameNode[] Nodes;

        public static GameModel From(GraphicsDevice graphicsDevice, GLTFFile file)
        {
            var scene = file.Scenes[0];
            GameModel result = null;

            if (scene.HasNodes)
            {
                result = new GameModel();
                result.Nodes = new GameNode[scene.Nodes.Length];
                for (int i = 0; i < scene.Nodes.Length; i++)
                {
                    var node = scene.Nodes[i];
                    var gameNode = GameNode.From(graphicsDevice, file, node);
                    result.Nodes[i] = gameNode;
                }
            }

            return result;
        }
    }
}
