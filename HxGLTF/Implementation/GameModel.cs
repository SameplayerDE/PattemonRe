using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HxGLTF.Implementation
{
    public class GameModel
    {
        public GameNode[] Nodes;
        public GameScene[] Scenes;
        public GameMesh[] Meshes;
        public GameSkin[] Skins;
        public GameModelAnimation[] Animations;

        public Matrix GlobalTranformation;
        public Matrix LocalTranformation;
        
        public Vector3 Translation = Vector3.Zero;
        public Vector3 Scale = Vector3.One;
        public Quaternion Rotation = Quaternion.Identity;
        
        private int _currentAnimationIndex;
        private int _nextAnimationIndex;

        private float _animationTimer;
        public bool IsPlaying;
        
        //AnimationPlayerClass
        
        public void Play(int index)
        {
            _currentAnimationIndex = index;
            IsPlaying = true;
            // set what animation should be played
            // check if currenlty animaton is running
            // if currently not then just play the animation 
            // if yes then try to interpolate currently running animation with next animation so that there is not visual cut
        }
        
        public void Stop()
        {
            _currentAnimationIndex = -1;
            IsPlaying = false;
            // set what animation should be played
            // check if currenlty animaton is running
            // if currently not then just play the animation 
            // if yes then try to interpolate currently running animation with next animation so that there is not visual cut
        }
        
        public void Update(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Animations != null)
            {
                if (IsPlaying)
                {
                    GameModelAnimation currentAnimation;
                    if (Animations.Length <= _currentAnimationIndex)
                    {
                        throw new Exception();
                    }

                    currentAnimation = Animations[_currentAnimationIndex];
                    UpdateAnimation(currentAnimation, _animationTimer);
                    _animationTimer += delta;
                    if (_animationTimer > currentAnimation.Duration)
                    {
                        _animationTimer = 0;
                    }
                }
            }

            if (Skins != null)
            {
                if (Skins.Length > 0)
                {
                    foreach (var skin in Skins)
                    {
                        skin.Update(gameTime);
                    }
                }
            }
        }

        private void UpdateAnimation(GameModelAnimation animation, float animationTime)
        {
            
            //GameModelAnimation nextAnimation;
            //if (Animations.Length <= _nextAnimationIndex)
            //{
            //    throw new Exception();
            //}
            //nextAnimation = Animations[_nextAnimationIndex];
            
            foreach (var channel in animation.Channels)
            {
                var targetPath = channel.Target.Path;

                var valuesPerElement = targetPath switch
                {
                    "rotation" => 4,
                    "translation" or "scale" => 3,
                    "texture" => 2,
                    _ => 0
                };

                if (valuesPerElement <= 0)
                {
                    continue;
                }
                var sampler = animation.Samplers[channel.SamplerIndex];
                var timeStamps = sampler.Input;

                var prevIndex = -1;
                var nextIndex = -1;

                // Find the indices for the surrounding keyframes
                for (var i = 0; i < timeStamps.Length; i++)
                {
                    if (timeStamps[i] <= animationTime)
                    {
                        prevIndex = i;
                    }

                    if (!(timeStamps[i] > animationTime))
                    {
                        continue;
                    }
                    nextIndex = i;
                    break;
                }

                if (nextIndex == -1)
                {
                    nextIndex = 0;
                }

                switch (sampler.InterpolationAlgorithm)
                {
                    case InterpolationAlgorithm.Step:
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
                            else if (targetPath == "texture")
                            {
                                Vector2 uv = new Vector2(data[0], data[1]);
                                var node = Nodes[channel.Target.NodeIndex];
                                if (node.HasMesh)
                                {
                                    var mesh = node.Mesh;
                                    //mesh.Primitives.
                                }

                            }
                        }

                        break;
                    }
                    case InterpolationAlgorithm.Linear:
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
                                Quaternion rotation = Quaternion.Lerp(prevRotation, nextRotation, t);
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

                        break;
                    }
                    case InterpolationAlgorithm.Cubicspline:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
        }

        public void Translate(Vector3 translation)
        {
            Translation += translation;
            UpdateTransform();
        }

        public void RotateBy(Quaternion rotation)
        {
            Rotation *= rotation;
            UpdateTransform();
        }
        
        public void RotateTo(Quaternion rotation)
        {
            Rotation = rotation;
            UpdateTransform();
        }

        private void UpdateTransform()
        {
            LocalTranformation = Matrix.CreateScale(Scale) *
                                  Matrix.CreateFromQuaternion(Rotation) *
                                  Matrix.CreateTranslation(Translation);

            foreach (var node in Nodes)
            {
                //node.UpdateGlobalTransform();
            }
        }

        public void Draw(GraphicsDevice graphicsDevice, Effect effect, Matrix world, Matrix view, Matrix projection)
        {
            
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
                result.Animations = new GameModelAnimation[file.Animations.Length];
                for (int i = 0; i < file.Animations.Length; i++)
                {
                    var animation = file.Animations[i];
                    result.Animations[i] = GameModelAnimation.From(graphicsDevice, file, animation);
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
                    skin.Model = result;
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