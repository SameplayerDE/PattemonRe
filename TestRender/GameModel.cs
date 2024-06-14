using System;
using HxGLTF;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Transactions;

namespace TestRender
{
    public class GameModel
    {
        public GameNode[] Nodes;
        public GameScene[] Scenes;
        public GameMesh[] Meshes;
        public Dictionary<string, GameModelAnimation> Animations;

        public bool HasAnimations => Animations != null;
        
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
            
            var result = new GameModel();

            if (file.HasScenes)
            {
                result.Scenes = new GameScene[file.Scenes.Length];
                for (var i = 0; i < file.Scenes.Length; i++)
                {
                    var scene = GameScene.From(graphicsDevice, file, file.Scenes[i]);
                    result.Scenes[i] = scene;
                }
            }
            
            if (file.HasNodes)
            {
                result.Nodes = new GameNode[file.Nodes.Length];
                for (var i = 0; i < file.Nodes.Length; i++)
                {
                    var node = GameNode.From(graphicsDevice, file, file.Nodes[i]);
                    node.Model = result;
                    result.Nodes[i] = node;
                }

                for (var i = 0; i < result.Nodes.Length; i++)
                {
                    var node = result.Nodes[i];
                    if (node.HasChildren)
                    {
                        for (var j = 0; j < node.Children.Length; j++)
                        {
                            result.Nodes[node.Children[j]].Parent = node;
                        }
                    }
                }
            }

            if (file.HasMeshes)
            {
                result.Meshes = new GameMesh[file.Meshes.Length];
                for (var i = 0; i < file.Meshes.Length; i++)
                {
                    var mesh = GameMesh.From(graphicsDevice, file, file.Meshes[i]);
                    result.Meshes[i] = mesh;
                }
            }
            
            if (file.HasAnimations)
            {
                result.Animations = new Dictionary<string, GameModelAnimation>();
                for (int i = 0; i < file.Animations.Length; i++)
                {
                    var animation = file.Animations[i];
                    result.Animations[Guid.NewGuid().ToString()] =
                        GameModelAnimation.From(graphicsDevice, file, animation);
                }
            }

            //if (scene.HasNodes)
            //{
            //    result = new GameModel();
            //    result.Nodes = new GameNode[scene.Nodes.Length];
            //    for (int i = 0; i < scene.Nodes.Length; i++)
            //    {
            //        var node = scene.Nodes[i];
            //        var gameNode = GameNode.From(graphicsDevice, file, node);
            //        result.Nodes[i] = gameNode;
            //    }
//
            //    if (file.Animations != null)
            //    {
            //        result.Animations = new Dictionary<string, GameModelAnimation>();
            //        for (int i = 0; i < file.Animations.Length; i++)
            //        {
            //            var animation = file.Animations[i];
            //            result.Animations[Guid.NewGuid().ToString()] =
            //                GameModelAnimation.From(graphicsDevice, file, animation);
            //        }
            //    }
//
            //}

            return result;
        }
    }
}
