using System;
using System.Linq;
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
        private GameModel _hero;
        private World _world;
        
        private int _chunkX = 0;
        private int _chunkY = 0;

        private int _cellX = 0;
        private int _cellY = 0;
        
        private int _heroX = 0;
        private int _heroY = 0;
        
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
            _screen = new RenderTarget2D(GraphicsDevice, 1280 / 1, 720 / 1, false,
                GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24Stencil8);
            _camera = new Camera(GraphicsDevice);

            _world = World.Load(GraphicsDevice, "129");
            _hero = GameModel.From(GraphicsDevice, GLTFLoader.Load("A:\\ModelExporter\\Platin\\output_assets\\hero\\hero"));
            
            OnChunkChanged += HandleChunkChange;
            
            base.Initialize();
        }

        private void HandleChunkChange(int oldChunkX, int oldChunkY, int newChunkX, int newChunkY)
        {
            /*
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
            */
        }

        
        protected override void LoadContent()
        {
            // ReSharper disable once HeapView.ObjectAllocation.Evident
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _effect = Content.Load<Effect>("PBRShader");
            _effect.Parameters["Bones"].SetValue(new Matrix[64]); 
            _font = Content.Load<SpriteFont>("Font");
            _camera.Teleport(new Vector3(0.25f, 0.5f, 1f) * 512);
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

            // Kamera-Rotationen
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

            // Richtungsvektor initialisieren
            var direction = Vector3.Zero;

            // Richtungssteuerung mit diskreter Bewegung
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
                direction += Vector3.Down;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.E))
            {
                direction += Vector3.Up;
            }

            // Wenn eine Richtungstaste gedrückt wird, bewege den Spieler
            if (direction != Vector3.Zero)
            {
                // Normalisiere den Richtungsvektor und multipliziere mit der Schrittgröße (Kachelgröße)
                direction.Normalize();
                var stepSize = 1; // Annahme: Kachelgröße ist 16 Einheiten
                direction *= stepSize;

                // Berechne die neue Position des Helden
                var newHeroPosition = new Vector3(_heroX, 0, _heroY) + direction;

                // Überprüfe, ob die neue Position gültig ist und keine Kollision verursacht
                var newChunkX = (int)newHeroPosition.X / 512;
                var newChunkY = (int)newHeroPosition.Z / 512;

                if (_world.Combination.TryGetValue((newChunkX, newChunkY), out var tuple))
                {
                    var chunkId = tuple.chunkId;
                    var headerId = tuple.headerId;

                    if (World.Chunks.TryGetValue(chunkId, out var chunk))
                    {
                        if (chunk.IsLoaded && chunk.Model != null)
                        {
                            bool collisionDetected = false;

                            // Überprüfe Kollisionen in diesem Chunk
                            for (int y = 0; y < chunk.Collision.GetLength(0); y++)
                            {
                                for (int x = 0; x < chunk.Collision.GetLength(1); x++)
                                {
                                    if (chunk.Collision[y, x] != 0)
                                    {
                                        // Berechnung der Position des Modells basierend auf Zellenkoordinaten
                                        float posX = (x * 16); // 16 ist die Zellengröße
                                        float posZ = (y * 16);

                                        // Prüfe, ob die neue Hero-Position in einer kollidierenden Zelle liegt
                                        if (newHeroPosition.X >= posX && newHeroPosition.X < posX + 16 &&
                                            newHeroPosition.Z >= posZ && newHeroPosition.Z < posZ + 16)
                                        {
                                            collisionDetected = true;
                                            break;
                                        }
                                    }
                                }

                                if (collisionDetected)
                                {
                                    break;
                                }
                            }

                            // Wenn keine Kollision detektiert wurde, aktualisiere die Hero-Position
                            if (!collisionDetected)
                            {
                                _heroX = (int)newHeroPosition.X;
                                _heroY = (int)newHeroPosition.Z;
                            }
                        }
                    }
                }
            }

            // Kamera aktualisieren
            _camera.Update(gameTime);

            base.Update(gameTime);
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

            int[] offsetX = [-1, 0, 1];
            int[] offsetY = [-1, 0, 1];
            
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
                                        //DrawModel(_hero, offset: new Vector3(posX, 0, posZ));
                                    }
                                }
                            }
                        }
                        else
                        {
                            //chunk.Load(GraphicsDevice);
                        }
                    }
                }
            }
            
            DrawModel(_hero, offset: new Vector3(_heroX, 0, _heroY));
            
            /* foreach (var dx in offsetX)
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
                                     offset: new Vector3(targetX * 512, tuple.height * 8, targetY * 512), alpha: true);
                                 foreach (var building in chunk.Buildings)
                                 {
                                     DrawModel(building.Model,
                                         offset: new Vector3(targetX * 512, tuple.height * 8, targetY * 512),
                                         alpha: true);

                                 }
                             }

                             else
                             {chunk.Load(GraphicsDevice);
                                 var task = Task.Run(
                                     () => {  });
                             }
                         }
                     }
                 }
             }*/

           
           
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.DrawString(_font, $"{_chunkX}, {_chunkY}", Vector2.Zero, Color.Red);
            _spriteBatch.DrawString(_font, $"{_cellX}, {_cellY}", new Vector2(0, _font.LineSpacing), Color.Red);
           
            if (_world.Combination.TryGetValue((_chunkX, _chunkY), out var targetChunkTuple))
            {
                if (World.Chunks.TryGetValue(targetChunkTuple.chunkId, out var targetChunk))
                {
                    _spriteBatch.DrawString(_font, $"{targetChunk.Collision[_cellY, _cellX]}", new Vector2(0, _font.LineSpacing * 2), Color.Red);
                }
            }
            else
            {
                _spriteBatch.DrawString(_font, $"xx", new Vector2(0, _font.LineSpacing * 2), Color.Red);
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