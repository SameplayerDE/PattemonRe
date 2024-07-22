using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HxGLTF;
using HxGLTF.Implementation;
using HxLocal;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json.Linq;
using PatteLib;
using PatteLib.Data;
using PatteLib.Graphics;
using PatteLib.World;
using TestRender.Graphics;

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
    private Effect _animationShader;
    
    private Camera _camera;
    private Camera _normalCamera;
    
    private World _world;
    private WorldTimeManager _timeManager;
    private Texture2D _pixel;
    private ImageFont _imageFont;
    private ImageFontRenderer _fontRenderer;
    private RenderTarget2D _alphaPassTarget;
    private RenderTarget2D _defaultPassTarget;

    private Texture2D _textBox;

    private Dictionary<int, Texture2D> _sprites = [];
    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    
    private Dictionary<string[], AdvTextureAnimation> _animations = [];
    private Dictionary<int, SoundEffect> _soundEffects = [];
    private Dictionary<int, Music> _musics = [];
    private SoundEffectInstance _currentSoundEffectInstance;

    private Point _preferredDimensions = new Point(1280, 980);
    private int _matrix = 411;
    private bool _debugTexture = false;

    public Game1()
    {
        _graphicsDeviceManager = new GraphicsDeviceManager(this);
        Content.RootDirectory = @"Content";
        IsMouseVisible = true;
            
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1d / 120d);

        _graphicsDeviceManager.PreferredBackBufferHeight = 960;
        _graphicsDeviceManager.PreferredBackBufferWidth = 1280;
            
        _graphicsDeviceManager.ApplyChanges();

        MediaPlayer.Volume = 0.3f;
    }

    protected override void Initialize()
    {
        _timeManager = new WorldTimeManager();
        
        _alphaPassTarget = new RenderTarget2D(GraphicsDevice, _preferredDimensions.X, _preferredDimensions.Y, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24Stencil8);
        _defaultPassTarget = new RenderTarget2D(GraphicsDevice, _preferredDimensions.X, _preferredDimensions.Y, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24Stencil8);
        
        Localisation.RootDirectory = @"Content\Localisation";
        Localisation.SetLanguage("de");
        Localisation.LoadData("561.txt");

        HeaderManager = new HeaderManager();
        HeaderManager.RootDirectory = @"Content\WorldData\Headers";
        HeaderManager.Load();

        TextArchiveManager = new TextArchiveManager();
        TextArchiveManager.RootDirectory = @"Content\Localisation\de";
        TextArchiveManager.Load(561);
        
        Building.RootDirectory = @"A:\ModelExporter\Platin\output_assets";
        Chunk.RootDirectory = @"A:\ModelExporter\Platin\overworldmaps";
        
        _camera = new Camera();
        _camera.InitWithPosition(Vector3.One, (float)NitroUtils.Fx32ToDecimal(2731713), Vector3.Zero, NitroUtils.GetAngleFromU16Int(1473));
        _camera.SetRotation(new Vector3(NitroUtils.GetAngleFromU16Int(54786), 0, 0));
        _camera.SetClipping(0.01f, 1000f);
        _camera.SetAsActive();
        
        _normalCamera = new Camera();
        _normalCamera.InitWithPosition(Vector3.One, 10, Vector3.Zero, 75);
        _normalCamera.SetClipping(0.01f, 1000f);

        _world = World.LoadByHeader(GraphicsDevice, _matrix);
        
        _animations.Add(["l_lake"], AdvTextureAnimation.Load(GraphicsDevice, "Content/sea_animation.json"));
        _animations.Add(["c1_fun2", "taki"], AdvTextureAnimation.Load(GraphicsDevice, "Content/funsui_animation.json"));
        _animations.Add(["neon0"], AdvTextureAnimation.Load(GraphicsDevice, "Content/party_animation.json"));
        
        var vertices = new VertexPositionTexture[4];
        
        vertices[0].Position = new Vector3(-0.5f, 0f, 0f);
        vertices[1].Position = new Vector3( 0.5f, 0f, 0f);
        vertices[2].Position = new Vector3( 0.5f, 1f, 0f);
        vertices[3].Position = new Vector3(-0.5f, 1f, 0f);

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
        var texAnim = AdvTextureAnimation.Load(GraphicsDevice, "Content/sea_animation.json"); 
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _worldShader = Content.Load<Effect>("Shaders/WorldShader");
        _buildingShader = Content.Load<Effect>("Shaders/BuildingShader");
        _animationShader = Content.Load<Effect>("Shaders/AnimationShader");
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
        
        var pokeballSpriteCollection = new SpriteCollection(Services);
        pokeballSpriteCollection.Load("Content/Sprites/monstarball", "monstarball", 1);
        
        var missingCollection = new SpriteCollection(Services);
        missingCollection.Load("Content/Sprites/missing", "missing", 1);
        
        AppContext.OverWorldSprites.Add(12, woman1SpriteCollection);
        AppContext.OverWorldSprites.Add(10, man2SpriteCollection);
        AppContext.OverWorldSprites.Add(87, pokeballSpriteCollection);
        AppContext.OverWorldSprites.Add(148, rivalSpriteCollection);
        AppContext.OverWorldSprites.Add(-1, missingCollection);
        
        AppContext.OverWorldModels.Add(91, GameModel.From(GraphicsDevice, GLTFLoader.Load(@"A:\ModelExporter\Platin\output_assets\board_a\board_a")));
        AppContext.OverWorldModels.Add(92, GameModel.From(GraphicsDevice, GLTFLoader.Load(@"A:\ModelExporter\Platin\output_assets\board_b\board_b")));
        AppContext.OverWorldModels.Add(93, GameModel.From(GraphicsDevice, GLTFLoader.Load(@"A:\ModelExporter\Platin\output_assets\board_c\board_c")));
        AppContext.OverWorldModels.Add(94, GameModel.From(GraphicsDevice, GLTFLoader.Load(@"A:\ModelExporter\Platin\output_assets\board_d\board_d")));
        AppContext.OverWorldModels.Add(95, GameModel.From(GraphicsDevice, GLTFLoader.Load(@"A:\ModelExporter\Platin\output_assets\board_e\board_e")));
        AppContext.OverWorldModels.Add(96, GameModel.From(GraphicsDevice, GLTFLoader.Load(@"A:\ModelExporter\Platin\output_assets\board_f\board_f")));

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
            //animation.Load();
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
            
        foreach (var animation in _animations.Values)
        {
            animation.Update(gameTime);
        }
            
        _worldShader.Parameters["TimeOfDay"]?.SetValue(2);
        _worldShader.Parameters["Delta"]?.SetValue(delta);
        _worldShader.Parameters["Total"]?.SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            
        _buildingShader.Parameters["TimeOfDay"]?.SetValue(2);
        _buildingShader.Parameters["Delta"]?.SetValue(delta);
        _buildingShader.Parameters["Total"]?.SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            
        _animationShader.Parameters["TimeOfDay"]?.SetValue(2);
        _animationShader.Parameters["Delta"]?.SetValue(delta);
        _animationShader.Parameters["Total"]?.SetValue((float)gameTime.TotalGameTime.TotalSeconds);

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
            _world = World.LoadByHeader(GraphicsDevice, _matrix);
        }

       // _camera.SetAsActive();
       // UpdateCamera(gameTime);
       //Camera.ClearActive();
       //
       _normalCamera.SetAsActive();
       UpdateCamera(gameTime);
       //Camera.ClearActive();

        foreach (var chunk in World.Chunks)
        {
            foreach (var building in chunk.Value.Buildings)
            {
                building.Model.Update(gameTime);
                building.Model.Play(0);
                //building.Model.Stop();
            }
        }

        base.Update(gameTime);
    }

    private void UpdateCamera(GameTime gameTime)
    {
        if (Camera.ActiveCamera == null)
        {
            return;
        }
        
        var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Vector3 Direction = new Vector3();
        float turnSpeed = 1;

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
            Direction += Vector3.Down;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.E))
        {
            Direction += Vector3.Up;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
        {
            Direction *= 64;
        }
        else
        {
            Direction *= 4;
        }

        Camera.ActiveCamera.MoveAlongRotation(Direction * delta);
        
        turnSpeed *= 64;
        turnSpeed = MathHelper.ToRadians(turnSpeed * delta);
        
        if (Keyboard.GetState().IsKeyDown(Keys.Up))
        {
            Camera.ActiveCamera.AdjustRotation(new Vector3(turnSpeed, 0, 0));
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Down))
        {
            Camera.ActiveCamera.AdjustRotation(new Vector3(-turnSpeed, 0, 0));
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Left))
        {
            Camera.ActiveCamera.AdjustRotation(new Vector3(0, turnSpeed, 0));
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Right))
        {
            Camera.ActiveCamera.AdjustRotation(new Vector3(0, -turnSpeed, 0));
        }
        
        //_camera.Update(gameTime);
        Camera.ActiveCamera.ComputeViewMatrix();
    }

    private void DrawSprite(GameTime gameTime, AlphaTestEffect effect, Vector3 position, Vector3 scale, Vector3 rotation, Texture2D texture)
    {
        
        var worldMatrix = Matrix.CreateScale(scale) *
                          Matrix.CreateRotationX(rotation.X) *
                          Matrix.CreateRotationY(rotation.Y) *
                          Matrix.CreateTranslation(position);

        effect.World = worldMatrix;
        effect.View = _camera.ViewMatrix;
        effect.Projection = _camera.ProjectionMatrix;
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
        if (!IsActive || Camera.ActiveCamera == null)
        {
            return;
        }

        _camera.SetAsActive();
        GraphicsDevice.SetRenderTarget(_defaultPassTarget);
        GraphicsDevice.Clear(Color.Transparent);
        DrawWorld(gameTime, _world);
        
        _normalCamera.SetAsActive();
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);
        DrawWorld(gameTime, _world);
        
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, depthStencilState: DepthStencilState.Default, blendState: BlendState.AlphaBlend);
        _spriteBatch.Draw(_defaultPassTarget, new Rectangle(0, 0, 1280 / 5, 960 / 5), Color.White);
        //_spriteBatch.Draw(_textBox, Vector2.Zero, Color.White);
        
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

    private void DrawWorld(GameTime gameTime, World world, bool alphaPass = false)
    {
        foreach (var chunkEntry in world.Combination)
        {
            var (targetX, targetY) = chunkEntry.Key;
            var tuple = chunkEntry.Value;

            var chunkId = tuple.chunkId;
            var headerId = tuple.headerId;

            var distance = Vector2.Distance(new Vector2(targetX, targetY), (new Vector2(_camera.Position.X, _camera.Position.Z) / 32).ToPoint().ToVector2());
            
            if (World.Chunks.TryGetValue(chunkId, out var chunk))
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
        
        foreach (var chunkEntry in world.Combination)
        {
            var (targetX, targetY) = chunkEntry.Key;
            var tuple = chunkEntry.Value;

            var chunkId = tuple.chunkId;
            var headerId = tuple.headerId;

            var distance = Vector2.Distance(new Vector2(targetX, targetY), (new Vector2(_camera.Position.X, _camera.Position.Z) / 32).ToPoint().ToVector2());

            if (headerId == -1)
            {
                headerId = AppContext.CurrentHeaderId;
            }
            
            if (headerId > 0 && distance < 1)
            {
                var header = HeaderManager.GetHeaderById(headerId);
                if (distance < 1)
                {
                    //Console.WriteLine(("Distance to " + new Vector2(targetX, targetY) + " : " + distance));
                    //Console.WriteLine(header.LocationName + " : " + headerId);
                }
                try
                {
                    
                    //var eventFile = EventContainerLoader.Load($@"A:\ModelExporter\Platin\event_files\{header.EventFileId}.json");
                    var eventFile = AppContext.CurrentEventContainer;
                    if (eventFile != null)
                    {
                        foreach (var entity in eventFile.Overworlds)
                        {

                            if (entity.Script == 0xFFFF)
                            {
                                //continue;
                            }
                            else
                            {
                                if (entity.Flag != 0)
                                {
                                    if (MemoryContext.GetFlag(entity.Flag) != 1)
                                    {
                                        continue;
                                    }
                                }
                            }

                            var worldPosition = new Vector3(entity.MatrixX, 0, entity.MatrixY) * 32;
                            var chunkPosition = new Vector3(entity.ChunkX, entity.ChunkZ, entity.ChunkY);

                            var chunkPlate = world.GetChunkAtPosition(worldPosition)
                                .GetNearestChunkPlate(chunkPosition);
                            var height = 0f;
                            if (chunkPlate != null)
                            {
                                height = chunkPlate.GetHeightAt(entity.ChunkX, entity.ChunkY);
                            }
                            else
                            {
                                height = 1;
                            }
                            chunkPosition.Y = height;
                            worldPosition += chunkPosition;
                            
                            if (entity.Is3D)
                            {
                                var model = 92;
                                if (AppContext.OverWorldModels.ContainsKey(entity.EntryId))
                                {
                                    model = entity.EntryId;
                                }

                                DrawModel(gameTime, _buildingShader, AppContext.OverWorldModels[model], false,
                                    offset: worldPosition - new Vector3(-0.5f, 1, -0.5f));
                                DrawModel(gameTime, _buildingShader, AppContext.OverWorldModels[model], true,
                                    offset: worldPosition - new Vector3(-0.5f, 1, -0.5f));
                            }
                            else
                            {

                                var sprite = -1;
                                if (AppContext.OverWorldSprites.ContainsKey(entity.EntryId))
                                {
                                    sprite = entity.EntryId;
                                }

                                var frameIndex = (int)(gameTime.TotalGameTime.TotalSeconds % 16);
                                var collection = AppContext.OverWorldSprites[sprite];

                                frameIndex %= collection.Count;

                                if (collection.Has(frameIndex))
                                {
                                    var texture = collection.Get(frameIndex);
                                    var scale = texture.Width / 16;

                                   
                                    var position = PatteLib.Utils.WorldToScreen(worldPosition, _camera.ViewMatrix,
                                        _camera.ProjectionMatrix,
                                        GraphicsDevice.Viewport);
                                    DrawSprite(gameTime, _basicEffect, worldPosition + new Vector3(0.5f, -1, 0.5f),
                                        new Vector3(1, 1, 1) * scale, _camera.Rotation, texture);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            
        }
        
        foreach (var chunkEntry in world.Combination)
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
        }
        */
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
        effect.Parameters["View"].SetValue(Camera.ActiveCamera.ViewMatrix);
        effect.Parameters["Projection"].SetValue(Camera.ActiveCamera.ProjectionMatrix);

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
                effect.Parameters["ShouldAnimate"]?.SetValue(false);
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
        foreach (var animationPair in _animations)
        {
            if (animationPair.Key.Contains(material.Name))
            {
                var animation = animationPair.Value;

                if (animation.Type == AnimationType.Texture)
                {
                    return;
                }
                
                effect.Parameters["ShouldAnimate"]?.SetValue(true);
                effect.Parameters["Offset"]?.SetValue(animation.Offset);
            }
        }
    }
}