using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Pattemon.PoketchApps;

public abstract class PoketchApp
{
    
    protected readonly Game _game;
    protected GameServiceContainer _services => _game.Services;
    protected GameWindow _window => _game.Window;
    
    protected ContentManager _content;
    protected GraphicsDevice _graphics;
    private object _args;
    
    protected const int _stateFadeIn = 10;
    protected const int _stateWaitFadeIn = 20;
    protected const int _stateProcess = 30;
    protected const int _stateFadeOut = 40;
    protected const int _stateWaitFadeOut = 50;
    protected int _state;
    
    protected PoketchApp(Game game, object args = null, string contentDirectory = "Content")
    {
        _game = game;
        _args = args;
        _graphics = game.GraphicsDevice;
        _content = new ContentManager(game.Services, contentDirectory);
    }
    
    public abstract bool Init();
    public abstract bool Exit();
    public abstract void Process(GameTime gameTime, float delta);
    public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime, float delta);
    
    public virtual bool Update(GameTime gameTime, float delta)
    {
        switch (_state)
        {
            case _stateFadeIn:
            {
                // transition
                _state = _stateWaitFadeIn;
                break;
            }
            case _stateWaitFadeIn:
            {
                if (true)
                {
                    _state = _stateProcess;
                }
                break;
            }
            case _stateProcess:
            {
                Process(gameTime, delta);
                break;
            }
            case _stateFadeOut:
            {
                // transition
                _state = _stateWaitFadeIn;
                break;
            }
            case _stateWaitFadeOut:
            {
                if (true)
                {
                    return Exit();
                }
                break;
            }
        }
        return false;
    }
    
    public void Close()
    {
        if (_state == _stateProcess)
        {
            _state = _stateFadeOut;
        }
    }
}