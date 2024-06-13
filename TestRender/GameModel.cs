using System;
using HxGLTF;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TestRender
{
    public class GameModel
    {
        public GameNode[] Nodes;
        public Dictionary<string, GameModelAnimation> Animations;
        
        public void Play(string animationKey)
        {
            if (Animations == null)
            {
                return;
            }

            if (!Animations.TryGetValue(animationKey, out var animation))
            {
                return;
            }
            
            // do something
            
        }
        
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

                result.Animations = new Dictionary<string, GameModelAnimation>();
                for (int i = 0; i < file.Animations.Length; i++)
                {
                    var animation = file.Animations[i];
                    result.Animations[Guid.NewGuid().ToString()] = GameModelAnimation.From(graphicsDevice, file, animation);
                }
                
            }

            return result;
        }
    }
}
