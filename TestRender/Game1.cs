using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HxGLTF;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Survival.Rendering;
using TestRender;

namespace TestRendering
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphicsDeviceManager;
        private SpriteBatch _spriteBatch;
        
        private Effect _effect;

        private Camera _camera;
        
        private Texture2D _testTexture2D;

        private float AnimationTimer = 0.00f;

        private GameModel gameModel;
        private GameModel gameModel2;
        private GameModel gameModel3;
        private GameModel gameModel4;

        public readonly static BlendState AlphaSubtractive = new BlendState
        {
            ColorSourceBlend = Blend.One,
            AlphaSourceBlend = Blend.One,			    
            ColorDestinationBlend = Blend.Zero,
            AlphaDestinationBlend = Blend.Zero
        };
        
        public Game1()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);

            _graphicsDeviceManager.PreferredBackBufferHeight = 720;
            _graphicsDeviceManager.PreferredBackBufferWidth = 1280;
            
            _graphicsDeviceManager.ApplyChanges();
        }

        protected override void Initialize()
        {
            
            _camera = new Camera(GraphicsDevice);

            GLTFFile gltfFile;
            gltfFile = GLTFLoader.Load(@"A:\ModelExporter\black2\output_assets\m_dun3501_00_00\m_dun3501_00_00.glb");
            gameModel3 = GameModel.From(GraphicsDevice, gltfFile);
            gltfFile = GLTFLoader.Load(@"A:\ModelExporter\black2\output_assets\m_dun3501_01_01\m_dun3501_01_01.glb");
            gameModel4 = GameModel.From(GraphicsDevice, gltfFile);
            //gltfFile = GLTFLoader.Load(@"Content\pkemon_oben");
            //gltfFile = GLTFLoader.Load(@"Content\Cube.glb");
            gltfFile = GLTFLoader.Load(@"A:\ModelExporter\black2\output_assets\m_dun3501_00_01\m_dun3501_00_01.glb");
            gameModel2 = GameModel.From(GraphicsDevice, gltfFile);
            //gltfFile = GLTFLoader.Load(@"A:\ModelExporter\black2\output_assets\badgegate_02\badgegate_02.glb");
            //gltfFile = GLTFLoader.Load(@"Content\Simple");
            gltfFile = GLTFLoader.Load(@"A:\ModelExporter\black2\output_assets\m_dun3501_01_00\m_dun3501_01_00.glb");
            //gltfFile = GLTFLoader.Load(@"Content\helm");
            //gltfFile = GLTFLoader.Load(@"Content\map01_22c\map01_22c.glb");
            gameModel = GameModel.From(GraphicsDevice, gltfFile);
            Console.WriteLine(gltfFile.Asset.Version);

            gameModel.Translation = new Vector3(1, 0, 1) * 256;
            gameModel2.Translation = new Vector3(-1, 0, 3) * 256;
            gameModel3.Translation = new Vector3(-1, 0, 1) * 256;
            gameModel4.Translation = new Vector3(1, 0, 3) * 256;
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // ReSharper disable once HeapView.ObjectAllocation.Evident
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _effect = Content.Load<Effect>("PBRShader");
            _testTexture2D = Content.Load<Texture2D>("1");
        }

        protected override void Update(GameTime gameTime)
        {
            if (!IsActive)
            {
                return;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.O))
            {
                _camera.EnableMix = true;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                _camera.EnableMix = false;
            }

            Vector3 Direction = new Vector3();

            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                Direction += Vector3.Forward;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                Direction += Vector3.Left;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                Direction += Vector3.Right;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                Direction += Vector3.Backward;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                Direction -= Vector3.Down;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.E))
            {
                Direction -= Vector3.Up;
            }

            Direction *= 100;
            
            _camera.Move(-Direction * (float)gameTime.ElapsedGameTime.TotalSeconds);

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                _camera.RotateX(-1);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                _camera.RotateX(1);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                _camera.RotateY(1);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                _camera.RotateY(-1);
            }

            _camera.Update(gameTime);

            // animation


            AnimationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds / 2;


            if (gameModel.HasAnimations)
            {
                var animation = gameModel.Animations.First().Value;
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
                            if (timeStamps[i] <= AnimationTimer)
                            {
                                prevIndex = i;
                            }

                            if (timeStamps[i] > AnimationTimer)
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
                                    gameModel.Nodes[channel.Target.NodeIndex].Rotate(rotation);
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

                                // Sicherstellen, dass t im Bereich von 0 bis 1 liegt
                                if (nextTimeStamp > prevTimeStamp)
                                {
                                    t = (AnimationTimer - prevTimeStamp) / (nextTimeStamp - prevTimeStamp);
                                    t = MathHelper.Clamp(t, 0f, 1f); // Clamp auf den Bereich von 0 bis 1
                                }
                                else
                                {
                                    t = 0f; // Fall, wenn prevTimeStamp == nextTimeStamp (selten, aber möglich)
                                }

                                Console.WriteLine("Time inter: " + t);

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
                                    // Use Slerp for smoother quaternion interpolation
                                    Quaternion prevRotation = new Quaternion(prevData[0], prevData[1], prevData[2], prevData[3]);
                                    Quaternion nextRotation = new Quaternion(nextData[0], nextData[1], nextData[2], nextData[3]);
                                    Quaternion rotation = Quaternion.Slerp(prevRotation, nextRotation, t);
                                    // Apply the rotation
                                    gameModel.Nodes[channel.Target.NodeIndex].Rotate(rotation);

                                }
                                else if (targetPath == "translation")
                                {
                                    
                                    // Use Slerp for smoother quaternion interpolation
                                    Vector3 prev = new Vector3(prevData[0], prevData[1], prevData[2]);
                                    Vector3 next = new Vector3(nextData[0], nextData[1], nextData[2]);
                                    Vector3 translation = Vector3.Lerp(prev, next, t);
                                    gameModel.Nodes[channel.Target.NodeIndex].Translate(translation);
                                    
                                }
                                else if (targetPath == "scale")
                                {
                                    // Use Slerp for smoother quaternion interpolation
                                    Vector3 prev = new Vector3(prevData[0], prevData[1], prevData[2]);
                                    Vector3 next = new Vector3(nextData[0], nextData[1], nextData[2]);
                                    Vector3 scale = Vector3.Lerp(prev, next, t);
                                    gameModel.Nodes[channel.Target.NodeIndex].Resize(scale);
                                }
                            }
                        }
                    }

                    if (AnimationTimer > animation.Duration)
                    {
                        AnimationTimer = 0;
                    }
                    
                }

                base.Update(gameTime);
            }
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            
            
            DrawModel(gameModel);
            DrawModel(gameModel2);
            DrawModel(gameModel3);
            DrawModel(gameModel4);
            
            base.Draw(gameTime);
        }

        private void DrawModel(GameModel model)
        {
            foreach (var scene in model.Scenes)
            {
                foreach (var nodeIndex in scene.Nodes)
                {
                    var node = model.Nodes[nodeIndex];
                    DrawNode(model, node);
                }
            }
        }

        private void DrawNode(GameModel model, GameNode node)
        {
            if (node.HasMesh)
            {
                //DrawMesh(node, gameModel.Meshes[node.MeshIndex]);
                DrawMesh(model, node, node.Mesh);
            }
            
            if (node.HasChildren)
            {
                foreach (var child in node.Children)
                {
                    DrawNode(model, node.Model.Nodes[child]);
                }
            }
        }

        private void DrawMesh(GameModel model, GameNode node, GameMesh mesh)
        {
            int alphaMode = 0;
            
            _effect.Parameters["World"].SetValue(node.GlobalTransform * Matrix.CreateTranslation(model.Translation));
            _effect.Parameters["View"].SetValue(_camera.View);
            _effect.Parameters["Projection"].SetValue(_camera.Projection);
            
            foreach (var primitive in mesh.Primitives)
            {
                GraphicsDevice.SetVertexBuffer(primitive.VertexBuffer);
                
                _effect.Parameters["TextureEnabled"]?.SetValue(false);
                _effect.Parameters["NormalMapEnabled"]?.SetValue(false);
                _effect.Parameters["OcclusionMapEnabled"]?.SetValue(false);
                _effect.Parameters["EmissiveTextureEnabled"]?.SetValue(false);
                
                if (primitive.Material != null)
                {
                    var material = primitive.Material;
                    
                    //_effect.Parameters["EmissiveColorFactor"].SetValue(material.EmissiveFactor.ToVector4());
                    _effect.Parameters["BaseColorFactor"].SetValue(material.BaseColorFactor.ToVector4());
                    _effect.Parameters["AlphaCutoff"].SetValue(material.AlphaCutoff);

                    switch (material.AlphaMode)
                    {
                        case "OPAQUE":
                            alphaMode = 0;
                            break;
                        case "MASK":
                            alphaMode = 1;
                            break;
                        case "BLEND":
                            continue;
                            break;
                    }
                    
                    _effect.Parameters["AlphaMode"].SetValue(alphaMode);
                    
                    if (material.HasTexture)
                    {
                        _effect.Parameters["TextureEnabled"].SetValue(true);
                        _effect.Parameters["Texture"].SetValue(primitive.Material.BaseTexture.Texture);
                        
                        GraphicsDevice.SamplerStates[0] = primitive.Material.BaseTexture.Sampler.SamplerState;
                    }
                    
                    //if (material.HasEmissiveTexture)
                    //{
                    //    _effect.Parameters["EmissiveTextureEnabled"].SetValue(true);
                    //    _effect.Parameters["EmissiveTexture"].SetValue(primitive.Material.EmissiveTexture.Texture);
                    //    
                    //    GraphicsDevice.SamplerStates[3] = primitive.Material.EmissiveTexture.Sampler.SamplerState;
                    //}
                }
                
                foreach (var pass in _effect.Techniques[Math.Max(alphaMode - 1, 0)].Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, primitive.VertexBuffer.VertexCount / 3);
                }
            }
            
            foreach (var primitive in mesh.Primitives)
            {
                GraphicsDevice.SetVertexBuffer(primitive.VertexBuffer);
                
                _effect.Parameters["TextureEnabled"]?.SetValue(false);
                _effect.Parameters["NormalMapEnabled"]?.SetValue(false);
                _effect.Parameters["OcclusionMapEnabled"]?.SetValue(false);
                _effect.Parameters["EmissiveTextureEnabled"]?.SetValue(false);
                
                if (primitive.Material != null)
                {
                    var material = primitive.Material;
                    
                    //_effect.Parameters["EmissiveColorFactor"].SetValue(material.EmissiveFactor.ToVector4());
                    _effect.Parameters["BaseColorFactor"].SetValue(material.BaseColorFactor.ToVector4());
                    _effect.Parameters["AlphaCutoff"].SetValue(material.AlphaCutoff);

                    switch (material.AlphaMode)
                    {
                        case "OPAQUE":
                            continue;
                        case "MASK":
                            continue;
                        case "BLEND":
                            alphaMode = 2;
                            break;
                    }
                    
                    _effect.Parameters["AlphaMode"].SetValue(alphaMode);
                    
                    if (material.HasTexture)
                    {
                        _effect.Parameters["TextureEnabled"].SetValue(true);
                        _effect.Parameters["Texture"].SetValue(primitive.Material.BaseTexture.Texture);
                        
                        GraphicsDevice.SamplerStates[0] = primitive.Material.BaseTexture.Sampler.SamplerState;
                    }
                    
                    //if (material.HasEmissiveTexture)
                    //{
                    //    _effect.Parameters["EmissiveTextureEnabled"].SetValue(true);
                    //    _effect.Parameters["EmissiveTexture"].SetValue(primitive.Material.EmissiveTexture.Texture);
                    //    
                    //    GraphicsDevice.SamplerStates[3] = primitive.Material.EmissiveTexture.Sampler.SamplerState;
                    //}
                }
                
                foreach (var pass in _effect.Techniques[Math.Max(alphaMode - 1, 0)].Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, primitive.VertexBuffer.VertexCount / 3);
                }
            }
        }
    }
}