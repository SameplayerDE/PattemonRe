using System;
using System.Collections.Generic;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PatteLib.Data;
using PatteLib.Graphics;
using PatteLib.World;
using Pattemon.Engine;
using Pattemon.Global;
using Pattemon.Graphics;
using Pattemon.PoketchApps;
using Pattemon.Scenes.FieldMenu;
using Pattemon.Scenes.Poketch;
using Pattemon.Scenes.WorldMap;
using Camera = Pattemon.Engine.Camera;

namespace Pattemon.Scenes.Field;

public class FieldScene : SceneA
{
    private const int _stateFadeIn = 10;
    private const int _stateWaitFadeIn = 20;
    private const int _stateProcess = 30;
    private const int _stateFadeOut = 40;
    private const int _stateWaitFadeOut = 50;
    private const int _stateMenu = 60;
    private const int _stateApplication = 70;
    
    private int _state = _stateFadeIn;

    private const int _areaIconStateNone = 0;
    private const int _areaIconStateFadeIn = 10;
    private const int _areaIconStateWaitFadeIn = 20;
    private const int _areaIconStateWait = 30;
    private const int _areaIconStateFadeOut = 40;
    private const int _areaIconStateWaitFadeOut = 50;
    
    private int _areaIconState = _areaIconStateNone;
    
    private Texture2D[] _areaIcons = new Texture2D[10];
    
    private const float _areaIconFadeTime = 320f;
    private const float _areaIconStayTime = 5000f;
    
    private Action<ChunkHeader> _onChunkHeaderIdChanged;
    
    private PoketchScene _poketchScene; // for the bottom screen
    
    private Effect _worldShader; // shader for world 3d
    private Effect _buildingShader; // shader for entities
    private Camera _camera; // camera

    private MatrixData _currentMatrix; // the data of the current area (matrix)
    private int _currentHeaderId = int.MinValue;
    private int _prevHeaderId = int.MinValue;
    private int _nextHeaderId = int.MinValue;
    
    private World _world;
    //private Vector3 _spawn = new Vector3(4, 0, 6);
    private Vector3 _spawn = new Vector3(173, 0, 830);
    private Random _random = new Random();
    private float _chunkLoadTimer = 0f;

    
    public FieldScene(Game game, object args = null, string contentDirectory = "Content") : base(game, args, contentDirectory)
    {
        MessageSystem.Subscribe("Application Open", (_) =>
        {
            _state = _stateApplication;
        });
        MessageSystem.Subscribe("Application Close", (_) =>
        {
            _state = _stateMenu;
        });
        _poketchScene = new PoketchScene(game);
        _poketchScene?.Init();
    }

    public override bool Init()
    {
        // load text archieve for chunk names
        LanguageCore.Load("433");
        
        // load area icons
        _areaIcons[0] = GraphicsCore.LoadTexture("area_icon_00", @"Assets/images/00.png");
        _areaIcons[1] = GraphicsCore.LoadTexture("area_icon_01", @"Assets/images/01.png");
        _areaIcons[2] = GraphicsCore.LoadTexture("area_icon_02", @"Assets/images/02.png");
        _areaIcons[3] = GraphicsCore.LoadTexture("area_icon_03", @"Assets/images/03.png");
        _areaIcons[4] = GraphicsCore.LoadTexture("area_icon_04", @"Assets/images/04.png");
        _areaIcons[5] = GraphicsCore.LoadTexture("area_icon_05", @"Assets/images/05.png");
        _areaIcons[6] = GraphicsCore.LoadTexture("area_icon_06", @"Assets/images/06.png");
        _areaIcons[7] = GraphicsCore.LoadTexture("area_icon_07", @"Assets/images/07.png");
        _areaIcons[8] = GraphicsCore.LoadTexture("area_icon_08", @"Assets/images/08.png");
        _areaIcons[9] = GraphicsCore.LoadTexture("area_icon_09", @"Assets/images/09.png");
        
        // 
        _onChunkHeaderIdChanged += header =>
        {
            Console.WriteLine(header.Id);
            _currentHeaderId = header.Id;
            //_camera = Camera.CameraLookMap[header.CameraSettingsId];
            //Camera.ActiveCamera = _camera;
            //_camera.CaptureTarget(() => _spawn);
            //_camera.SetAsActive();
        };
        
        _onChunkHeaderIdChanged += _ =>
        {
            AudioCore.LoadSong(_content, 10, @"Audio/Songs/Sandgem Town (Night)");
            AudioCore.PlaySong(10);
        };
        
        _worldShader = _content.Load<Effect>("Shaders/WorldShader");
        _buildingShader = _content.Load<Effect>("Shaders/BuildingShader");
        
        _camera = Camera.CameraLookMap[0];
        Camera.ActiveCamera = _camera;
        _camera.CaptureTarget(() => _spawn);
        _camera.SetAsActive();

        _currentMatrix = MatrixData.LoadFromFile(@"Content/WorldData/Matrices/0.json");
        
        GraphicsCore.LoadTexture("icon", "Assets/player_icon.png");
        Building.RootDirectory = @"Assets\meshes\output_assets";
        Chunk.RootDirectory = @"Assets\meshes\overworldmaps";
        _world = World.LoadByMatrix(_graphics, 0);
        
        return true;
    }

    public override bool Exit()
    {
        GraphicsCore.FreeTexture("area_icon_00");
        GraphicsCore.FreeTexture("area_icon_01");
        GraphicsCore.FreeTexture("area_icon_02");
        GraphicsCore.FreeTexture("area_icon_03");
        GraphicsCore.FreeTexture("area_icon_04");
        GraphicsCore.FreeTexture("area_icon_05");
        GraphicsCore.FreeTexture("area_icon_06");
        GraphicsCore.FreeTexture("area_icon_07");
        GraphicsCore.FreeTexture("area_icon_08");
        GraphicsCore.FreeTexture("area_icon_09");
        
        GraphicsCore.FreeTexture("icon");
        _content.Unload();
        _content.Dispose();
        return true;
    }

    public override bool Update(GameTime gameTime, float delta)
    {
        _ = World.LoadNextChunkAsync(_graphics);
        
        _world.UpdateChunkLoadingOnMove(_spawn, 2);
        
        switch (_state)
        {
            case _stateFadeIn:
            {
                RenderCore.StartScreenTransition(250, RenderCore.TransitionType.AlphaIn);
                _state = _stateWaitFadeIn;
                return false;
            }
            case _stateWaitFadeIn:
            {
                if (RenderCore.IsScreenTransitionDone())
                {
                    _state = _stateProcess;
                }
                return false;
            }
            case _stateProcess:
            {
                UpdateAreaIconState();
                var direction = KeyboardHandler.GetDirection();
                _spawn += direction;
                
                var currentMatrixCell = _currentMatrix.Get((int)(_spawn.X / 32), (int)(_spawn.Z / 32));
                if (currentMatrixCell != MatrixCellData.Empty)
                {
                    int newHeaderId = currentMatrixCell.HeaderId;
                    if (_currentHeaderId != newHeaderId)
                    {
                        var currentHeader = _services.GetService<HeaderManager>().GetHeaderById(newHeaderId);
                        if (currentHeader != null)
                        {
                            _areaIconState = _areaIconStateFadeIn;
                            _onChunkHeaderIdChanged.Invoke(currentHeader);
                        }
                    }
                }
                
                _poketchScene.Update(gameTime, delta);
                if (KeyboardHandler.IsKeyDownOnce(Keys.Escape))
                {
                    if (!HasProcess)
                    {
                        DualScreenCore.SwapScreens(DualScreenCore.SwapTop, 4);
                        Process = new FieldMenuScene(_game);
                        if (Process.Init())
                        {
                            _state = _stateMenu;
                        }
                    }
                }
                _camera.ComputeViewMatrix();
                return false;
            }
            case _stateMenu:
            {
                _camera.ComputeViewMatrix();
                UpdateAreaIconState();
                goto case _stateApplication;
            }
            case _stateApplication:
            {
                if (Process.Update(gameTime, delta))
                {
                    if (Process.Exit())
                    {
                        Process = null;
                        _state = _stateProcess;
                    }
                }
                return false;
            }
        }
        return true;
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
        switch (_state)
        {
            case _stateWaitFadeIn:
            case _stateProcess:
            case _stateMenu:
            {
                RenderCore.SetTopScreen();
                DrawWorldSmart(gameTime, _world, _spawn);
                DrawAreaIcon(spriteBatch);
                
                RenderCore.SetBottomScreen();
                _poketchScene.Draw(spriteBatch, gameTime, delta);

                if (HasProcess)
                {
                    Process.Draw(spriteBatch, gameTime, delta);
                }
                break;
            }
            case _stateApplication:
            {
                Process.Draw(spriteBatch, gameTime, delta);
                break;
            }
        }
    }

    private void DrawAreaIcon(SpriteBatch spriteBatch)
    {
        if (_areaIconState == _areaIconStateNone) return; // Falls kein Icon aktiv ist, nichts zeichnen

        float animationProgress = TimerCore.Get("area_icon_fade"); // Wert zwischen 0 und 1
        float yOffset = 0f;

        if (_areaIconState is _areaIconStateWaitFadeIn or _areaIconStateFadeIn)
        {
            // Bewegung von -38 nach 0 (von oben nach unten)
            yOffset = -38 + (38 * animationProgress);
        }
        else if (_areaIconState == _areaIconStateWait)
        {
            // Bleibt an Position 0
            yOffset = 0;
        }
        else if (_areaIconState is _areaIconStateWaitFadeOut or _areaIconStateFadeOut)
        {
            // Bewegung von 0 nach -38 (nach oben verschwinden)
            yOffset = -38 * animationProgress;
        }

        var currentHeader = _services.GetService<HeaderManager>().GetHeaderById(_currentHeaderId);
        if (currentHeader != null)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            if (currentHeader.AreaIcon is >= 0 and <= 9)
            {
                var position = new Vector2(4, yOffset);
                position.Floor();
                spriteBatch.Draw(_areaIcons[currentHeader.AreaIcon], position, Color.White);
                RenderCore.WriteText(LanguageCore.GetLine("433", currentHeader.LocationNameId), new Vector2(4 * 3, 5 * 3 + 1 + yOffset), ColorCombination.Area);
            }
            spriteBatch.End();
        }
    }
    
    private void UpdateAreaIconState()
    {
        switch (_areaIconState)
        {
            case _areaIconStateNone:
                break;
            case _areaIconStateFadeIn:
                TimerCore.Create("area_icon_fade", _areaIconFadeTime);
                _areaIconState = _areaIconStateWaitFadeIn;
                break;
            case _areaIconStateWaitFadeIn:
                if (TimerCore.IsDone("area_icon_fade"))
                {
                    TimerCore.Remove("area_icon_fade");
                    TimerCore.Create("area_icon_wait", _areaIconStayTime);
                    _areaIconState = _areaIconStateWait;
                }
                break;
            case _areaIconStateWait:
                if (TimerCore.IsDone("area_icon_wait"))
                {
                    TimerCore.Remove("area_icon_wait");
                    _areaIconState = _areaIconStateFadeOut;
                }
                break;
            case _areaIconStateFadeOut:
                TimerCore.Create("area_icon_fade", _areaIconFadeTime);
                _areaIconState = _areaIconStateWaitFadeOut;
                break;
            case _areaIconStateWaitFadeOut:
                if (TimerCore.IsDone("area_icon_fade"))
                {
                    TimerCore.Remove("area_icon_fade");
                    _areaIconState = _areaIconStateNone;
                }
                break;
        }
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

        if (!world.Combination.TryGetValue((chunkX, chunkY), out var tuple))
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

                RenderCore.DrawModel(gameTime, _worldShader, chunk.Model, alpha, offset);

                foreach (var building in chunk.Buildings)
                {
                    // Hol dir die ursprüngliche Position des Gebäudes
                    Vector3 buildingPos = building.Position;

                    // Erstelle eine Wellenbewegung basierend auf der X- oder Z-Position
                    float waveOffset = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds + buildingPos.X * 0.1f) * 4.0f;

                    // Wende die Wellenbewegung auf die Y-Koordinate an
                    Vector3 animatedOffset = offset + new Vector3(0, waveOffset, 0);

                    // Zeichne das Modell mit der neuen Position
                    RenderCore.DrawModel(gameTime, _buildingShader, building.Model, alpha, animatedOffset);
                }

            }
        }
    }
}