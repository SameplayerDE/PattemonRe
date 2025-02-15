using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Pattemon.Engine;

public abstract class SceneA
{
    public SceneA? Process { get; protected set; } = null;
    public bool HasProcess => Process != null;
    
    protected ContentManager _content;
    protected GraphicsDevice _graphics;
    protected readonly Game _game;
    protected GameServiceContainer _services => _game.Services;
    protected GameWindow _window => _game.Window;
    
    private object _args;
    
    protected SceneA(Game game, object args = null, string contentDirectory = "Content")
    {
        _game = game;
        _args = args;
        _graphics = game.GraphicsDevice;
        _content = new ContentManager(game.Services, contentDirectory);
    }
    
    // loads data and inits
    public abstract bool Init();
    // frees memory and cleans up
    public abstract bool Exit();
    
    // Nitro System uses one function
    // I use two, because Update and Draw are seperated
    // Only update() returns a bool, because draw only renders
    // and should not manage the state of the scene
    public abstract bool Update(GameTime gameTime, float delta);
    public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime, float delta);
}