using System;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pattemon.Engine;
using Pattemon.Graphics;
using Pattemon.Scenes.FieldMenu;
using Pattemon.Scenes.WorldMap;

namespace Pattemon.Scenes.Field;

public class FieldScene : SceneA
{
    private const int _stateMain = 10;
    private const int _stateMenu = 20;
    private const int _stateApplication = 30;
    
    private int _state = _stateMain;
    
    private Texture2D _background;
    private Texture2D _bottomScreen;
    
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
    }

    public override bool Init()
    {
        GraphicsCore.LoadTexture("icon", "Assets/player_icon.png");
        _background = _content.Load<Texture2D>("TopScreen");
        _bottomScreen = _content.Load<Texture2D>("BottomScreen");
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
            case _stateMain:
            {
                if (KeyboardHandler.IsKeyDownOnce(Keys.Escape))
                {
                    if (!HasProcess)
                    {
                        DualScreenCore.SwapScreens(4, DualScreenCore.SwapTop);
                        Process = new FieldMenuScene(_game);
                        if (Process.Init())
                        {
                            _state = _stateMenu;
                        }
                    }
                }
                return false;
            }
            case _stateMenu:
            case _stateApplication:
            {
                if (Process.Update(gameTime, delta))
                {
                    if (Process.Exit())
                    {
                        Process = null;
                        _state = _stateMain;
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
            case _stateMain:
            case _stateMenu:
            {
                RenderCore.SetTopScreen();
                spriteBatch.Begin();
                spriteBatch.Draw(_background, new Vector2(0, 0), Color.White);
                spriteBatch.End();
                
                RenderCore.SetBottomScreen();
                spriteBatch.Begin();
                spriteBatch.Draw(_bottomScreen, new Vector2(0, 0), Color.White);
                spriteBatch.End();
                
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
}