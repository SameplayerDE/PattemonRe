using System;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PatteLib.World;
using Pattemon.Engine;
using Pattemon.Global;
using Pattemon.Graphics;
using Pattemon.Scenes.FieldMenu;
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

    private SceneA _poketchScene;
    
    private Texture2D _background;
    private Texture2D _bottomScreen;
    private Effect _worldShader;
    private Effect _buildingShader;
    private Camera _camera;
    private World _world;
    private Vector3 _spawn = new Vector3(4, 0, 6);
    private Random _random = new Random();
    
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
        MessageSystem.Subscribe("Poketch", (scene) =>
        {
            _poketchScene = scene as SceneA;
            _poketchScene?.Init();
        });
    }

    public override bool Init()
    {
        GraphicsCore.LoadTexture("icon", "Assets/player_icon.png");
        _background = _content.Load<Texture2D>("TopScreen");
        _bottomScreen = _content.Load<Texture2D>("BottomScreen");
        
        Building.RootDirectory = @"Assets\meshes\output_assets";
        Chunk.RootDirectory = @"Assets\meshes\overworldmaps";
   
        _worldShader = _content.Load<Effect>("Shaders/WorldShader");
        _buildingShader = _content.Load<Effect>("Shaders/BuildingShader");
        _world = World.LoadByMatrix(_graphics, 5);
        _camera = Camera.CameraLookMap[4];
        Camera.ActiveCamera = _camera;
        _camera.CaptureTarget(() => _spawn);
        _camera.SetAsActive();
        return true;
    }

    public override bool Exit()
    {
        GraphicsCore.FreeTexture("icon");
        _content.Unload();
        _content.Dispose();
        return true;
    }

    public override bool Update(GameTime gameTime, float delta)
    {

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
                if (PlayerData.HasPoketch)
                {
                    _poketchScene.Update(gameTime, delta);
                }
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
                
                RenderCore.SetBottomScreen();
                if (PlayerData.HasPoketch)
                {
                    _poketchScene.Draw(spriteBatch, gameTime, delta);
                }
                else
                {
                    spriteBatch.Begin();
                    spriteBatch.Draw(_bottomScreen, new Vector2(0, 0), Color.White);
                    spriteBatch.End();
                }

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