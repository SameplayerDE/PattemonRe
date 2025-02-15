using System;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PatteLib.Data;
using PatteLib.World;
using Pattemon.Engine;
using Pattemon.Graphics;

namespace Pattemon.Scenes.WorldMap;

public class WorldMapScene : SceneA
{
    // state
    private const int _stateFadeIn = 0;
    private const int _stateWaitFadeIn = 1;
    private const int _stateMain = 2;
    private const int _stateFadeOut = 3;
    private const int _stateWaitFadeOut = 4;
    
    private int _state = _stateFadeIn;

    
    // assets
    private Texture2D _islandTexture;
    private Texture2D _roadTexture;
    private Texture2D _playerIconTexture;
    private Texture2D _cursorSheetTexture;
    
    private WorldMapMatrix _worldMatrix;
    
    // vars
    private int _x;
    private int _y;
    private int _mapX;
    private int _mapY;
    private int _offsetX = 3 * 8;
    private int _offsetY = -1; // bug inside the ROM fixed
    private int _cellSizeX = 7;
    private int _cellSizeY = 7;
    
    private int _animationState = 0;
    private int _animationFrame = 0;
    private float _animationTime = 0;

    private WorldMapMatrixEntry _currentWorldMapRegion = WorldMapMatrixEntry.Empty;
    
    public WorldMapScene(Game game, object args = null, string contentDirectory = "Content") : base(game, args, contentDirectory)
    {
    }

    public override bool Init()
    {
        // load map matrix
        _worldMatrix = WorldMapMatrix.LoadFromFile(@$"Assets/worldmap.json");
        
        // load assets
        _islandTexture = GraphicsCore.LoadTexture("worldmap_0", @"Assets/worldmap_0.png");
        _roadTexture = GraphicsCore.LoadTexture("worldmap_1", @"Assets/worldmap_1.png");
        _playerIconTexture = GraphicsCore.LoadTexture("worldmap_icons", @"Assets/player_icon.png");
        _cursorSheetTexture = GraphicsCore.LoadTexture("worldmap_cursor", @"Assets/worldmap_cursor.png");
        
        // load textures for everything that is being drawn
        return true;
    }

    
    public override bool Exit()
    {
        // unload all assets
        
        
        // free memory
        return true;
    }
    
    public override bool Update(GameTime gameTime, float delta)
    {
        switch (_state)
        {
            case _stateFadeIn:
            {
                RenderCore.StartScreenTransition(500, RenderCore.TransitionType.AlphaIn);
                _state++;
                break;
            }
            case _stateWaitFadeIn:
            {
                if (RenderCore.IsScreenTransitionDone())
                {
                    _state++;
                }
                break;
            }
            case _stateMain:
            {
                ProcessInput();
                
                // calculate render position
                _mapX = _x * _cellSizeX + _offsetX;
                _mapY = _y * _cellSizeY + _offsetY;
                
                _animationTime += delta;
                if (_animationTime >= 0.25f) // 250ms
                {
                    _animationTime = 0;
                    if (_animationState == 0)
                    {
                        _animationFrame = 1;
                        _animationState = 1;
                    }
                    else if (_animationState == 1)
                    {
                        _animationFrame = 0;
                        _animationState = 0;
                    }
                }
                
                break;
            }
            case _stateFadeOut:
            {
                RenderCore.StartScreenTransition(500, RenderCore.TransitionType.AlphaOut);
                _state++;
                break;
            }
            case _stateWaitFadeOut:
            {
                if (RenderCore.IsScreenTransitionDone())
                {
                    return true;
                }
                break;
            }
        }
       
        return false;
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
        RenderCore.SetTopScreen();
        spriteBatch.Begin();
        spriteBatch.Draw(_islandTexture, new Vector2(0, 0), Color.White);
        spriteBatch.Draw(_roadTexture, new Vector2(0, 0), Color.White);
        spriteBatch.Draw(_cursorSheetTexture, new Vector2(_mapX, _mapY), new Rectangle(16 * _animationFrame, 0, 16 ,16), Color.White);
        if (!Equals(_currentWorldMapRegion, WorldMapMatrixEntry.Empty))
        {
           // RenderCore.WriteText(_currentWorldMapRegion.Name, new Vector2(00));
        }
        spriteBatch.End();
    }
    
    #region

    private void ProcessInput()
    {
        if (KeyboardHandler.IsKeyDownOnce(Keys.Escape))
        {
            _state = _stateFadeOut;
            return;
        }
        var positionChanged = false;
        if (KeyboardHandler.IsKeyDownOnce(Keys.Left))
        {
            _x--;
            positionChanged = true;
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.Right))
        {
            _x++;
            positionChanged = true;
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.Up))
        {
            _y--;
            positionChanged = true;
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.Down))
        {
            _y++;
            positionChanged = true;
        }
        if (positionChanged)
        {
            _x = Math.Clamp(_x, 0, 28);
            _y = Math.Clamp(_y, 0, 22);
            
            var cellData = _worldMatrix.Get(_x, _y);
            if (!Equals(cellData, _currentWorldMapRegion))
            {
                _currentWorldMapRegion = cellData;
            }
        }
    }
    
    #endregion
}