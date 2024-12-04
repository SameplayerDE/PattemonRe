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
    
    private bool _blank = false;
    
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
    private Texture2D _map;
    private ImageFont _imageFont;
    private ImageFontRenderer _fontRenderer;
    
    private RenderTarget2D _alphaPassTarget;
    private RenderTarget2D _defaultPassTarget;

    private RenderTarget2D _depthRenderTarget;
    private RenderTarget2D _normalRenderTarget;
    private RenderTarget2D _colorRenderTarget;

    private Texture2D _textBox;

    private Dictionary<int, Texture2D> _sprites = [];
    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    
    private Dictionary<(string[] Materials, AnimationCompareFunction CompareFunction), TextureAnimation> _animations = [];
    private Dictionary<int, SoundEffect> _soundEffects = [];
    private Dictionary<int, Music> _musics = [];
    private SoundEffectInstance _currentSoundEffectInstance;

    private Point _preferredDimensions = new Point(1280, 980);
    private int _matrix = 411;
    private bool _debugTexture = false;

    private Vector3 _target;

    public Game1()
    {
        _graphicsDeviceManager = new GraphicsDeviceManager(this);
        Content.RootDirectory = @"Content";
        IsMouseVisible = true;
            
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1d / 30d);

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
        
        _depthRenderTarget = new RenderTarget2D(
            GraphicsDevice,
            _preferredDimensions.X, _preferredDimensions.Y,
            false,
            SurfaceFormat.Single, // Für Tiefenwerte
            DepthFormat.Depth24
        );

        _normalRenderTarget = new RenderTarget2D(
            GraphicsDevice,
            _preferredDimensions.X, _preferredDimensions.Y,
            false,
            SurfaceFormat.Color,
            DepthFormat.None
        );

        _colorRenderTarget = new RenderTarget2D(
            GraphicsDevice,
            _preferredDimensions.X, _preferredDimensions.Y,
            false,
            SurfaceFormat.Color,
            DepthFormat.None
        );
        
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
        
        _normalCamera = new Camera();
        _normalCamera.InitWithPosition(Vector3.Zero, 10, Vector3.Zero, MathHelper.ToRadians(30), CameraProjectionType.Perspective);
        //_normalCamera.InitWithTarget(_target, 10, Vector3.Zero, MathHelper.ToRadians(75), CameraProjectionType.Perspective, true);
        _normalCamera.SetClipping(0.01f, 100000f);
        
        _camera = Camera.CameraLookMap[0];
        
        _world = World.LoadByHeader(GraphicsDevice, _matrix);

        LoadAnimations();
        
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

    private void LoadAnimations()
    {
        string path = "Content/Animations/animations.json";   
        JArray jAnimations = JArray.Parse(File.ReadAllText(path));

        foreach (var jAnimation in jAnimations)
        {
            
            IEnumerable<string> materialsEnumerable = JsonUtils.GetValues<string>(jAnimation["materials"]);
            string[] materials = new string[Enumerable.Count(materialsEnumerable)];
            int index = 0;
            foreach (var material in materialsEnumerable)
            {
                materials[index++] = material;
            }
            string compareFunctionString = JsonUtils.GetValue<string>(jAnimation["compareFunction"]);
            AnimationCompareFunction compareFunction = compareFunctionString switch
            {
                "equals" => AnimationCompareFunction.Equals,
                "contains" => AnimationCompareFunction.Contains,
                "startsWith" => AnimationCompareFunction.StartsWith,
                _ => throw new Exception()
            };
            
            string animationPathString = JsonUtils.GetValue<string>(jAnimation["animation"]);
            string combinedPath = Path.IsPathRooted(animationPathString) ? animationPathString : Path.Combine(Path.GetDirectoryName(path), animationPathString);
            
            _animations.Add((materials, compareFunction), TextureAnimation.Load(GraphicsDevice, combinedPath));
        } 
    }

    protected override void LoadContent()
    {
        GraphicsDevice.BlendState = BlendState.AlphaBlend;
        
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _worldShader = Content.Load<Effect>("Shaders/WorldShader");
        _buildingShader = Content.Load<Effect>("Shaders/BuildingShader");
        _animationShader = Content.Load<Effect>("Shaders/AnimationShader");
        _basicEffect = new AlphaTestEffect(GraphicsDevice);
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _map = Content.Load<Texture2D>("map");
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

    private int _currentMusicId = -1;
    private int _currentHeaderId = -1;
    public void UpdateMusic(Point prev, Point curr)
    {
        if (prev.X != curr.X || prev.Y != curr.Y)
        {
            int _chunkX = curr.X;
            int _chunkY = curr.Y;
            //_chunkX -= 1;
            //_chunkY -= 1;
            try
            {
                if (_world.Combination.TryGetValue((_chunkX, _chunkY), out var targetChunkTuple))
                {
                    if (World.Chunks.TryGetValue(targetChunkTuple.chunkId, out var targetChunk))
                    {
                        var header = HeaderManager.GetHeaderById(targetChunkTuple.headerId);
                        var soundId = _timeManager.CurrentPeriod.TimeOfDay == TimeOfDay.Day ? header.MusicDayId : header.MusicNightId;
                        Console.WriteLine(header.Id);
                        Console.WriteLine(_chunkX);
                        Console.WriteLine(_chunkY);
                        Console.WriteLine(header.LocationName);
                        // Überprüfe, ob sich die Musik-ID oder Header-ID geändert haben
                        if (header.Id != _currentHeaderId)
                        {
                            if (soundId != _currentMusicId)
                            {
                                //// Fadet die aktuelle Musik aus
                                //await FadeOutCurrentSoundEffectAsync();
//
                                //// Starte die neue Musik
                                //await PlayNewMusicAsync(soundId);

                                if (_musics.TryGetValue(soundId, out var music))
                                {
                                    music.Play();
                                }

                                // Aktualisiere die aktuellen IDs
                                _currentMusicId = soundId;
                                _currentHeaderId = header.Id;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // Fehlerbehandlung (optional)
            }
        }
    }
    
    protected override void Update(GameTime gameTime)
    {
        var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Console.Clear();
        Console.WriteLine(new Vector2(_camera.Position.X, _camera.Position.Z).ToString());
        UpdateMusic(Point.Zero, (new Vector2(_normalCamera.Position.X, _normalCamera.Position.Z) / 32).ToPoint());
        if (!IsActive)
        {
            MediaPlayer.Pause();
            return;
        }

        if (MediaPlayer.State == MediaState.Paused)
        {
            MediaPlayer.Resume();
        }
        
        //if (_world.GetChunkAtPosition(_camera.Position) != null)
        //{
        //    var chunk = _world.GetChunkAtPosition(_camera.Position);
        //    var header = HeaderManager.GetHeaderById(chunk.Id);
        //    _musics[header.MusicDayId].Play();
        //    //MediaPlayer.Play();
        //}
        
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

        //if (KeyboardHandler.IsKeyDownOnce(Keys.P))
        //{
        //    Localisation.Reload();
        //    TextArchiveManager.Dispose();
        //    TextArchiveManager.Load(561);
        //    TextArchiveManager.Load(412);
        //}

#if DEBUG
        if (KeyboardHandler.IsKeyDownOnce(Keys.PageDown))
        {
            _matrix = Math.Max(_matrix - 1, 0);
            //_camera.AdjustFieldOfView(MathHelper.ToRadians(-1));
        }

        if (KeyboardHandler.IsKeyDownOnce(Keys.PageUp))
        {
            _matrix = Math.Min(_matrix + 1, 288);
            //_camera.AdjustFieldOfView(MathHelper.ToRadians(1));
        }
        
        if (KeyboardHandler.IsKeyDownOnce(Keys.O))
        {
            _camera = Camera.CameraLookMap[4];
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.P))
        {
            _camera = Camera.CameraLookMap[0];
        }

        if (KeyboardHandler.IsKeyDownOnce(Keys.Space))
        {
            _world = World.LoadByHeader(GraphicsDevice, _matrix);
        }
#endif
        _camera.SetAsActive();
#if DEBUG
        _camera.CaptureTarget(ref _normalCamera.Position);
#else
        _camera.CaptureTarget(ref _target);
#endif
        _camera.ComputeViewMatrix();
#if DEBUG
        _normalCamera.SetAsActive();
        ControlActiveCamera(gameTime);
        _normalCamera.ComputeViewMatrix();
#else
        if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
        {
            _target += GetDirectionFromInput() * World.ChunkWx * delta;
        }
        else
        {
            _target += GetDirectionFromInput() * World.ChunkWx / 8 * delta;
        }
        var result = _world.GetChunkPlateUnderPosition(_target);
        if (result.Length >= 1)
        {
            var plate = result[0];

            // Get camera position relative to chunk top-left corner
            var localX = (_target.X % World.ChunkWx) - plate.X;
            var localZ = (_target.Z % World.ChunkWy) - plate.Y;

            // Get height at camera position considering angles
            var height = plate.GetHeightAt(localX, localZ);

            if (height >= 0)
            {
                Console.WriteLine($"Height under camera: {height}");
                _target += (new Vector3(_camera.Position.X, height, _camera.Position.Z));
            }
            else
            {
                Console.WriteLine("Camera position is outside the ChunkPlate");
            }
        }
#endif

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

    private void ControlActiveCamera(GameTime gameTime)
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
            Direction *= World.ChunkWx;
        }
        else
        {
            Direction *= World.ChunkWx / 8;
        }

        Camera.ActiveCamera.MoveAlongRotation(Direction * delta);
        
        turnSpeed *= MathHelper.ToRadians(64);
        turnSpeed *= delta;
        
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
    }
    
    protected override void Draw(GameTime gameTime)
    {
        if (!IsActive || Camera.ActiveCamera == null)
        {
            return;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Space))
        {
            _camera.SetAsActive();
            GraphicsDevice.SetRenderTarget(_defaultPassTarget);
            GraphicsDevice.Clear(Color.Black);
            DrawWorldSmart(gameTime, _world, _camera.Target);
            GraphicsDevice.SetRenderTarget(null);
        }
#if DEBUG
        _normalCamera.SetAsActive();
        GraphicsDevice.Clear(new Color(34, 42, 53, 255));
        DrawWorldSmart(gameTime, _world, _normalCamera.Position, 5);
#endif
        
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, depthStencilState: DepthStencilState.Default, blendState: BlendState.Opaque);
#if RELEASE
        _spriteBatch.Draw(_defaultPassTarget, GraphicsDevice.Viewport.Bounds, Color.White);
#else
        if (Keyboard.GetState().IsKeyDown(Keys.Space))
        {
            _spriteBatch.Draw(_defaultPassTarget, new Rectangle(0, 0, 256 * 2, 192 * 2), Color.White);
        }
#endif
        try
        {
            var text = "position: " + _normalCamera.Position + "\\nrotation: " + _normalCamera.Rotation + "\\nfieldofview: " + MathHelper.ToDegrees(_normalCamera.FieldOfViewY) + "\\n\\ntimeOfDay: " + _timeManager.CurrentPeriod.TimeOfDay.ToString() + "\\nlocation: " + HeaderManager.GetHeaderById(_currentHeaderId)?.LocationName;
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
            using var file = File.Create($"output_{DateTime.Now:HH_mm_ss_d_M}.jpeg");
            _defaultPassTarget.SaveAsJpeg(file, _defaultPassTarget.Width, _defaultPassTarget.Height);
        }
        
        base.Draw(gameTime);
    }

    private void DrawWorldSmart(GameTime gameTime, World world, Vector3 target, int renderDistance = 1)
    {
        int startX = -renderDistance;
        int endX = renderDistance;
        int startY = -renderDistance;
        int endY = renderDistance;

        // Ein Loop für opake und transparente Modelle
        foreach (var alpha in new[] { false, true })
        {
            for (int dx = startX; dx <= endX; dx++)
            {
                for (int dy = startY; dy <= endY; dy++)
                {
                    DrawChunk(gameTime, world, target, dx, dy, alpha);
                }
            }
        }
    }

    private void DrawChunk(GameTime gameTime, World world, Vector3 target, int dx, int dy, bool alpha)
    {
        var chunkX = (int)target.X / World.ChunkWx + dx;
        var chunkY = (int)target.Z / World.ChunkWy + dy;

        if (!_world.Combination.TryGetValue((chunkX, chunkY), out var tuple))
        {
            return;
        }

        var chunkId = tuple.chunkId;

        if (World.Chunks.TryGetValue(chunkId, out var chunk))
        {
            if (chunk.IsLoaded)
            {
                Vector3 offset = new Vector3(chunkX * World.ChunkWx, tuple.height / 2f, chunkY * World.ChunkWy) +
                                 new Vector3(World.ChunkWx, 0, World.ChunkWy) / 2;

                DrawModel(gameTime, _worldShader, chunk.Model, alpha, offset);

                foreach (var building in chunk.Buildings)
                {
                    DrawModel(gameTime, _buildingShader, building.Model, alpha, offset);
                }
            }
        }
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
                        offset: new Vector3(targetX * World.ChunkWx, tuple.height * 0.5f, targetY * World.ChunkWy) +
                                new Vector3(World.ChunkWx, 0, World.ChunkWy) / 2, alpha: false);
                    foreach (var building in chunk.Buildings)
                    {
                        DrawModel(gameTime, _buildingShader, building.Model,
                            offset: new Vector3(targetX * World.ChunkWx, tuple.height, targetY * World.ChunkWy) +
                                    new Vector3(World.ChunkWx, 0, World.ChunkWy) / 2, alpha: false);
                    }
                }
            }
        }

        foreach (var chunkEntry in world.Combination)
        {
            var (targetX, targetY) = chunkEntry.Key;
            var tuple = chunkEntry.Value;
//
            var chunkId = tuple.chunkId;
            var headerId = tuple.headerId;
//
            if (World.Chunks.TryGetValue(chunkId, out var chunk))
            {
                if (chunk.IsLoaded && chunk.Model != null)
                {
                    DrawModel(gameTime, _worldShader, chunk.Model,
                        offset: new Vector3(targetX * World.ChunkWx, tuple.height * 0.5f, targetY * World.ChunkWy) +
                                new Vector3(World.ChunkWx, 0, World.ChunkWy) / 2f, alpha: true);
                    foreach (var building in chunk.Buildings)
                    {
                        DrawModel(gameTime, _buildingShader, building.Model,
                            offset: new Vector3(targetX * World.ChunkWx, tuple.height * 8f, targetY * World.ChunkWy) +
                                    new Vector3(World.ChunkWx, 0, World.ChunkWy) / 2f, alpha: true);
                    }
                }
            }
        }
        
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

        if (alpha)
        {
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
        }
        else
        {
            //GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
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
            AnimationCompareFunction compareFunction = animationPair.Key.CompareFunction;
            foreach (var keyMaterial in animationPair.Key.Materials)
            {
                TextureAnimation animation = null;
                if (compareFunction == AnimationCompareFunction.StartsWith)
                {
                    if (material.Name.StartsWith(keyMaterial))
                    {
                        animation = animationPair.Value;
                    }
                }
                else if (compareFunction == AnimationCompareFunction.Contains)
                {
                    if (material.Name.Contains(keyMaterial))
                    {
                        animation = animationPair.Value;
                    }
                }
                else if (compareFunction == AnimationCompareFunction.Equals)
                {
                    if (material.Name.Equals(keyMaterial))
                    {
                        animation = animationPair.Value;
                    }
                }

                if (animation == null)
                {
                    continue;
                }
                
                if (animation.Type == AnimationType.Texture)
                {
                    effect.Parameters["Texture"]?.SetValue(animation.Frames[animation.CurrentIndex]);
                    return;
                }

                effect.Parameters["ShouldAnimate"]?.SetValue(true);
                effect.Parameters["Offset"]?.SetValue(animation.Offset);
            }
        }
    }
}