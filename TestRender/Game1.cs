using System;
using System.Collections.Generic;
using HxGLTF;
using HxGLTF.Implementation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TestRendering;

namespace TestRender
{
    public class Game1 : Game
    {
        private KeyboardState _prev;
        private KeyboardState _curr;
        
        private GraphicsDeviceManager _graphicsDeviceManager;
        private SpriteBatch _spriteBatch;
        
        private Effect _effect;

        private Camera _camera;
        
        private Texture2D _testTexture2D;

        private float AnimationTimer = 0.00f;

        private List<GameModel> gameModels = [];
        private GameModel hero;

        private SpriteFont _font;
        
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
            //gltfFile = GLTFLoader.Load(@"A:\ModelExporter\black2\output_assets\m_dun3501_00_00\m_dun3501_00_00.glb");
            //gameModels.Add(GameModel.From(GraphicsDevice, gltfFile));
            ////gltfFile = GLTFLoader.Load(@"A:\ModelExporter\Platin\output_assets\hero\hero");
            ////hero = GameModel.From(GraphicsDevice, gltfFile);
            //gltfFile = GLTFLoader.Load(@"A:\ModelExporter\black2\output_assets\m_dun3501_01_01\m_dun3501_01_01.glb");
            //gameModels.Add(GameModel.From(GraphicsDevice, gltfFile));
            //////gltfFile = GLTFLoader.Load(@"Content\pkemon_oben");
            //////gltfFile = GLTFLoader.Load(@"Content\Cube.glb");
            //gltfFile = GLTFLoader.Load(@"A:\ModelExporter\black2\output_assets\m_dun3501_00_01\m_dun3501_00_01.glb");
            //gameModels.Add(GameModel.From(GraphicsDevice, gltfFile));
            ////
            //////gltfFile = GLTFLoader.Load(@"Content\Simple");
            //gltfFile = GLTFLoader.Load(@"A:\ModelExporter\black2\output_assets\m_dun3501_01_00\m_dun3501_01_00.glb");
            //////gltfFile = GLTFLoader.Load(@"Content\helm");
            ////gltfFile = GLTFLoader.Load(@"Content\map01_22c\map01_22c.glb");
            //gameModels.Add(GameModel.From(GraphicsDevice, gltfFile));
            ////
            ////gltfFile = GLTFLoader.Load(@"Content\Fox.gltf");
            ////gltfFile = GLTFLoader.Load(@"A:\ModelExporter\black2\output_assets\badgegate_02\badgegate_02.glb");
            ////gltfFile = GLTFLoader.Load(@"A:\ModelExporter\black2\output_assets\ak_w02\ak_w02");
            ////gltfFile = GLTFLoader.Load(@"Content\aura");
            
            //gltfFile = GLTFLoader.Load(@"A:\ModelExporter\Platin\output_assets\psel_mb_a\psel_mb_a");
            //gameModels.Add(GameModel.From(GraphicsDevice, gltfFile));
            //
            //gltfFile = GLTFLoader.Load(@"A:\ModelExporter\Platin\output_assets\psel_mb_b\psel_mb_b");
            //gameModels.Add(GameModel.From(GraphicsDevice, gltfFile));
            //
            //gltfFile = GLTFLoader.Load(@"A:\ModelExporter\Platin\output_assets\psel_mb_c\psel_mb_c");
            //gameModels.Add(GameModel.From(GraphicsDevice, gltfFile));
            
            gltfFile = GLTFLoader.Load(@"Content\psel");
            gameModels.Add(GameModel.From(GraphicsDevice, gltfFile));
            
            //gltfFile = GLTFLoader.Load(@"Content\hilda_regular_00");
            ////gltfFile = GLTFLoader.Load(@"Content\jeny_tpose_riged");
            ////gltfFile = GLTFLoader.Load(@"Content\Simple");
            //gameModels.Add(GameModel.From(GraphicsDevice, gltfFile));
            //
            //Console.WriteLine(gltfFile.Asset.Version);

           //gameModels[3].Translation = new Vector3(1, 0, 1) * 256;
           //gameModels[2].Translation = new Vector3(-1, 0, 3) * 256;
           //gameModels[0].Translation = new Vector3(-1, 0, 1) * 256;
           //gameModels[1].Translation = new Vector3(1, 0, 3) * 256;
           ////
           ////gameModels[4].Translation = new Vector3(-140, 2.5f, 380);
           ////gameModels[4].Scale = Vector3.One * 20;
           ////gameModels[4].RotateX(90);
           ////
           //gameModels[5].Translation = new Vector3(-160, 2.5f, 350);
           //gameModels[5].Scale = Vector3.One * 30;
//
           //hero = gameModels[4];
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // ReSharper disable once HeapView.ObjectAllocation.Evident
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _effect = Content.Load<Effect>("PBRShader");
            _effect.Parameters["Bones"].SetValue(new Matrix[128]); 
            _testTexture2D = Content.Load<Texture2D>("1");
            _font = Content.Load<SpriteFont>("Font");
            
            _camera.Move(0, 1, -3);
        }

        protected override void Update(GameTime gameTime)
        {

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            if (!IsActive)
            {
                return;
            }

            _prev = _curr;
            _curr = Keyboard.GetState();
            
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
            
            if (Keyboard.GetState().IsKeyDown(Keys.O))
            {
                _camera.EnableMix = true;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                _camera.EnableMix = false;
            }

            if (Keyboard.GetState().IsKeyUp(Keys.LeftShift))
            {

                Direction *= 100;

                _camera.Move(-Direction * delta);

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

            }
            else
            {
                
                hero.Translation += Direction * 16 * delta;
            }

            //hero.Rotation = _camera.Rotation;
            _camera.Update(gameTime);

            if (_prev.IsKeyUp(Keys.Space) && _curr.IsKeyDown(Keys.Space))
            {
                foreach (var model in gameModels)
                {
                    if (!model.HasAnimations)
                    {
                        continue;
                    }

                    int i = 0;
                    foreach (var animation in model.Animations)
                    {
                        if (i++ > 1)
                        {

                        }

                        model.Play(animation.Key);
                    }

                }
            }

            foreach (var model in gameModels)
            {
                model.Update(gameTime);
            }
            
            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            
            foreach (var model in gameModels)
            {
                DrawModel(model);
            }

            
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
            
            //skinnig here?
            
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
            
            var worldMatrix = node.GlobalTransform *
                                 Matrix.CreateScale(model.Scale) *
                                 Matrix.CreateRotationX(model.Rotation.X) *
                                 Matrix.CreateRotationY(model.Rotation.Y) *
                                 Matrix.CreateTranslation(model.Translation);
            
            _effect.Parameters["World"].SetValue(node.GlobalTransform * worldMatrix);
            _effect.Parameters["View"].SetValue(_camera.View);
            _effect.Parameters["Projection"].SetValue(_camera.Projection);
            
            if (model.Skins is { Length: > 0 } && model.HasAnimations)
            {
                var skin = model.Skins[0];
                Matrix[] jointMatrices = new Matrix[skin.Joints.Length];

                for (int i = 0; i < skin.Joints.Length; i++)
                {
                    int nodeIndex = skin.Joints[i];
                    GameNode tnode = model.Nodes[nodeIndex];
                    tnode.UpdateGlobalTransform();

                    // Get the global transform of the joint node
                    Matrix globalTransform = tnode.GlobalTransform;

                    // Get the inverse bind matrix for the joint from the skin
                    Matrix inverseBindMatrix = skin.InverseBindMatrices[i];

                    // Compute the joint matrix
                    Matrix jointMatrix = inverseBindMatrix * globalTransform; // Reihenfolge der Matrixmultiplikation beachten

                    // Store the joint matrix in the array
                    jointMatrices[i] = jointMatrix;
                }

                // Set the joint matrices array in the shader effect
                _effect.Parameters["Bones"].SetValue(jointMatrices); // Setze die korrekten Joint-Matrizen
                _effect.Parameters["SkinningEnabled"]?.SetValue(true);
            }
            else
            {
                _effect.Parameters["SkinningEnabled"]?.SetValue(false);
            }
            
            //if (model.Skins is { Length: > 0 })
            //{
            //    var skin = model.Skins[0];
            //    _effect.Parameters["Bones"].SetValue(skin.InverseBindMatrices);
            //    _effect.Parameters["SkinningEnabled"]?.SetValue(true);
            //}
            
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