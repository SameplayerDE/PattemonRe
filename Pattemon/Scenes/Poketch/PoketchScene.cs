using System;
using System.Collections.Generic;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PatteLib;
using Pattemon.Engine;
using Pattemon.Global;
using Pattemon.Graphics;
using Pattemon.PoketchApps;
using Pattemon.PoketchApps.Calculator;
using Pattemon.PoketchApps.DigitalClock;

namespace Pattemon.Scenes.Poketch;

public class PoketchScene : SceneA
{
    private List<PoketchApp> _installedApps = [];
    private int _currentAppIndex = 0;

    private int _appIndex;

    private Texture2D _bottomScreen;
    private Texture2D _poketchOverlay;
    private Color _backgroundColor = ColorUtils.FromHex("#303030");
    
    public PoketchScene(Game game, object args = null, string contentDirectory = "Content") : base(game, args, contentDirectory)
    {
        PlayerData.HasPoketch = true;
    }

    public override bool Init()
    {
        _bottomScreen = _content.Load<Texture2D>("BottomScreen");
        _poketchOverlay = GraphicsCore.LoadTexture("PoketchOverlay", "Assets/poketch_overlay.png");

        MessageSystem.Subscribe(MessageSystem.PoketchApp, (param) =>
        {
            var appId = param as string;
            switch (appId)
            {
                case "DigitalClock":
                    _installedApps.Add(new DigitalClockPoketchApp(_game));
                    break;
                case "Calculator":
                    _installedApps.Add(new CalculatorPoketchApp(_game));
                    break;
            }
        });
        
        return true;
    }

    public override bool Exit()
    {
        return true;
    }

    public override bool Update(GameTime gameTime, float delta)
    {
        if (!DualScreenCore.IsBottomScreenInFocus)
        {
            return false;
        }
        // Upper
        if (MouseHandler.IsAreaOnce(MouseButton.Left, new Rectangle(28 * 8, 4 * 8, 4 * 8, 8 * 8)))
        {
            _appIndex++;
        }
        // Lower
        if (MouseHandler.IsAreaOnce(MouseButton.Left, new Rectangle(28 * 8, 12 * 8, 4 * 8, 8 * 8)))
        {
            _appIndex--;
        }

        if (_appIndex < 0)
        {
            _appIndex = _installedApps.Count - 1;
        }
        if (_appIndex >= _installedApps.Count)
        {
            _appIndex = 0;
        }
        
        return true;
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
       
        if (!PlayerData.HasPoketch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(_bottomScreen, new Vector2(0, 0), Color.White);
            spriteBatch.End();
        }
        else
        {
            _graphics.Clear(_backgroundColor);
            spriteBatch.Begin();
            spriteBatch.Draw(_poketchOverlay, new Vector2(0, 0), Color.White);
            spriteBatch.End();
        }
    }
}