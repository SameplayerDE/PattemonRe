using System;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PatteLib;
using Pattemon.Data;
using Pattemon.Engine;
using Pattemon.Graphics;

namespace Pattemon.Scenes.Inventory;

public class InventoryScene : SceneA
{
    private const int _stateFadeIn = 10;
    private const int _stateWaitFadeIn = 20;
    private const int _stateProcess = 30;
    private const int _stateFadeOut = 40;
    private const int _stateWaitFadeOut = 50;
    
    private int _state = 0;

    // vars
    
    private const int _categoryLaneX = 7;
    private const int _categoryLaneY = 91;
    private const int _categoryLaneIconW = 10;
    private const int _categoryLaneIconH = 10;
    private const int _categoryLaneIconSpace = 1;
    private int _categoryLaneIndex = 0;
    
    private const int _bagIconW = 64;
    private const int _bagIconH = 64;

    private Bag _bag;
    
    public InventoryScene(Game game, object args = null, string contentDirectory = "Content") : base(game, args, contentDirectory)
    {
    }

    public override bool Init()
    {
        GraphicsCore.LoadTexture("inventory", @"Assets/bag_overlay.png");
        GraphicsCore.LoadTexture("inventory_bag", @"Assets/bag_sheet.png");
        GraphicsCore.LoadTexture("inventory_category_lane_sheet", @"Assets/bag_icon.png");
        GraphicsCore.LoadTexture("inventory_category_lane_cursor", @"Assets/bag_icon_cursor.png");
        
        //_bag = _services.GetService<Bag>();
        
        _state = _stateFadeIn;
        return true;
    }

    public override bool Exit()
    {
        GraphicsCore.FreeTexture("inventory");
        GraphicsCore.FreeTexture("inventory_bag");
        GraphicsCore.FreeTexture("inventory_category_lane_sheet");
        GraphicsCore.FreeTexture("inventory_category_lane_cursor");
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
                break;
            }
            case _stateWaitFadeIn:
            {
                if (RenderCore.IsScreenTransitionDone())
                {
                    _state = _stateProcess;
                }
                break;
            }
            case _stateProcess:
            {
                if (KeyboardHandler.IsKeyDownOnce(Keys.Escape))
                {
                    _state = _stateFadeOut;
                }

                if (KeyboardHandler.IsKeyDownOnce(Keys.Left))
                {
                    _categoryLaneIndex--;
                }
                if (KeyboardHandler.IsKeyDownOnce(Keys.Right))
                {
                    _categoryLaneIndex++;
                }
                _categoryLaneIndex = Utils.Wrap(_categoryLaneIndex, 0, 7);
                break;
            }
            case _stateFadeOut:
            {
                RenderCore.StartScreenTransition(250, RenderCore.TransitionType.AlphaOut);
                _state = _stateWaitFadeOut;
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
        if (_state <= _stateFadeIn)
        {
            return;
        }
        RenderCore.SetTopScreen();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        // render text
        spriteBatch.Draw(GraphicsCore.GetTexture("inventory"), Vector2.Zero, Color.White);
        // render icons
        for (int iconIndex = 0; iconIndex < 8; iconIndex++)
        {
            spriteBatch.Draw(GraphicsCore.GetTexture("inventory_category_lane_sheet"), new Vector2(_categoryLaneX + ((_categoryLaneIconSpace + _categoryLaneIconW) * iconIndex), _categoryLaneY), new Rectangle(iconIndex == _categoryLaneIndex ? _categoryLaneIconW : 0, iconIndex * _categoryLaneIconH, _categoryLaneIconW, _categoryLaneIconH), Color.White);
        }
        spriteBatch.Draw(GraphicsCore.GetTexture("inventory_category_lane_cursor"), new Vector2(_categoryLaneX + ((_categoryLaneIconSpace + _categoryLaneIconW) * _categoryLaneIndex) - 3, _categoryLaneY - 3), Color.White);
        spriteBatch.Draw(GraphicsCore.GetTexture("inventory_bag"), new Vector2(16, 16), new Rectangle(_bagIconW * 0, _bagIconH * _categoryLaneIndex, _bagIconW, _bagIconH), Color.White);
        // render content
        spriteBatch.End();
    }
}