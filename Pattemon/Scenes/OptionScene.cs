using System;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PatteLib;
using Pattemon.Engine;

public class OptionScene : Scene
{
    private enum State
    {
        FadeId = 0,
        WaitFadeIn,
        Process,
        FadeOut,
        WaitFadeOut,
    }
    
    private State _state;
    private Texture2D _dummy;
    private Texture2D _selector;
    
    private int _optionCursor;
    
    public OptionScene(string name, Game game, string contentDirectory = "Content") : base(name, game, contentDirectory)
    {
    }

    public override bool Load()
    {
        _dummy = Content.Load<Texture2D>("DummyOptions");
        _selector = Content.Load<Texture2D>("OptionSelector");
        return true;
    }

    protected override bool Unload()
    {
        Content.Unload();
        return true;
    }

    public override bool Init()
    {
        _optionCursor = 0;
        _state = State.FadeId;
        return true;
    }

    public override bool Update(GameTime gameTime, float delta)
    {
        switch (_state)
        {
            case State.FadeId:
                StartScreenTransition();
                SceneManager.Next(Scene.PlayerMenu);
                _state = State.WaitFadeIn; // Wechsle in den Fade-In Zustand
                break;

            case State.WaitFadeIn:
                if (RenderCore.IsScreenTransitionDone())
                {
                    _state++;
                }
                break;

            case State.Process:
            {
                if (KeyboardHandler.IsKeyDownOnce(Keys.Down))
                {
                    _optionCursor++;
                }
                if (KeyboardHandler.IsKeyDownOnce(Keys.Up))
                {
                    _optionCursor--;
                }
                _optionCursor = Utils.Wrap(_optionCursor, 0, 6);

                if (KeyboardHandler.IsKeyDownOnce(Keys.Enter))
                {
                    if (_optionCursor == 6)
                    {
                        _state = State.FadeOut;
                    }
                }
            }
                break;
            case State.FadeOut:
                RenderCore.StartScreenTransition(1000, RenderCore.TransitionType.SlideIn);
                _state = State.WaitFadeOut;
                break;

            case State.WaitFadeOut:
                if (RenderCore.IsScreenTransitionDone())
                {
                    return true;
                }
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
        return false;
    }

    private bool ScreenTransitionDone()
    {
        // Hier könnte die tatsächliche Logik für den Übergang überprüft werden.
        return true;
    }

    private void StartScreenTransition()
    {
        RenderCore.StartScreenTransition(1000, RenderCore.TransitionType.SlideOut);
    }

    protected override void Draw2D(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
        RenderCore.SetTopScreen();
        GraphicsDevice.Clear(Color.Black);
        spriteBatch.Begin();
        spriteBatch.Draw(_dummy, Vector2.Zero, Color.White);
        spriteBatch.Draw(_selector, (new Vector2(2, 6 + _optionCursor * 4) * 4), Color.White);
        spriteBatch.End();
        
        RenderCore.SetBottomScreen();
        GraphicsDevice.Clear(Color.Black);
    }

    protected override void Draw3D(GameTime gameTime, float delta)
    {
        // Hier könnte eine 3D-Logik eingebaut werden, falls benötigt
    }

    public override void Exit()
    {
        Unload();
    }
}
