using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HxGLTF;
using HxGLTF.Implementation;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;
using TestRendering;

namespace TestRender
{
    public class Game1 : Game
    {   
        private GraphicsDeviceManager _graphicsDeviceManager;
        private SpriteBatch _spriteBatch;
        
        private Effect _effect;
        private Effect _worldShader;
        private Effect _buildingShader;
        private Camera _camera;
        private SpriteFont _font;
        private RenderTarget2D _screen;
        private GameModel _hero;
        private GameModel _heroShadow;
        private World _world;

        private bool _debugTexture = false;
        
        private TextureAnimation _seaAnimation;
        private TextureAnimation _hamabeAnimation;
        private TextureAnimation _seaRockAnimation;

        private List<TextureAnimation> _animations = [];
        private Dictionary<int, SoundEffect> _soundEffects = [];
        
        private int _chunkX = 5;
        private int _chunkY = 27;

        private int _cellX = 5;
        private int _cellY = 5;
        
        private Vector2 CellTarget;

        private int matrix = 0;
        
        private float _heroX = 0;
        private float _heroHeight = 0;
        private float _heroY = 0;
        private bool _shadow = true;

        private Vector3 FogColor = Vector3.One;
        private float FogStart;
        private float FogEnd;
        private bool IsFoggy;

        public Game1()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);

            _graphicsDeviceManager.PreferredBackBufferHeight = 960;
            _graphicsDeviceManager.PreferredBackBufferWidth = 1280;
            
            _graphicsDeviceManager.ApplyChanges();
            
            _heroX = _chunkX * 512;
            _heroX += _cellX * 16;
            
            _heroY = _chunkY * 512;
            _heroY += _cellY * 16;
        }

        protected override void Initialize()
        {
            _screen = new RenderTarget2D(GraphicsDevice, 256 * 4, 192 * 4, false,
                GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24Stencil8);
            _camera = new Camera(GraphicsDevice);

            _world = World.Load(GraphicsDevice, matrix);
            _hero = GameModel.From(GraphicsDevice, GLTFLoader.Load("A:\\ModelExporter\\Platin\\output_assets\\hero\\hero"));
            _heroShadow = GameModel.From(GraphicsDevice, GLTFLoader.Load("A:\\ModelExporter\\Platin\\output_assets\\kage.002\\kage"));

            _hamabeAnimation = new TextureAnimation("Hamabe", "hamabe", 0.32f, 8, ["hamabe_lm"]);
            _seaRockAnimation = new TextureAnimation("SeaRock", "searock", 0.16f, 4, ["searock_"]);
            _seaAnimation = new TextureAnimation("Sea", "sea", 0.32f, 8, ["sea_"]);
            
            _animations.Add(new TextureAnimation("Lakep", "lakep", 0.32f, 4, ["lakep_lm"]));
            _animations.Add(new TextureAnimation("C1_Lamp1", "c1_lamp01", 0.16f, 5, ["c1_lamp01_", "lamp01"], AnimationPlayMode.Bounce, 0.64f));
            _animations.Add(new TextureAnimation("C1_Lamp2", "c1_lamp02", 0.16f, 5, ["c1_lamp02_", "lamp03"], AnimationPlayMode.Bounce, 0.64f));
            _animations.Add(new TextureAnimation("C1_S02_3", "c1_s02_3", 0.32f, 4, ["c1_s02_4"]));
            _animations.Add(new TextureAnimation("C1_S01_D", "c1_s01_d", 0.32f, 4, ["c1_s01_d"]));
            _animations.Add(_hamabeAnimation);
            _animations.Add(_seaRockAnimation);
            _animations.Add(_seaAnimation);
            
            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            // ReSharper disable once HeapView.ObjectAllocation.Evident
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _effect = Content.Load<Effect>("PBRShader");
            _worldShader = Content.Load<Effect>("Shaders/WorldShader");
            _buildingShader = Content.Load<Effect>("Shaders/BuildingShader");
            _effect.Parameters["Bones"]?.SetValue(new Matrix[64]); 
            _font = Content.Load<SpriteFont>("Font");
            _camera.Teleport(new Vector3(0.25f, 1f, 0.5f) * 32);
            
            var songJson = File.ReadAllText($@"Content/SoundData.json");
            var jSongArray = JArray.Parse(songJson);

            foreach (var jSong in jSongArray)
            {
                try
                {
                    var songIdToken = jSong["soundId"];
                    if (songIdToken == null)
                    {
                        throw new Exception();
                    }
                    var songNameToken = jSong["soundName"];
                    if (songNameToken == null)
                    {
                        throw new Exception();
                    }
                    _soundEffects.Add(songIdToken.Value<int>(), Content.Load<SoundEffect>($@"Sounds\{songNameToken}"));
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
            
            foreach (var animation in _animations)
            {
                animation.LoadContent(Content);
            }
        }
        
        private Vector3 GetDirectionFromInput()
        {
            var direction = Vector3.Zero;

            if (KeyboardHandler.IsKeyDown(Keys.Q))
            {
                direction += Vector3.Down;
            }
            else if (KeyboardHandler.IsKeyDown(Keys.E))
            {
                direction += Vector3.Up;
            }

            if (KeyboardHandler.IsKeyDown(Keys.W))
            {
                direction += Vector3.Forward;
            }
            else if (KeyboardHandler.IsKeyDown(Keys.A))
            {
                direction += Vector3.Left;
            }
            else if (KeyboardHandler.IsKeyDown(Keys.D))
            {
                direction += Vector3.Right;
            }
            else if (KeyboardHandler.IsKeyDown(Keys.S))
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
            //Animation

            foreach (var animation in _animations)
            {
                animation.Update(gameTime);
            }
            
            _worldShader.Parameters["Delta"]?.SetValue(delta);
            _worldShader.Parameters["Total"]?.SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            
            _buildingShader.Parameters["Delta"]?.SetValue(delta);
            _buildingShader.Parameters["Total"]?.SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            
            KeyboardHandler.Update(gameTime);

            if (KeyboardHandler.IsKeyDownOnce(Keys.T))
            {
                _soundEffects[1004].Play();
                _debugTexture = true;
                Console.WriteLine("--------------------");
            }
            
            if (KeyboardHandler.IsKeyDownOnce(Keys.L))
            {
                var morningAngle = new Vector3(-0.4f, -1f, -0.2f);
                var dayAngle = new Vector3(0f, -1f, -0.2f);
                var noonAngle = new Vector3(0.4f, -1f, -0.2f);
                var lightPosition = _camera.Position;
                var lightDirection = new Vector3(0, -1, 0);
                Console.WriteLine(lightPosition);
                Console.WriteLine(lightDirection);
                _worldShader.Parameters["LightPosition"]?.SetValue(lightPosition);
                _worldShader.Parameters["LightDirection"]?.SetValue(lightDirection);
                _buildingShader.Parameters["LightPosition"]?.SetValue(lightPosition);
                _buildingShader.Parameters["LightDirection"]?.SetValue(lightDirection);
            }
            
            if (KeyboardHandler.IsKeyDownOnce(Keys.F))
            {
                IsFoggy = !IsFoggy;
            }
            
            FogStart = 16f;
            FogEnd = 32f;

            if (IsFoggy)
            {
                // Welt-Shader-Parameter setzen
                _worldShader.Parameters["fogColor"]?.SetValue(FogColor);
                _worldShader.Parameters["fogStart"]?.SetValue(FogStart);
                _worldShader.Parameters["fogEnd"]?.SetValue(FogEnd);

                // Gebäude-Shader-Parameter setzen
                _buildingShader.Parameters["fogColor"]?.SetValue(FogColor);
                _buildingShader.Parameters["fogStart"]?.SetValue(FogStart);
                _buildingShader.Parameters["fogEnd"]?.SetValue(FogEnd);
            }
            
            _worldShader.Parameters["Fog"]?.SetValue(IsFoggy);
            _worldShader.Parameters["CameraPosition"]?.SetValue(_camera.Position);
            if (KeyboardHandler.IsKeyDownOnce(Keys.PageDown))
            {
                matrix = Math.Max(matrix - 1, 0);
            }

            if (KeyboardHandler.IsKeyDownOnce(Keys.PageUp))
            {
                matrix = Math.Min(matrix + 1, 288);
            }

            if (KeyboardHandler.IsKeyDownOnce(Keys.Space))
            {
                _world = World.Load(GraphicsDevice, matrix);
            }
            
            /*var direction = GetDirectionFromInput();

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
                        if (type == 0x5E)
                        {
                            _world = World.Load(GraphicsDevice, 0);
                            _chunkX = 3;
                            _chunkY = 27;
                            _cellX = 15;
                            _cellY = 15;
                            _heroX = _chunkX * 512;
                            _heroX += _cellX * 16;

                            _heroY = _chunkY * 512;
                            _heroY += _cellY * 16;
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
            }*/

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

                Direction *= 64;
                

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
            
            foreach (var chunk in World.Chunks)
            {
                foreach (var building in chunk.Value.Buildings)
                {
                    building.Model.Update(gameTime);
                    building.Model.Play(0);
                }
            }
            
            UpdateCamera(gameTime);

            _chunkX = (int)_camera.Position.X / 32;
            _chunkY = (int)_camera.Position.Z / 32;

            base.Update(gameTime);
        }

        private void UpdateCamera(GameTime gameTime)
        {
            //_camera.Teleport(new Vector3(0f, 0.5f, 0.5f) * 512 + new Vector3(_heroX, _heroHeight, _heroY));
            //_camera.RotateTo(new Vector3(0.8185586f, (float)Math.PI, 0f));
            //_hero.RotateTo(Quaternion.CreateFromRotationMatrix(_camera.RotationMInvX));
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
            if (!IsActive)
            {
                return;
            }
            GraphicsDevice.SetRenderTarget(_screen);
            GraphicsDevice.Clear(Color.Black);

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
                        DrawModel(gameTime, _worldShader, chunk.Model,
                            offset: new Vector3(targetX * 32, tuple.height / 2f, targetY * 32) +
                                    new Vector3(16, 0, 16));
                        foreach (var building in chunk.Buildings)
                        {
                            DrawModel(gameTime, _buildingShader, building.Model,
                                offset: new Vector3(targetX * 32, tuple.height / 2f, targetY * 32) +
                                        new Vector3(16, 0, 16));
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
                        DrawModel(gameTime, _worldShader, chunk.Model,
                            offset: new Vector3(targetX * 32, tuple.height / 2f, targetY * 32) +
                                    new Vector3(16, 0, 16), alpha: true);
                        foreach (var building in chunk.Buildings)
                        {
                            DrawModel(gameTime, _buildingShader, building.Model,
                                offset: new Vector3(targetX * 32, tuple.height / 2f, targetY * 32) +
                                        new Vector3(16, 0, 16), alpha: true);
                        }
                    }
                }
            }
            /*
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
                            DrawModel(gameTime, _worldShader, chunk.Model,
                                offset: new Vector3(targetX * 32, tuple.height / 2f, targetY * 32) +
                                        new Vector3(16, 0, 16), alpha: false);
                            foreach (var building in chunk.Buildings)
                            {
                                DrawModel(gameTime, _buildingShader, building.Model,
                                    offset: new Vector3(targetX * 32, tuple.height / 2f, targetY * 32) +
                                            new Vector3(16, 0, 16), alpha: false);
                            }
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

                    if (_world.Combination.TryGetValue((targetX, targetY), out var tuple))
                    {
                        var chunkId = tuple.chunkId;
                        var headerId = tuple.headerId;

                        if (World.Chunks.TryGetValue(chunkId, out var chunk))// && _world.Headers.TryGetValue(headerId, out var header))
                        {
                            if (chunk.IsLoaded && chunk.Model != null)
                            {
                                DrawModel(gameTime, _worldShader, chunk.Model,
                                    offset: new Vector3(targetX * 32, tuple.height / 2f, targetY * 32) +
                                            new Vector3(16, 0, 16), alpha: true);
                                foreach (var building in chunk.Buildings)
                                {
                                    DrawModel(gameTime, _buildingShader, building.Model,
                                        offset: new Vector3(targetX * 32, tuple.height / 2f, targetY * 32) +
                                                new Vector3(16, 0, 16), alpha: true);
                                }
                            }
                        }
                    }
                }
            }*/

           

            
            GraphicsDevice.SetRenderTarget(null);
            
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, depthStencilState: DepthStencilState.DepthRead, blendState: BlendState.AlphaBlend, sortMode: SpriteSortMode.Deferred);
            _spriteBatch.Draw(_screen, GraphicsDevice.Viewport.Bounds, Color.White);
            //_spriteBatch.Draw(_screen, new Rectangle(0, 0, 256 * 2, 192 * 2), Color.White);
            
            _spriteBatch.DrawString(_font, $"Chunk: [{_chunkX}, {_chunkY}]", Vector2.Zero, Color.White);
            _spriteBatch.DrawString(_font, $"Cell: [{_cellX}, {_cellY}]", new Vector2(0, _font.LineSpacing), Color.White);
            _spriteBatch.DrawString(_font, $"World: [{_heroX}, {_heroY}]", new Vector2(0, _font.LineSpacing * 2), Color.White);
            _spriteBatch.DrawString(_font, $"Matrix: [{matrix}]", new Vector2(0, _font.LineSpacing * 3), Color.White);

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
            if (_debugTexture)
            {
                _debugTexture = false;
            }

            base.Draw(gameTime);
        }

        private void DrawModel(GameTime gameTime, Effect effect, GameModel model, bool alpha = false, Vector3 offset = default)
        {
            foreach (var scene in model.Scenes)
            {
                foreach (var nodeIndex in scene.Nodes)
                {
                    var node = model.Nodes[nodeIndex];
                    DrawNode(gameTime, effect, model, node, alpha, offset);
                }
            }
        }

        private void DrawNode(GameTime gameTime, Effect effect, GameModel model, GameNode node, bool alpha = false, Vector3 offset = default)
        {
            
            //skinnig here?
            
            if (node.HasMesh)
            {
                //DrawMesh(node, gameModel.Meshes[node.MeshIndex]);
                DrawMesh(gameTime, effect, model, node, node.Mesh, alpha, offset);
            }
            
            if (node.HasChildren)
            {
                foreach (var child in node.Children)
                {
                    DrawNode(gameTime, effect, model, node.Model.Nodes[child], alpha, offset);
                }
            }
        }

        private void DrawMesh(GameTime gameTime, Effect effect, GameModel model, GameNode node, GameMesh mesh, bool alpha = false, Vector3 offset = default)
        {
            var alphaMode = 0;

            var worldMatrix = Matrix.CreateScale(model.Scale) *
                              Matrix.CreateFromQuaternion(model.Rotation) *
                              Matrix.CreateTranslation(model.Translation) *
                              Matrix.CreateTranslation(offset);

            effect.Parameters["World"].SetValue(node.GlobalTransform * worldMatrix);
            effect.Parameters["View"].SetValue(_camera.View);
            effect.Parameters["Projection"].SetValue(_camera.Projection);

            if (model.IsPlaying)
            {
                if (model.Skins is { Length: > 0 })
                {
                    var skin = model.Skins[0];
                    if (skin.JointMatrices.Length > 128)
                    {
                        effect.Parameters["SkinningEnabled"]?.SetValue(false);
                    }
                    else
                    {
                        effect.Parameters["Bones"]?.SetValue(skin.JointMatrices);
                        effect.Parameters["NumberOfBones"]?.SetValue(skin.JointMatrices.Length);
                        effect.Parameters["SkinningEnabled"]?.SetValue(true);
                    }
                }
                else
                {
                    effect.Parameters["SkinningEnabled"]?.SetValue(false);
                }
            }
            else
            {
                effect.Parameters["SkinningEnabled"]?.SetValue(false);
            }

            foreach (var primitive in mesh.Primitives)
            {
                if (ShouldSkipPrimitive(primitive, effect, alpha, ref alphaMode))
                {
                    continue;
                }

                SetPrimitiveMaterialParameters(gameTime, primitive, effect);

                foreach (var pass in effect.Techniques[Math.Max(alphaMode - 1, 0)].Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0,
                        primitive.VertexBuffer.VertexCount / 3);
                }
            }
        }

        private bool ShouldSkipPrimitive(GameMeshPrimitives primitive, Effect effect, bool alpha, ref int alphaMode)
        {
            if (primitive.Material != null)
            {
                var material = primitive.Material;

                switch (material.AlphaMode)
                {
                    case "OPAQUE":
                        if (alpha)
                        {
                            return true;
                        }
                        alphaMode = 0;
                        break;
                    case "MASK":
                        if (alpha)
                        {
                            return true;
                        }
                        alphaMode = 1;
                        break;
                    case "BLEND":
                        if (!alpha)
                        {
                            return true;
                        }
                        alphaMode = 2;
                        break;
                }

                effect.Parameters["AlphaMode"]?.SetValue(alphaMode);
            }
            return false;
        }

        private void SetPrimitiveMaterialParameters(GameTime gameTime, GameMeshPrimitives primitive, Effect effect)
        {
            GraphicsDevice.SetVertexBuffer(primitive.VertexBuffer);

            effect.Parameters["TextureEnabled"]?.SetValue(false);
            effect.Parameters["NormalMapEnabled"]?.SetValue(false);
            effect.Parameters["OcclusionMapEnabled"]?.SetValue(false);
            effect.Parameters["EmissiveTextureEnabled"]?.SetValue(false);

            if (primitive.Material != null)
            {
                var material = primitive.Material;

                if (_debugTexture)
                {
                    Console.WriteLine(material.Name);
                }
                
                effect.Parameters["EmissiveColorFactor"]?.SetValue(material.EmissiveFactor.ToVector4());
                effect.Parameters["BaseColorFactor"]?.SetValue(material.BaseColorFactor.ToVector4());
                effect.Parameters["AlphaCutoff"]?.SetValue(material.AlphaCutoff);

                if (material.HasTexture)
                {
                    effect.Parameters["TextureEnabled"]?.SetValue(true);
                    effect.Parameters["TextureDimensions"]?.SetValue(material.BaseTexture.Texture.Bounds.Size.ToVector2());
                    effect.Parameters["Texture"]?.SetValue(material.BaseTexture.Texture);
                    effect.Parameters["TextureAnimation"]?.SetValue(false);
                    SetTextureAnimation(gameTime, material, effect);

                    GraphicsDevice.SamplerStates[0] = material.BaseTexture.Sampler.SamplerState;
                }
            }
        }

        private void SetTextureAnimation(GameTime gameTime, GameMaterial material, Effect effect)
        {
            foreach (var animation in _animations)
            {
                foreach (var keyWord in animation.ForMaterial)
                {
                    if (material.Name.Contains(keyWord))
                    {
                        effect.Parameters["Texture"]?.SetValue(animation.CurrentFrame);
                        return;
                    }
                }
            }
            if (material.Name.Contains("c3_s03b"))
            {
                effect.Parameters["AnimationSpeed"]?.SetValue(16);
                effect.Parameters["AnimationDirection"]?.SetValue((byte)TextureAnimationDirection.Up);
                effect.Parameters["TextureAnimation"]?.SetValue(true);
            }
            else if (material.Name.Contains("taki"))
            {
                effect.Parameters["AnimationSpeed"]?.SetValue(16);
                effect.Parameters["AnimationDirection"]?.SetValue((byte)TextureAnimationDirection.Down);
                effect.Parameters["TextureAnimation"]?.SetValue(true);
            }
            else if (material.Name.Contains("c1_fun2"))
            {
                effect.Parameters["AnimationSpeed"]?.SetValue(32);
                effect.Parameters["AnimationDirection"]?.SetValue((byte)TextureAnimationDirection.Down);
                effect.Parameters["TextureAnimation"]?.SetValue(true);
            }
            else if (material.Name.Contains("mag_smoke"))
            {
                effect.Parameters["AnimationSpeed"]?.SetValue(16);
                effect.Parameters["AnimationDirection"]?.SetValue((byte)TextureAnimationDirection.Up);
                effect.Parameters["TextureAnimation"]?.SetValue(true);
            }
            else if (material.Name.Contains("kemuri"))
            {
                effect.Parameters["AnimationSpeed"]?.SetValue(32);
                if ((int)gameTime.TotalGameTime.TotalSeconds % 2 == 0)
                {
                    effect.Parameters["AnimationDirection"]?.SetValue((byte)TextureAnimationDirection.Up);
                }
                else
                {
                    effect.Parameters["AnimationDirection"]?.SetValue((byte)TextureAnimationDirection.Down);
                }
                effect.Parameters["AnimationDirection"]?.SetValue((byte)TextureAnimationDirection.Left);
                effect.Parameters["TextureAnimation"]?.SetValue(true);
            }
            else if (material.Name.Contains("l_lake"))
            {
                effect.Parameters["AnimationSpeed"]?.SetValue(16);
                effect.Parameters["AnimationDirection"]?.SetValue((byte)TextureAnimationDirection.DownLeft);
                effect.Parameters["TextureAnimation"]?.SetValue(true);
            }
        }
    }
}