using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        private SpriteFont _font;
        private RenderTarget2D _screen;

        private List<Chunk> _chunks = [];
        private List<GameModel> _mapObjects = [];
        
        private int _chunkX = 0;
        private int _chunkY = 0;

        private bool ShowArea = false;
        private Texture2D AreaTexture;
        
        public event Action<int, int, int, int> OnChunkChanged;
        
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
            _screen = new RenderTarget2D(GraphicsDevice, 256 / 1, 192 / 1, false,
                GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            _camera = new Camera(GraphicsDevice);

            _chunks = Chunk.Load(GraphicsDevice);
            
            OnChunkChanged += HandleChunkChange;
            
            base.Initialize();
        }

        private void HandleChunkChange(int oldChunkX, int oldChunkY, int newChunkX, int newChunkY)
        {
            // Erstelle den Schlüssel für den neuen Chunk
            var prevChunk = _chunks.FirstOrDefault(chunk => chunk.X == oldChunkX && chunk.Y == oldChunkY);
            var currChunk = _chunks.FirstOrDefault(chunk => chunk.X == newChunkX && chunk.Y == newChunkY);

            if (currChunk != null && currChunk.Header != null)
            {
                // Prüfe, ob der vorherige Chunk null ist oder ob die Header unterschiedlich sind
                if (prevChunk == null || prevChunk.Header == null || prevChunk.Header.Id != currChunk.Header.Id)
                {
                    if (currChunk.Header.ShowNameTag)
                    {
                        Console.WriteLine($"{currChunk.Header.LocationName}");
                        ShowArea = true;
                    }
                }
            }
        }

        
        protected override void LoadContent()
        {
            // ReSharper disable once HeapView.ObjectAllocation.Evident
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            AreaTexture = Content.Load<Texture2D>("00");
            _effect = Content.Load<Effect>("PBRShader");
            _effect.Parameters["Bones"].SetValue(new Matrix[64]); 
            _font = Content.Load<SpriteFont>("Font");
            _camera.Teleport(new Vector3(3, 0, 27) * 512);
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
            
            var direction = new Vector3();

            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                direction += Vector3.Forward;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                direction += Vector3.Left;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                direction += Vector3.Right;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                direction += Vector3.Backward;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                direction -= Vector3.Down;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.E))
            {
                direction -= Vector3.Up;
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

                direction *= 128;
                _camera.Move(-direction * delta);

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
            
            _camera.Update(gameTime);

            // Die Größe eines Chunks
            int chunkSize = 512;

            // Position der Kamera
            float cameraPosX = _camera.Position.X;
            float cameraPosZ = _camera.Position.Z;

            // Berechne die Chunk-Koordinaten, verschoben um die Hälfte der Chunk-Größe
            int newChunkX = (int)((cameraPosX + chunkSize / 2) / chunkSize);
            int newChunkY = (int)((cameraPosZ + chunkSize / 2) / chunkSize);

            // Prüfe auf Chunk-Wechsel
            if (newChunkX != _chunkX || newChunkY != _chunkY)
            {
                OnChunkChanged?.Invoke(_chunkX, _chunkY, newChunkX, newChunkY);
                _chunkX = newChunkX;
                _chunkY = newChunkY;
            }
            
            
            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_screen);
            GraphicsDevice.Clear(Color.Black);

            int[] offsetX = [-5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5];
            int[] offsetY = [-5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5];

            foreach (var dx in offsetX)
            {
                foreach (var dy in offsetY)
                {
                    var targetX = _chunkX + dx;
                    var targetY = _chunkY + dy;

                    var validChunks = _chunks.Where(chunk => chunk.X == targetX && chunk.Y == targetY);
                    
                    //No Alpha Blend
                    foreach (var chunk in validChunks)
                    {
                        DrawModel(chunk.Terrain);
                        foreach (var building in chunk.Buildings)
                        {
                            DrawModel(building);
                        }
                    }
                }
            }

            foreach (var dx in offsetX)
            {
                foreach (var dy in offsetY)
                {
                    var targetX = _chunkX + dx;
                    var targetY = _chunkY + dy;

                    var validChunks = _chunks.Where(chunk => chunk.X == targetX && chunk.Y == targetY);
                    
                    
                    //Only Alpha Blend
                    foreach (var chunk in validChunks)
                    {
                        DrawModel(chunk.Terrain, true);
                        foreach (var building in chunk.Buildings)
                        {
                            DrawModel(building, true);
                        }
                    }
                }
            }

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.DrawString(_font, _camera.Rotation.ToString(), Vector2.Zero, Color.Red);
            var currentChunk = _chunks.FirstOrDefault(chunk => chunk.X == _chunkX && chunk.Y == _chunkY);

            if (currentChunk != null)
            {
                _spriteBatch.DrawString(_font, $"{_chunkX}, {_chunkY}, {currentChunk.Id}",
                    new Vector2(0, _font.LineSpacing), Color.Red);
                
                if (ShowArea)
                {
                    _spriteBatch.Draw(AreaTexture, new Vector2(134 - AreaTexture.Width , 38 - AreaTexture.Height), Color.White);
                    _spriteBatch.DrawString(_font, currentChunk.Header.LocationName, new Vector2(134 - AreaTexture.Width , 38 - AreaTexture.Height) + new Vector2(8, 7), Color.White);
                }
            }

            _spriteBatch.End();
            
            GraphicsDevice.SetRenderTarget(null);
            
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_screen, GraphicsDevice.Viewport.Bounds, Color.White);
            _spriteBatch.End();
            
            base.Draw(gameTime);
        }

        private void DrawModel(GameModel model, bool alpha = false)
        {
            foreach (var scene in model.Scenes)
            {
                foreach (var nodeIndex in scene.Nodes)
                {
                    var node = model.Nodes[nodeIndex];
                    DrawNode(model, node, alpha);
                }
            }
        }

        private void DrawNode(GameModel model, GameNode node, bool alpha = false)
        {
            
            //skinnig here?
            
            if (node.HasMesh)
            {
                //DrawMesh(node, gameModel.Meshes[node.MeshIndex]);
                DrawMesh(model, node, node.Mesh, alpha);
            }
            
            if (node.HasChildren)
            {
                foreach (var child in node.Children)
                {
                    DrawNode(model, node.Model.Nodes[child], alpha);
                }
            }
        }

        private void DrawMesh(GameModel model, GameNode node, GameMesh mesh, bool alpha = false)
        {
            var alphaMode = 0;

            
            var worldMatrix = Matrix.CreateScale(model.Scale) *
                              Matrix.CreateFromQuaternion(model.Rotation) *
                              Matrix.CreateTranslation(model.Translation);

            _effect.Parameters["World"].SetValue(node.GlobalTransform * worldMatrix);
            _effect.Parameters["View"].SetValue(_camera.View);
            _effect.Parameters["Projection"].SetValue(_camera.Projection);

            if (model.IsPlaying)
            {
                if (model.Skins is { Length: > 0 })
                {
                    var skin = model.Skins[0];
                    if (skin.JointMatrices.Length > 128)
                    {
                        _effect.Parameters["SkinningEnabled"]?.SetValue(false);
                        // throw new Exception();
                    }
                    else
                    {
                        _effect.Parameters["Bones"]?.SetValue(skin.JointMatrices);
                        _effect.Parameters["NumberOfBones"]?.SetValue(skin.JointMatrices.Length);
                        _effect.Parameters["SkinningEnabled"]?.SetValue(true);
                    }
                }
                else
                {
                    _effect.Parameters["SkinningEnabled"]?.SetValue(false);
                }
            }
            else
            {
                _effect.Parameters["SkinningEnabled"]?.SetValue(false);
            }
            
            if (alpha == false)
            {
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

                        _effect.Parameters["EmissiveColorFactor"].SetValue(material.EmissiveFactor.ToVector4());
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

                        //if (material.HasNormalMap)
                        //{
                        //    //_effect.Parameters["NormalMapEnabled"]?.SetValue(true);
                        //    //_effect.Parameters["NormalMap"]?.SetValue(primitive.Material.NormalMap.Texture);
                        //    
                        //    //GraphicsDevice.SamplerStates[1] = primitive.Material.NormalMap.Sampler.SamplerState;
                        //}

                        if (material.HasEmissiveTexture)
                        {
                            _effect.Parameters["EmissiveTextureEnabled"].SetValue(true);
                            _effect.Parameters["EmissiveTexture"].SetValue(primitive.Material.EmissiveTexture.Texture);

                            GraphicsDevice.SamplerStates[3] = primitive.Material.EmissiveTexture.Sampler.SamplerState;
                        }
                    }

                    foreach (var pass in _effect.Techniques[Math.Max(alphaMode - 1, 0)].Passes)
                    {
                        pass.Apply();
                        GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0,
                            primitive.VertexBuffer.VertexCount / 3);
                    }
                }
            }
            else
            {

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
                        GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0,
                            primitive.VertexBuffer.VertexCount / 3);
                    }
                }
            }
        }
    }
}