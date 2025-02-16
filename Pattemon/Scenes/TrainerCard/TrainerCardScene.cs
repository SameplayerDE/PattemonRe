using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pattemon.Engine;
using Pattemon.Graphics;

namespace Pattemon.Scenes.TrainerCard;

public class TrainerCardScene : SceneA
{
    private const int _stateFadeIn = 10;
    private const int _stateWaitFadeIn = 20;
    private const int _stateProcess = 30;
    private const int _stateFadeOut = 40;
    private const int _stateWaitFadeOut = 50;
    
    private int _state = 0;
    
    public TrainerCardScene(Game game, object args = null, string contentDirectory = "Content") : base(game, args, contentDirectory)
    {
    }

    public override bool Init()
    {
        GraphicsCore.LoadTexture("trainer_card", @"Assets/trainerpass_000.png");
        _state = _stateFadeIn;
        return true;
    }

    public override bool Exit()
    {
        GraphicsCore.FreeTexture("trainer_card");
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
        spriteBatch.Draw(GraphicsCore.GetTexture("trainer_card"), Vector2.Zero, Color.White);
        spriteBatch.End();
    }
}