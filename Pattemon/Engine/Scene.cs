using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Pattemon.Engine;

public abstract class Scene
{
    public string Name { get; protected set; }
    protected ContentManager Content;
    protected GraphicsDevice GraphicsDevice;
    protected readonly Game _game;
    protected GameServiceContainer Services => _game.Services;
    protected GameWindow Window => _game.Window;
    protected SceneManager SceneManager;
    
    protected Scene(string name, Game game, string contentDirectory = "Content")
    {
        Name = name;
        _game = game;
        GraphicsDevice = game.GraphicsDevice;
        Content = new ContentManager(game.Services, contentDirectory);
    }

    public void SetManager(SceneManager manager)
    {
        SceneManager = manager;
    }
    
    public abstract void Load();
    public abstract void Unload();
    public abstract void Init();
    
    public abstract void Update(GameTime gameTime, float delta);
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
        Draw3D(gameTime, delta);
        Draw2D(spriteBatch, gameTime, delta);
    }
    protected abstract void Draw2D(SpriteBatch spriteBatch, GameTime gameTime, float delta);
    protected abstract void Draw3D(GameTime gameTime, float delta);
}