using System;
using HxGLTF;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Transactions;
using Microsoft.Xna.Framework;

namespace TestRender
{
    public class GameModel
    {
        public GameNode[] Nodes;
        public GameScene[] Scenes;
        public GameMesh[] Meshes;
        public GameSkin[] Skins;
        public Dictionary<string, GameModelAnimation> Animations;

        public Vector3 Translation = Vector3.Zero;
        public Vector3 Scale = Vector3.One;
        public Vector3 Rotation = Vector3.Zero;
        
        public bool HasAnimations => Animations != null;
        
        private string _currentAnimationKey;
        private float _animationTimer;
        private bool _isPlaying;
        
        public void RotateX(float x)
        {
            Rotation.X += MathHelper.ToRadians(x);
            Rotation.X = Math.Clamp(Rotation.X, MathHelper.ToRadians(-89.9f), MathHelper.ToRadians(89.9f));
        }

        public void RotateZ(float z)
        {
            Rotation.Z += MathHelper.ToRadians(z);
        }

        public void RotateY(float y)
        {
            Rotation.Y += MathHelper.ToRadians(y);
            if (Rotation.Y < 0) Rotation.Y += MathHelper.ToRadians(360);
        }
        
        public void Play(string animationKey)
        {
            if (animationKey == _currentAnimationKey && _isPlaying)
            {
                return;
            }
            if (HasAnimations && Animations.TryGetValue(animationKey, out var animation))
            {
                _currentAnimationKey = animationKey;
                _animationTimer = 0;
                _isPlaying = true;
            }
            else
            {
                _currentAnimationKey = null;
                _animationTimer = 0;
                _isPlaying = false;
            }
        }
        
        public void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_isPlaying && HasAnimations && _currentAnimationKey != null)
            {
                var animation = Animations[_currentAnimationKey];

                // Update animation timer
                _animationTimer += delta;

                // Update animation based on current timer
                UpdateAnimation(_animationTimer);

                // Check if animation has completed
                if (_animationTimer >= animation.Duration)
                {
                    _isPlaying = false;
                    _currentAnimationKey = null;
                    _animationTimer = 0;
                }
            }
        }

        private void UpdateAnimation(float animationTime)
        {
            var animation = Animations[_currentAnimationKey];

            foreach (var channel in animation.Channels)
            {
                var targetPath = channel.Target.Path;

                var valuesPerElement = targetPath switch
                {
                    "rotation" => 4,
                    "translation" or "scale" => 3,
                    _ => 0
                };

                if (valuesPerElement > 0)
                {
                    var sampler = animation.Samplers[channel.SamplerIndex];
                    float[] timeStamps = sampler.Input;

                    int prevIndex = -1;
                    int nextIndex = -1;

                    // Find the indices for the surrounding keyframes
                    for (int i = 0; i < timeStamps.Length; i++)
                    {
                        if (timeStamps[i] <= animationTime)
                        {
                            prevIndex = i;
                        }

                        if (timeStamps[i] > animationTime)
                        {
                            nextIndex = i;
                            break;
                        }
                    }

                    if (nextIndex == -1)
                    {
                        nextIndex = 0;
                    }

                    if (sampler.InterpolationAlgorithm == InterpolationAlgorithm.Step)
                    {
                        // For step interpolation, just use the previous index
                        if (prevIndex >= 0)
                        {
                            var data = new float[valuesPerElement];
                            for (int i = 0; i < valuesPerElement; i++)
                            {
                                int offset = prevIndex * valuesPerElement;
                                data[i] = sampler.Output[offset + i];
                            }

                            if (targetPath == "rotation")
                            {
                                Quaternion rotation = new Quaternion(data[0], data[1], data[2], data[3]);
                                Nodes[channel.Target.NodeIndex].Rotate(rotation);
                            }
                            else if (targetPath == "translation")
                            {
                                Vector3 translation = new Vector3(data[0], data[1], data[2]);
                                Nodes[channel.Target.NodeIndex].Translate(translation);
                            }
                            else if (targetPath == "scale")
                            {
                                Vector3 scale = new Vector3(data[0], data[1], data[2]);
                                Nodes[channel.Target.NodeIndex].Resize(scale);
                            }
                        }
                    }
                    else if (sampler.InterpolationAlgorithm == InterpolationAlgorithm.Linear)
                    {
                        // For linear interpolation, use both prev and next indices
                        if (prevIndex >= 0 && nextIndex >= 0)
                        {
                            var prevTimeStamp = timeStamps[prevIndex];
                            var nextTimeStamp = timeStamps[nextIndex];
                            float t;

                            // Ensure t is within the range of 0 to 1
                            if (nextTimeStamp > prevTimeStamp)
                            {
                                t = (animationTime - prevTimeStamp) / (nextTimeStamp - prevTimeStamp);
                                t = MathHelper.Clamp(t, 0f, 1f); // Clamp to range from 0 to 1
                            }
                            else
                            {
                                t = 0f; // Case when prevTimeStamp == nextTimeStamp (rare but possible)
                            }

                            var prevData = new float[valuesPerElement];
                            var nextData = new float[valuesPerElement];
                            for (int i = 0; i < valuesPerElement; i++)
                            {
                                int offsetPrev = prevIndex * valuesPerElement;
                                int offsetNext = nextIndex * valuesPerElement;
                                prevData[i] = sampler.Output[offsetPrev + i];
                                nextData[i] = sampler.Output[offsetNext + i];
                            }

                            if (targetPath == "rotation")
                            {
                                Quaternion prevRotation = new Quaternion(prevData[0], prevData[1], prevData[2], prevData[3]);
                                Quaternion nextRotation = new Quaternion(nextData[0], nextData[1], nextData[2], nextData[3]);
                                Quaternion rotation = Quaternion.Slerp(prevRotation, nextRotation, t);
                                Nodes[channel.Target.NodeIndex].Rotate(rotation);
                            }
                            else if (targetPath == "translation")
                            {
                                Vector3 prevTranslation = new Vector3(prevData[0], prevData[1], prevData[2]);
                                Vector3 nextTranslation = new Vector3(nextData[0], nextData[1], nextData[2]);
                                Vector3 translation = Vector3.Lerp(prevTranslation, nextTranslation, t);
                                Nodes[channel.Target.NodeIndex].Translate(translation);
                            }
                            else if (targetPath == "scale")
                            {
                                Vector3 prevScale = new Vector3(prevData[0], prevData[1], prevData[2]);
                                Vector3 nextScale = new Vector3(nextData[0], nextData[1], nextData[2]);
                                Vector3 scale = Vector3.Lerp(prevScale, nextScale, t);
                                Nodes[channel.Target.NodeIndex].Resize(scale);
                            }
                        }
                    }
                }
            }
            
            Console.WriteLine(_animationTimer);
        }
        
        public static GameModel From(GraphicsDevice graphicsDevice, GLTFFile file)
        {
            
            var result = new GameModel();
            
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

            if (file.HasScenes)
            {
                result.Scenes = new GameScene[file.Scenes.Length];
                for (var i = 0; i < file.Scenes.Length; i++)
                {
                    var scene = GameScene.From(graphicsDevice, file, file.Scenes[i]);
                    for (var j = 0; j < scene.Nodes.Length; j++)
                    {
                        result.Nodes[j].UpdateGlobalTransform();
                    }
                    result.Scenes[i] = scene;
                }
            }
            
            if (file.HasSkins)
            {
                result.Skins = new GameSkin[file.Skins.Length];
                for (var i = 0; i < file.Skins.Length; i++)
                {
                    var skin = GameSkin.From(graphicsDevice, file, file.Skins[i]);
                    result.Skins[i] = skin;
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