using System;
using System.Collections.Generic;
using System.IO;
using HxGLTF.Implementation;
using HxLocal;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json.Linq;
using PatteLib.Data;
using PatteLib.Graphics;
using PatteLib.World;
using TestRendering;

namespace TestRender;

public class Game1 : Game
{   
    private GraphicsDeviceManager _graphicsDeviceManager;
    private SpriteBatch _spriteBatch;

    public HeaderManager HeaderManager;
    public TextArchiveManager TextArchiveManager;
    
    private AlphaTestEffect _basicEffect;
    private Effect _worldShader;
    private Effect _buildingShader;
    private Camera _camera;
    private World _world;
    private WorldTimeManager _timeManager;
    private Texture2D _pixel;
    private ImageFont _imageFont;
    private ImageFontRenderer _fontRenderer;

    private Texture2D _textBox;
    private float _frameCount = 0;

    private Dictionary<int, Texture2D> _sprites = [];
    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    
    private List<TextureAnimation> _animations = [];
    private Dictionary<int, SoundEffect> _soundEffects = [];
    private Dictionary<int, Music> _musics = [];
    private SoundEffectInstance _currentSoundEffectInstance;
    
    private int _matrix = 0;
    private bool _debugTexture = false;

    public Game1()
    {
        _graphicsDeviceManager = new GraphicsDeviceManager(this);
        Content.RootDirectory = @"Content";
        IsMouseVisible = true;
            
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);

        _graphicsDeviceManager.PreferredBackBufferHeight = 960;
        _graphicsDeviceManager.PreferredBackBufferWidth = 1280;
            
        _graphicsDeviceManager.ApplyChanges();

        MediaPlayer.Volume = 0.3f;
    }

    protected override void Initialize()
    {
        _timeManager = new WorldTimeManager();
        
        Localisation.RootDirectory = @"Content\Localisation";
        Localisation.SetLanguage("de");
        Localisation.LoadData("561.txt");

        HeaderManager = new HeaderManager();
        HeaderManager.RootDirectory = @"Content\WorldData\Headers";
        HeaderManager.Load();

        TextArchiveManager = new TextArchiveManager();
        TextArchiveManager.RootDirectory = @"Content\Localisation\de";
        TextArchiveManager.Load(561);
        
        Building.RootDirectory = @"A:\ModelExporter\Platin\export_output\output_assets";
        Chunk.RootDirectory = @"A:\ModelExporter\Platin\overworldmaps";
        
        _camera = new Camera(GraphicsDevice);

        _world = World.Load(GraphicsDevice, _matrix);
        
        _animations.Add(new TextureAnimation(Services, "Content/Animations/Lakep", "lakep", 0.32f, 4, ["lakep_lm"]));
        _animations.Add(new TextureAnimation(Services, "Content/Animations/C1_Lamp1", "c1_lamp01", 0.16f, 5, ["c1_lamp01_", "lamp01"], AnimationPlayMode.Bounce, 0.64f));
        _animations.Add(new TextureAnimation(Services, "Content/Animations/C1_Lamp2", "c1_lamp02", 0.16f, 5, ["c1_lamp02_", "lamp03"], AnimationPlayMode.Bounce, 0.64f));
        _animations.Add(new TextureAnimation(Services, "Content/Animations/C1_S02_3", "c1_s02_3", 0.32f, 4, ["c1_s02_4"]));
        _animations.Add(new TextureAnimation(Services, "Content/Animations/C1_S01_D", "c1_s01_d", 0.08f, 4, ["c1_s01_d"]));
        _animations.Add(new TextureAnimation(Services, "Content/Animations/Hamabe", "hamabe", 0.32f, 8, ["hamabe_lm"]));
        _animations.Add(new TextureAnimation(Services, "Content/Animations/SeaRock", "searock", 0.16f, 4, ["searock_"]));
        _animations.Add(new TextureAnimation(Services, "Content/Animations/Sea", "sea", 0.32f, 8, ["sea_"]));
        
        var vertices = new VertexPositionTexture[4];
        
        vertices[0].Position = new Vector3(-0.5f, -0.5f, 0f);
        vertices[1].Position = new Vector3( 0.5f, -0.5f, 0f);
        vertices[2].Position = new Vector3( 0.5f,  0.5f, 0f);
        vertices[3].Position = new Vector3(-0.5f,  0.5f, 0f);

        vertices[0].TextureCoordinate = new Vector2(0, 1);
        vertices[1].TextureCoordinate = new Vector2(1, 1);
        vertices[2].TextureCoordinate = new Vector2(1, 0);
        vertices[3].TextureCoordinate = new Vector2(0, 0);
        
        var indices = new short[] { 0, 1, 2, 0, 3, 2 };

        _vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), vertices.Length, BufferUsage.WriteOnly);
        _vertexBuffer.SetData(vertices);
        
        _indexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), indices.Length, BufferUsage.WriteOnly);
        _indexBuffer.SetData(indices);

        
        base.Initialize();
    }
        
    protected override void LoadContent()
    {
        // ReSharper disable once HeapView.ObjectAllocation.Evident
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _worldShader = Content.Load<Effect>("Shaders/WorldShader");
        _buildingShader = Content.Load<Effect>("Shaders/BuildingShader");
        _basicEffect = new AlphaTestEffect(GraphicsDevice);
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _imageFont = ImageFont.Load(GraphicsDevice, @"Content/Font.json");
        _fontRenderer = new ImageFontRenderer(GraphicsDevice, _spriteBatch, _imageFont);

        _textBox = Content.Load<Texture2D>("textbox");

        var woman1SpriteCollection = new SpriteCollection(Services);
        woman1SpriteCollection.Load("Content/Sprites/woman1", "woman1", 16);
        
        var man2SpriteCollection = new SpriteCollection(Services);
        man2SpriteCollection.Load("Content/Sprites/man2", "man2", 16);
        
        var rivalSpriteCollection = new SpriteCollection(Services);
        rivalSpriteCollection.Load("Content/Sprites/rival", "rivel", 16);
        
        AppContext.OverWorldSprites.Add(12, woman1SpriteCollection);
        AppContext.OverWorldSprites.Add(10, man2SpriteCollection);
        AppContext.OverWorldSprites.Add(148, rivalSpriteCollection);

        var songJson = File.ReadAllText($@"Content/SoundData.json");
        var jSongArray = JArray.Parse(songJson);

        foreach (var jSong in jSongArray)
        {
            try
            {
                var songIdToken = jSong["id"];
                if (songIdToken == null)
                {
                    throw new Exception();
                }
                var songId = songIdToken.Value<int>();
                    
                var songNameToken = jSong["name"];
                if (songNameToken == null)
                {
                    throw new Exception();
                }
                var songName = songNameToken.ToString();
                    
                var music = new Music(songId, $@"Audio\Songs\{songName}");
                    
                var songLoopToken = jSong["loop"];
                if (songLoopToken != null)
                {
                    var value = songLoopToken.Value<float>();
                    music.LoopStart = TimeSpan.FromSeconds(value);
                }
                    
                var songEndToken = jSong["end"];
                if (songEndToken != null)
                {
                    var value = songEndToken.Value<float>();
                    music.End = TimeSpan.FromSeconds(value);
                }
                    
                music.LoadContent(Content);
                _musics.Add(songId, music);
            }
            catch (Exception e)
            {
                // ignored
            }
        }
            
        foreach (var animation in _animations)
        {
            animation.Load();
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
            MediaPlayer.Pause();
            return;
        }

        if (MediaPlayer.State == MediaState.Paused)
        {
            MediaPlayer.Resume();
        }
            
        _timeManager.Update(gameTime);
            
        foreach (var animation in _animations)
        {
            animation.Update(gameTime);
        }
            
        _worldShader.Parameters["TimeOfDay"]?.SetValue(2);
        _worldShader.Parameters["Delta"]?.SetValue(delta);
        _worldShader.Parameters["Total"]?.SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            
        _buildingShader.Parameters["TimeOfDay"]?.SetValue(2);
        _buildingShader.Parameters["Delta"]?.SetValue(delta);
        _buildingShader.Parameters["Total"]?.SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            
        KeyboardHandler.Update(gameTime);

        if (KeyboardHandler.IsKeyDownOnce(Keys.T))
        {
            _debugTexture = true;
            Console.WriteLine("--------------------");
        }

        if (KeyboardHandler.IsKeyDownOnce(Keys.P))
        {
            Localisation.Reload();
            TextArchiveManager.Dispose();
            TextArchiveManager.Load(561);
            TextArchiveManager.Load(412);
        }

        if (KeyboardHandler.IsKeyDownOnce(Keys.PageDown))
        {
            _matrix = Math.Max(_matrix - 1, 0);
        }

        if (KeyboardHandler.IsKeyDownOnce(Keys.PageUp))
        {
            _matrix = Math.Min(_matrix + 1, 288);
        }

        if (KeyboardHandler.IsKeyDownOnce(Keys.Space))
        {
            _world = World.Load(GraphicsDevice, _matrix);
        }
/*
            var direction = GetDirectionFromInput();

            if (direction != Vector3.Zero)
            {
                direction.Normalize();
                var stepSize = 0.1f;

                CellTarget = new Vector2(_heroX, _heroY) + new Vector2(direction.X, direction.Z) * stepSize;
                _heroHeight += direction.Y * stepSize;


                var newHeroPosition = new Vector3(CellTarget.X, 0, CellTarget.Y);

                var newChunkX = (int)newHeroPosition.X / Chunk.Wx;
                var newChunkY = (int)newHeroPosition.Z / Chunk.Wy;

                var chunk = _world.GetChunkAtPosition(newHeroPosition);

                if (chunk != null)
                {
                    var collision = _world.CheckTileCollision(newHeroPosition);
                    var type = _world.CheckTileType(newHeroPosition);

                    Console.WriteLine(collision);

                    if (collision == 0x800)
                    {
                        if (type != 0x00)
                        {
                            Console.WriteLine(type);
                            Console.WriteLine(direction);
                            if (type == (byte)ChunkTileType.JumpDown)
                            {
                                if (direction == Vector3.Backward)
                                {
                                    _heroX = CellTarget.X;
                                    _heroY = CellTarget.Y + 1;
                                    _chunkX = newChunkX;
                                    _chunkY = newChunkY;
                                    _cellX = (int)(_heroX % 32) / 16;
                                    _cellY = (int)(_heroY % 32) / 16;
                                }
                            }
                            else if (type == (byte)ChunkTileType.TV)
                            {

                            }
                            else
                            {
                                _heroX = CellTarget.X;
                                _heroY = CellTarget.Y;
                                _chunkX = newChunkX;
                                _chunkY = newChunkY;
                                _cellX = (int)(_heroX % 32) / 16;
                                _cellY = (int)(_heroY % 32) / 16;
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
                            _heroX = _chunkX * 32;
                            _heroX += _cellX * 16;

                            _heroY = _chunkY * 32;
                            _heroY += _cellY * 16;
                        }
                        else
                        {
                            _heroX = CellTarget.X;
                            _heroY = CellTarget.Y;
                            _chunkX = newChunkX;
                            _chunkY = newChunkY;
                            _cellX = (int)(_heroX % 32) / 16;
                            _cellY = (int)(_heroY % 32) / 16;
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
*/
        _frameCount += delta;
        UpdateCamera(gameTime);

        foreach (var chunk in World.Chunks)
        {
            foreach (var building in chunk.Value.Buildings)
            {
                building.Model.Update(gameTime);
                building.Model.Play(0);
            }
        }

        base.Update(gameTime);
    }

    private void UpdateCamera(GameTime gameTime)
    {
        var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
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
        _camera.Update(gameTime);
    }

    private void DrawSprite(GameTime gameTime, AlphaTestEffect effect, Vector3 position, Vector3 scale, Vector3 rotation, Texture2D texture)
    {
        
        var worldMatrix = Matrix.CreateScale(scale) *
                          Matrix.CreateRotationX(rotation.X) *
                          Matrix.CreateRotationY(rotation.Y) *
                          Matrix.CreateTranslation(position);

        effect.World = worldMatrix;
        effect.View = _camera.View;
        effect.Projection = _camera.Projection;
        effect.AlphaFunction = CompareFunction.Greater;
        effect.ReferenceAlpha = 0;
        effect.Texture = texture;
            
        //effect.Parameters["World"].SetValue(worldMatrix);
        //effect.Parameters["View"].SetValue(_camera.View);
        //effect.Parameters["Projection"].SetValue(_camera.Projection);

        GraphicsDevice.SetVertexBuffer(_vertexBuffer);
        GraphicsDevice.Indices = _indexBuffer;
        
        // Draw the sprite with indices
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, 4);
        }
    }
    
    protected override void Draw(GameTime gameTime)
    {
        if (!IsActive)
        {
            return;
        }
        GraphicsDevice.Clear(Color.Black);
            
        DrawWorld(gameTime, _world);
        
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_textBox, Vector2.Zero, Color.White);
        try
        {
            var text = TextArchiveManager.GetLine(1, (int)(gameTime.TotalGameTime.TotalSeconds % Localisation.Count));
            text = text.Replace("\\r", "");
            text = text.Replace("\\f", "");
            var lines = text.Split("\\n");
            for (var i = 0; i < lines.Length; i++)
            {
                _fontRenderer.DrawText(lines[i], new Vector2(14, 10 + _imageFont.LineHeight * i));
            }
        }
        catch (Exception ex)
        {
            // ignored
        }

        _spriteBatch.End();

        if (_debugTexture)
        {
            _debugTexture = false;
        }

        base.Draw(gameTime);
    }

    private void DrawWorld(GameTime gameTime, World world)
    {
        foreach (var chunkEntry in world.Combination)
        {
            var (targetX, targetY) = chunkEntry.Key;
            var tuple = chunkEntry.Value;

            var chunkId = tuple.chunkId;
            var headerId = tuple.headerId;
            
            if (headerId != 0)
            {
                var header = HeaderManager.GetHeaderById(headerId);

                try
                {
                        
                    //Console.WriteLine(header.LocationName + " : " + headerId);
                    var eventFile = EventContainerLoader.Load($@"Content\Events\{header.EventFileId}.json");
                    if (eventFile != null)
                    {
                        foreach (var entity in eventFile.Overworlds)
                        {
                            var sprite = 12;
                            if (AppContext.OverWorldSprites.ContainsKey(entity.EntryId))
                            {
                                sprite = entity.EntryId;
                            }

                            var frameIndex = (int)(_frameCount % 16);

                            if (AppContext.OverWorldSprites[sprite].Has(frameIndex))
                            {

                                var worldPosition = new Vector3(entity.MatrixX, 0, entity.MatrixY) * 32;
                                var chunkPosition = new Vector3(entity.ChunkX, entity.ChunkZ + 1, entity.ChunkY);
                                worldPosition += chunkPosition;
                                var position = PatteLib.Utils.WorldToScreen(worldPosition, _camera.View,
                                    _camera.Projection,
                                    GraphicsDevice.Viewport);
                                DrawSprite(gameTime, _basicEffect, worldPosition + new Vector3(0.5f, 0, 0.5f),
                                    new Vector3(1, 1, 1) * 2, _camera.Rotation,
                                    AppContext.OverWorldSprites[sprite].Get(frameIndex));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //ignore
                }
            }
            
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

        foreach (var chunkEntry in world.Combination)
        {
            var (targetX, targetY) = chunkEntry.Key;
            var tuple = chunkEntry.Value;

            var chunkId = tuple.chunkId;
            var headerId = tuple.headerId;
            var worldOffset = new Vector3(targetX * 32, tuple.height / 2f, targetY * 32) + new Vector3(16, 0, 16);
            
            
            if (World.Chunks.TryGetValue(chunkId, out var chunk))
            {
                if (chunk.IsLoaded && chunk.Model != null)
                {
                    DrawModel(gameTime, _worldShader, chunk.Model,
                        offset: worldOffset, alpha: true);
                    foreach (var building in chunk.Buildings)
                    {
                        DrawModel(gameTime, _buildingShader, building.Model,
                            offset: worldOffset, alpha: true);
                    }
                }
            }
        }
    }

    private void DrawWorldSmart(GameTime gameTime, World world)
    {
        /*
        int[] offsetX = [-2, -1, 0, 1, 2];
        int[] offsetY = [-2, -1, 0, 1, 2];

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

                var worldOffset = new Vector3(targetX * 32, tuple.height / 2f, targetY * 32) + new Vector3(16, 0, 16);

                if (World.Chunks.TryGetValue(chunkId, out var chunk))// && _world.Headers.TryGetValue(headerId, out var header))
                {
                    if (chunk.IsLoaded && chunk.Model != null)
                    {
                        var buildingsWithDistance = new List<(Building building, float Distance)>();
                        DrawModel(gameTime, _worldShader, chunk.Model,
                            offset: worldOffset, alpha: false);
                        foreach (var building in chunk.Buildings)
                        {
                            DrawModel(gameTime, _buildingShader, building.Model, offset: worldOffset, alpha: false);
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

                    var worldOffset = new Vector3(targetX * 32, tuple.height / 2f, targetY * 32) + new Vector3(16, 0, 16);

                    if (World.Chunks.TryGetValue(chunkId, out var chunk))// && _world.Headers.TryGetValue(headerId, out var header))
                    {
                        if (chunk.IsLoaded && chunk.Model != null)
                        {
                            DrawModel(gameTime, _worldShader, chunk.Model,
                                offset: worldOffset, alpha: true);
                            foreach (var building in chunk.Buildings)
                            {
                                DrawModel(gameTime, _buildingShader, building.Model, offset: worldOffset, alpha: true);

                                var screenPos = PatteLib.Utils.WorldToScreen(worldOffset + building.Position, _camera.View, _camera.Projection, GraphicsDevice.Viewport);

                                if (KeyboardHandler.IsKeyDown(Keys.J))
                                {
                                    _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                                    _spriteBatch.Draw(_pixel, new Rectangle(screenPos.ToPoint(), _imageFont.MeasureString(building.BuildingName, 2)), Color.White * 0.5f);
                                    _fontRenderer.DrawText("§0Hallo §rHallo §cPokedex", screenPos);
                                    //_spriteBatch.DrawString(_font, building.BuildingName, screenPos, Color.White);
                                    _spriteBatch.End();
                                }
                            }
                        }
                    }
                }
            }
        }*/
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
                if (skin.JointMatrices.Length > 180)
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

        //Todo: Fix Sorting
   
        //var primitivesWithDistance = new List<(GameMeshPrimitives Primitive, float Distance)>();
        //foreach (var primitive in mesh.Primitives)
        //{
        //    var distance = Vector3.Distance(new Vector3(_camera.Position.X, _camera.Position.Y, _camera.Position.Z), primitive.LocalPosition);
        //    primitivesWithDistance.Add((primitive, distance));
        //}
        //var sortedPrimitives = primitivesWithDistance.OrderByDescending(p => p.Distance)
        //    .Select(p => p.Primitive).ToList();

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
            effect.Parameters["AdditionalColorFactor"]?.SetValue(Color.White.ToVector4());
            effect.Parameters["AlphaCutoff"]?.SetValue(material.AlphaCutoff);

            if (material.HasTexture)
            {
                effect.Parameters["TextureEnabled"]?.SetValue(true);
                effect.Parameters["TextureDimensions"]?.SetValue(material.BaseTexture.Texture.Bounds.Size.ToVector2());
                effect.Parameters["Texture"]?.SetValue(material.BaseTexture.Texture);
                effect.Parameters["TextureAnimation"]?.SetValue(false);
                SetTextureAnimation(gameTime, material, effect);
                SetTextureEffects(gameTime, material, effect);

                GraphicsDevice.SamplerStates[0] = material.BaseTexture.Sampler.SamplerState;
            }
        }
    }

    private void SetTextureEffects(GameTime gameTime, GameMaterial material, Effect effect)
    {
        if (material.Name.Contains("window") || material.Name.Contains("h_mado"))
        {
            effect.Parameters["AdditionalColorFactor"]?.SetValue(Color.Yellow.ToVector4());
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
            effect.Parameters["AnimationDirection"]?.SetValue((byte)TextureAnimationDirection.Left);
            effect.Parameters["TextureAnimation"]?.SetValue(true);
        }
        else if (material.Name.Contains("l_lake"))
        {
            effect.Parameters["AnimationSpeed"]?.SetValue(16);
            effect.Parameters["AnimationDirection"]?.SetValue((byte)TextureAnimationDirection.DownLeft);
            effect.Parameters["TextureAnimation"]?.SetValue(true);
        }
        else if (material.Name.Contains("pool_W"))
        {
            effect.Parameters["AnimationSpeed"]?.SetValue(1);
            effect.Parameters["AnimationDirection"]?.SetValue((byte)TextureAnimationDirection.DownLeft);
            effect.Parameters["TextureAnimation"]?.SetValue(true);
        }
        else if (material.Name.Contains("neon0"))
        {
            effect.Parameters["AnimationSpeed"]?.SetValue(1);
            effect.Parameters["AnimationDirection"]?.SetValue((byte)TextureAnimationDirection.DownLeft);
            effect.Parameters["TextureAnimation"]?.SetValue(true);
        }
        else if (material.Name.Contains("leag_yuka03"))
        {
            effect.Parameters["AnimationSpeed"]?.SetValue(42);
            effect.Parameters["AnimationDirection"]?.SetValue((byte)TextureAnimationDirection.Up);
            effect.Parameters["TextureAnimation"]?.SetValue(true);
        }
    }
}