using System;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pattemon.Engine;
using Pattemon.Global;
using Pattemon.Graphics;
using Pattemon.PoketchApps;
using Pattemon.PoketchApps.DigitalClock;

namespace Pattemon.Scenes.Poketch;

public class PoketchScene : SceneA
{

    private PoketchApp _currentApp;
    private Texture2D _bottomScreen;
    private Texture2D _poketchOverlay;
    
    public PoketchScene(Game game, object args = null, string contentDirectory = "Content") : base(game, args, contentDirectory)
    {
    }

    public override bool Init()
    {
        _bottomScreen = _content.Load<Texture2D>("BottomScreen");
        _poketchOverlay = GraphicsCore.LoadTexture("PoketchOverlay", "Assets/poketch_overlay.png");

        _currentApp = new DigitalClockPoketchApp(_game);
        _currentApp.Init();
        
        return true;
    }

    public override bool Exit()
    {
        _currentApp.Exit();
        return true;
    }

    public override bool Update(GameTime gameTime, float delta)
    {
        if (!DualScreenCore.IsBottomScreenInFocus)
        {
            return false;
        }
        
        _currentApp.Update(gameTime, delta);
        // Upper
        if (MouseHandler.IsAreaOnce(MouseButton.Left, new Rectangle(28 * 8, 4 * 8, 4 * 8, 7 * 8)))
        {
            Console.WriteLine("Upper");
        }
        // Lower
        if (MouseHandler.IsAreaOnce(MouseButton.Left, new Rectangle(28 * 8, 12 * 8, 4 * 8, 7 * 8)))
        {
            Console.WriteLine("Down");
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
            spriteBatch.Begin();
            spriteBatch.Draw(_poketchOverlay, new Vector2(0, 0), Color.White);
            spriteBatch.End();
            _currentApp.Draw(spriteBatch, gameTime, delta);
        }
    }
}