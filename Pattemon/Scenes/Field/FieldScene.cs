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

    private int _state = 0;
    
    public FieldScene(Game game, object args = null, string contentDirectory = "Content") : base(game, args, contentDirectory)
    {
    }

    public override bool Init()
    {
        GraphicsCore.LoadTexture("icon", "Assets/player_icon.png");
        return true;
    }

    public override bool Exit()
    {
        GraphicsCore.FreeTexture("icon");
        return true;
    }

    public override bool Update(GameTime gameTime, float delta)
    {

        switch (_state)
        {
            case 0:
            {
                if (KeyboardHandler.IsKeyDownOnce(Keys.Escape))
                {
                    if (!HasProcess)
                    {
                        Process = new FieldMenuScene(_game);
                        if (Process.Init())
                        {
                            _state++;
                        }
                    }
                }
                if (KeyboardHandler.IsKeyDownOnce(Keys.M))
                {
                    if (!HasProcess)
                    {
                        Process = new WorldMapScene(_game);
                        if (Process.Init())
                        {
                            _state++;
                        }
                    }
                }
                return false;
            }
            case 1:
            {
                if (Process.Update(gameTime, delta))
                {
                    if (Process.Exit())
                    {
                        Process = null;
                        _state--;
                    }
                }
                return false;
            }
        }
        return false;
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
        RenderCore.SetTopScreen();
        spriteBatch.Begin();
        spriteBatch.Draw(GraphicsCore.GetTexture("icon"), new Vector2(0, 0), Color.White);
        spriteBatch.End();

        if (HasProcess)
        {
            if (_state == 1)
            {
                Process.Draw(spriteBatch, gameTime, delta);
            }
        }
        
    }
}