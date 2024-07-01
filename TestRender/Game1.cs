using System;
using System.Linq;
using HxGLTF;
using HxGLTF.Implementation;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TestRendering;

namespace TestRender
{
    public class Game1 : Game
    {   
        private GraphicsDeviceManager _graphicsDeviceManager;
        private SpriteBatch _spriteBatch;
        
        private Effect _effect;
        private Effect _normalEffect;
        private Camera _camera;
        private SpriteFont _font;
        private RenderTarget2D _screen;
        private GameModel _hero;
        private GameModel _heroShadow;
        private World _world;
        
        private int _chunkX = 0;
        private int _chunkY = 0;

        private int _cellX = 5;
        private int _cellY = 5;
        private Vector2 CellTarget;
        private float _heroX = 0;
        private float _heroHeight = 0;
        private float _heroY = 0;
        private bool _shadow = true;

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
            
            _heroX = _chunkX * 512;
            _heroX += _cellX * 16;
            
            _heroY = _chunkY * 512;
            _heroY += _cellY * 16;
        }

        protected override void Initialize()
        {
            _screen = new RenderTarget2D(GraphicsDevice, 1280 / 1, 720 / 1, false,
                GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24Stencil8);
            _camera = new Camera(GraphicsDevice);

            _world = World.Load(GraphicsDevice, "129");
            _hero = GameModel.From(GraphicsDevice, GLTFLoader.Load("A:\\ModelExporter\\Platin\\output_assets\\hero\\hero"));
            _heroShadow = GameModel.From(GraphicsDevice, GLTFLoader.Load("A:\\ModelExporter\\Platin\\output_assets\\kage.002\\kage"));

            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            // ReSharper disable once HeapView.ObjectAllocation.Evident
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _effect = Content.Load<Effect>("PBRShader");
            _effect.Parameters["Bones"].SetValue(new Matrix[64]); 
            _font = Content.Load<SpriteFont>("Font");
            _camera.Teleport(new Vector3(0.25f, 1f, 0.5f) * 512);
        }
        
        private Vector3 GetDirectionFromInput()
        {
            var direction = Vector3.Zero;

            if (KeyboardHandler.IsKeyDownOnce(Keys.Q))
            {
                direction += Vector3.Down;
            }
            else if (KeyboardHandler.IsKeyDownOnce(Keys.E))
            {
                direction += Vector3.Up;
            }

            if (KeyboardHandler.IsKeyDownOnce(Keys.W))
            {
                direction += Vector3.Forward;
            }
            else if (KeyboardHandler.IsKeyDownOnce(Keys.A))
            {
                direction += Vector3.Left;
            }
            else if (KeyboardHandler.IsKeyDownOnce(Keys.D))
            {
                direction += Vector3.Right;
            }
            else if (KeyboardHandler.IsKeyDownOnce(Keys.S))
            {
                direction += Vector3.Backward;
            }

            return direction;
        }

        protected override void Update(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!IsActive)
            {
                return;
            }

            KeyboardHandler.Update(gameTime);

            var direction = GetDirectionFromInput();

            if (direction != Vector3.Zero)
            {
                direction.Normalize();
                var stepSize = 16;
                
                CellTarget = new Vector2(_heroX, _heroY) + new Vector2(direction.X, direction.Z) * stepSize;
                _heroHeight += direction.Y * stepSize;
                
                
                var newHeroPosition = new Vector3(CellTarget.X, 0, CellTarget.Y);

                var newChunkX = (int)newHeroPosition.X / 512;
                var newChunkY = (int)newHeroPosition.Z / 512;

                var chunk = _world.GetChunkAtPosition(newHeroPosition);

                if (chunk != null)
                {
                    var collision = _world.CheckTileCollision(newHeroPosition);
                    var type = _world.CheckTileType(newHeroPosition);
                    
                    if (collision == 0x100)
                    {
                        if (type != 0x00)
                        {
                            Console.WriteLine(type);
                            Console.WriteLine(direction);
                            if (type == (byte)TileType.JumpDown)
                            {
                                if (direction == Vector3.Backward)
                                {
                                    _heroX = CellTarget.X;
                                    _heroY = CellTarget.Y + 16;
                                    _chunkX = newChunkX;
                                    _chunkY = newChunkY;
                                    _cellX = (int)(_heroX % 512) / 16;
                                    _cellY = (int)(_heroY % 512) / 16;
                                }
                            }
                            else if (type == (byte)TileType.TV)
                            {
                                
                            }
                            else
                            {
                                _heroX = CellTarget.X;
                                _heroY = CellTarget.Y;
                                _chunkX = newChunkX;
                                _chunkY = newChunkY;
                                _cellX = (int)(_heroX % 512) / 16;
                                _cellY = (int)(_heroY % 512) / 16;
                            }
                        }
                    }
                    else
                    {
                        _heroX = CellTarget.X;
                        _heroY = CellTarget.Y;
                        _chunkX = newChunkX;
                        _chunkY = newChunkY;
                        _cellX = (int)(_heroX % 512) / 16;
                        _cellY = (int)(_heroY % 512) / 16;
                    }
                    
                }
                else
                {
                    _heroX = CellTarget.X;
                    _heroY = CellTarget.Y;
                    _chunkX = newChunkX;
                    _chunkY = newChunkY;
                    _cellX = (int)(_heroX % 512) / 16;
                    _cellY = (int)(_heroY % 512) / 16;
                }
            }

            UpdateCamera(gameTime);

            base.Update(gameTime);
        }

        private void UpdateCamera(GameTime gameTime)
        {
            _camera.Teleport(new Vector3(0f, 0.5f, 0.5f) * 512 + new Vector3(_heroX, _heroHeight, _heroY));
            _camera.RotateTo(new Vector3(0.8185586f, (float)Math.PI, 0f));
            _hero.RotateTo(Quaternion.CreateFromRotationMatrix(_camera.RotationMInvX));
            _camera.Update(gameTime);
        }

        private void UnloadDistantChunks()
        {
            // Sichtbarer Bereich: Hier kann je nach Spiellogik die Sichtweite angepasst werden
            int visibleRangeX = 1;
            int visibleRangeY = 1;

            // Entlade Chunks außerhalb des sichtbaren Bereichs
            foreach (var chunkEntry in _world.Combination.ToList()) // ToList(), um während des Iterierens zu ändern
            {
                var ((x, y), (chunkId, headerId, height)) = chunkEntry;

                if (Math.Abs(x - _chunkX) > visibleRangeX || Math.Abs(y - _chunkY) > visibleRangeY)
                {
                    if (World.Chunks.TryGetValue(chunkId, out var chunk))
                    {
                        chunk.Unload();
                    }
                }
            }
        }
        
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.SetRenderTarget(_screen);
            GraphicsDevice.Clear(Color.Black);

            /*foreach (var chunkEntry in _world.Combination)
            {
                var (targetX, targetY) = chunkEntry.Key;
                var tuple = chunkEntry.Value;

                var chunkId = tuple.chunkId;
                var headerId = tuple.headerId;

                if (World.Chunks.TryGetValue(chunkId, out var chunk))
                {
                    if (chunk.IsLoaded && chunk.Model != null)
                    {
                        DrawModel(chunk.Model,
                            offset: new Vector3(targetX * 512, tuple.height * 8, targetY * 512) +
                                    new Vector3(256, 0, 256));
                        foreach (var building in chunk.Buildings)
                        {
                            DrawModel(building.Model,
                                offset: new Vector3(targetX * 512, tuple.height * 8, targetY * 512) +
                                        new Vector3(256, 0, 256));

                        }
                        for (int y = 0; y < chunk.Collision.GetLength(0); y++)
                        {
                            for (int x = 0; x < chunk.Collision.GetLength(1); x++)
                            {
                                if (chunk.Collision[y, x] != 0)
                                {
                                    // Berechnung der Position des Modells basierend auf Zellenkoordinaten
                                    float posX = (x * 16); // 16 ist die Zellengröße, 8 für die Hälfte der Zelle
                                    float posZ = (y * 16) ;

                                    // Hier kannst du dein Modell zeichnen
                                    DrawModel(_hero, offset: new Vector3(posX, 0, posZ));
                                }
                            }
                        }
                    }
                }
            }

            foreach (var chunkEntry in _world.Combination)
            {
                var (targetX, targetY) = chunkEntry.Key;
                var tuple = chunkEntry.Value;

                var chunkId = tuple.chunkId;
                var headerId = tuple.headerId;

                if (World.Chunks.TryGetValue(chunkId, out var chunk))
                {
                    if (chunk.IsLoaded && chunk.Model != null)
                    {
                        DrawModel(chunk.Model,
                            offset: new Vector3(targetX * 512, tuple.height * 8, targetY * 512) +
                                    new Vector3(256, 0, 256), alpha: true);
                        foreach (var building in chunk.Buildings)
                        {
                            DrawModel(building.Model,
                                offset: new Vector3(targetX * 512, tuple.height * 8, targetY * 512) +
                                        new Vector3(256, 0, 256), alpha: true);

                        }
                    }
                }
            }*/
            
            int[] offsetX = [-5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5];
            int[] offsetY = [-5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5];

            foreach (var dx in offsetX)
            {
                foreach (var dy in offsetY)
                {
                    var targetX = _chunkX + dx;
                    var targetY = _chunkY + dy;

                    if (!_world.Combination.TryGetValue((targetX, targetY), out var tuple))
                    {
                        continue;
                    }

                    var chunkId = tuple.chunkId;
                    var headerId = tuple.headerId;

                    if (World.Chunks.TryGetValue(chunkId, out var chunk))// && _world.Headers.TryGetValue(headerId, out var header))
                    {
                        if (chunk.IsLoaded && chunk.Model != null)
                        {
                            DrawModel(chunk.Model,
                                offset: new Vector3(targetX * 512, tuple.height * 8, targetY * 512) + new Vector3(256, 0, 256));
                            foreach (var building in chunk.Buildings)
                            {
                                DrawModel(building.Model,
                                    offset: new Vector3(targetX * 512, tuple.height * 8, targetY * 512) + new Vector3(256, 0, 256));

                            }
                        }
                    }
                }
            }
            
            DrawModel(_hero, offset: new Vector3(_heroX + 8f, _heroHeight, _heroY + 16f));

            foreach (var dx in offsetX)
            {
                foreach (var dy in offsetY)
                {
                    var targetX = _chunkX + dx;
                    var targetY = _chunkY + dy;

                    if (_world.Combination.TryGetValue((targetX, targetY), out var tuple))
                    {
                        var chunkId = tuple.chunkId;
                        var headerId = tuple.headerId;

                        if (World.Chunks.TryGetValue(chunkId, out var chunk))// && _world.Headers.TryGetValue(headerId, out var header))
                        {
                            if (chunk.IsLoaded && chunk.Model != null)
                            {
                                DrawModel(chunk.Model,
                                    offset: new Vector3(targetX * 512, tuple.height * 8, targetY * 512) + new Vector3(256, 0, 256), alpha: true);
                                foreach (var building in chunk.Buildings)
                                {
                                    DrawModel(building.Model,
                                        offset: new Vector3(targetX * 512, tuple.height * 8, targetY * 512) + new Vector3(256, 0, 256),
                                        alpha: true);

                                }
                            }
                        }
                    }
                }
            }
            
            if (_shadow)
            {
                DrawModel(_heroShadow, offset: new Vector3(_heroX + 8f, _heroHeight - 0.25f, _heroY + 16f),
                    alpha: true);
            }

           
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.DrawString(_font, $"Chunk: [{_chunkX}, {_chunkY}]", Vector2.Zero, Color.White);
            _spriteBatch.DrawString(_font, $"Cell: [{_cellX}, {_cellY}]", new Vector2(0, _font.LineSpacing), Color.White);
            _spriteBatch.DrawString(_font, $"World: [{_heroX}, {_heroY}]", new Vector2(0, _font.LineSpacing * 2), Color.White);

            try
            {
                if (_world.Combination.TryGetValue((_chunkX, _chunkY), out var targetChunkTuple))
                {
                    if (World.Chunks.TryGetValue(targetChunkTuple.chunkId, out var targetChunk))
                    {
                        _spriteBatch.DrawString(_font, $"Collision: [{targetChunk.Collision[_cellY, _cellX]:x2}]",
                            new Vector2(0, _font.LineSpacing * 4), Color.White);
                        var name = Enum.GetName(typeof(TileType), targetChunk.Type[_cellY, _cellX]);
                        _spriteBatch.DrawString(_font, $"Type: [{name}, {targetChunk.Type[_cellY, _cellX]:x2}]",
                            new Vector2(0, _font.LineSpacing * 5), Color.White);
                    }
                }
                else
                {
                    _spriteBatch.DrawString(_font, $"Collision: [!]", new Vector2(0, _font.LineSpacing * 4),
                        Color.White);
                    _spriteBatch.DrawString(_font, $"Type: [!]", new Vector2(0, _font.LineSpacing * 5), Color.White);
                }
            }
            catch (Exception e)
            {
                
            }

            _spriteBatch.End();
            
            //GraphicsDevice.SetRenderTarget(null);
            
            //_spriteBatch.Begin(samplerState: SamplerState.PointClamp, depthStencilState: DepthStencilState.DepthRead, blendState: BlendState.AlphaBlend, sortMode: SpriteSortMode.Deferred);
            //_spriteBatch.Draw(_screen, GraphicsDevice.Viewport.Bounds, Color.White);
            //_spriteBatch.Draw(_screen, new Rectangle(0, 0, 256, 192), Color.White);
            //_spriteBatch.End();
            
            base.Draw(gameTime);
        }

        private void DrawModel(GameModel model, bool alpha = false, Vector3 offset = default)
        {
            foreach (var scene in model.Scenes)
            {
                foreach (var nodeIndex in scene.Nodes)
                {
                    var node = model.Nodes[nodeIndex];
                    DrawNode(model, node, alpha, offset);
                }
            }
        }

        private void DrawNode(GameModel model, GameNode node, bool alpha = false, Vector3 offset = default)
        {
            
            //skinnig here?
            
            if (node.HasMesh)
            {
                //DrawMesh(node, gameModel.Meshes[node.MeshIndex]);
                DrawMesh(model, node, node.Mesh, alpha, offset);
            }
            
            if (node.HasChildren)
            {
                foreach (var child in node.Children)
                {
                    DrawNode(model, node.Model.Nodes[child], alpha, offset);
                }
            }
        }

        private void DrawMesh(GameModel model, GameNode node, GameMesh mesh, bool alpha = false, Vector3 offset = default)
        {
            var alphaMode = 0;

            var worldMatrix = Matrix.CreateScale(model.Scale) *
                              Matrix.CreateFromQuaternion(model.Rotation) *
                              Matrix.CreateTranslation(model.Translation);

            // Hier wird die Offset-Verschiebung angewendet
            worldMatrix *= Matrix.CreateTranslation(offset);

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