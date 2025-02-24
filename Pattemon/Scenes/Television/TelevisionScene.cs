using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pattemon.Engine;
using Pattemon.Graphics;
using Pattemon.Scenes.ChoosePokemon;
using Pattemon.Scenes.Field;

namespace Pattemon.Scenes.Television;

public class TelevisionScene : SceneA
{
    private const int _stateFadeIn = 10;
    private const int _stateWaitFadeIn = 20;
    private const int _stateProcess = 30;
    private const int _stateFadeOut = 40;
    private const int _stateWaitFadeOut = 50;
    
    private int _state = 0;

    private float _timerValue = 0f;
    private const float _timerDuration = 5f;
    private float _scrollValue;
    
    public TelevisionScene(Game game, object args = null, string contentDirectory = "Content") : base(game, args, contentDirectory)
    {
    }

    public override bool Init()
    {
        GraphicsCore.LoadTexture("television_intro_city", @"Assets/television_intro_city.png");
        GraphicsCore.LoadTexture("television_intro_border", @"Assets/television_intro_border.png");
        GraphicsCore.LoadTexture("television_intro_text", @"Assets/television_intro_text.png");
        GraphicsCore.LoadTexture("television_intro_effect", @"Assets/television_intro_effect.png");
        _state = _stateFadeIn;
        return true;
    }

    public override bool Exit()
    {
        GraphicsCore.FreeTexture("television_intro_city");
        GraphicsCore.FreeTexture("television_intro_border");
        GraphicsCore.FreeTexture("television_intro_text");
        GraphicsCore.FreeTexture("television_intro_effect");
        return true;
    }

    public override bool Update(GameTime gameTime, float delta)
    {
        _scrollValue += 8 * delta;
        switch (_state)
        {
            case _stateFadeIn:
            {
                DualScreenCore.SwapScreens(DualScreenCore.SwapTop);
                DualScreenCore.Disable();
                if (!DualScreenCore.IsSwappingScreens)
                {
                    RenderCore.StartScreenTransition(250, RenderCore.TransitionType.AlphaIn);
                    _state = _stateWaitFadeIn;
                }
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
                if (_timerValue >= _timerDuration)
                {
                    if (KeyboardHandler.IsKeyDownOnce(Keys.Enter))
                    {
                        _state = _stateFadeOut;
                    }
                }
                else
                {
                    _timerValue += delta;
                }

                break;
            }
            case _stateFadeOut:
            {
                RenderCore.StartScreenTransition(250, RenderCore.TransitionType.AlphaOut);
                _services.GetService<SceneAManager>().Next(new FieldScene(_game));
                _state = _stateWaitFadeOut;
                break;
            }
            case _stateWaitFadeOut:
            {
                if (RenderCore.IsScreenTransitionDone())
                {
                    DualScreenCore.Enable();
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
        Rectangle sourceRect = new Rectangle(0, (int)_scrollValue, GraphicsCore.GetTexture("television_intro_effect").Width, GraphicsCore.GetTexture("television_intro_effect").Height);
        
        RenderCore.SetTopScreen();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap);
        spriteBatch.Draw(GraphicsCore.GetTexture("television_intro_city"), Vector2.Zero, Color.White);
        spriteBatch.Draw(GraphicsCore.GetTexture("television_intro_text"), Vector2.Zero, Color.White);
        
        spriteBatch.Draw(GraphicsCore.GetTexture("television_intro_effect"), 
            Vector2.Zero, sourceRect, Color.White * 0.333334f);
        
        spriteBatch.Draw(GraphicsCore.GetTexture("television_intro_border"), Vector2.Zero, Color.White);
        spriteBatch.End();
    }
}