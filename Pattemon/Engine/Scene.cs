using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Pattemon.Engine;

public enum SceneState
{
    Load,
    Init,
    Exec,
    Unload,
    Exit,
}

/// <summary>
/// The abstract base class for a game scene.  
/// Manages resources such as graphics, content, and provides methods for the scene's lifecycle.
/// </summary>
public abstract class Scene
{
    public const string Game = "Game";
    public const string PlayerMenu = "PlayerMenu";
    public const string PlayerMenuOptionMenu = "PlayerMenuOptionMenu";
    
    public string Name { get; protected set; }
    
    public SceneState State { get; private set; } = SceneState.Load;
    protected ContentManager Content;
    protected GraphicsDevice GraphicsDevice;
    protected readonly Game _game;
    protected GameServiceContainer Services => _game.Services;
    protected GameWindow Window => _game.Window;
    protected SceneManager SceneManager;
    
    protected Scene Child = null;
    protected Scene Parent = null;
    protected object Args { get; private set; }

    protected Scene(string name, Game game, object args = null, string contentDirectory = "Content")
    {
        Name = name;
        _game = game;
        Args = args;
        GraphicsDevice = game.GraphicsDevice;
        Content = new ContentManager(game.Services, contentDirectory);
    }
    
    public void SetManager(SceneManager manager)
    {
        SceneManager = manager;
    }

    public abstract bool Load();
    protected virtual bool Unload() { return true; }
    public abstract bool Init();
    public abstract bool Update(GameTime gameTime, float delta);
    protected abstract void Draw2D(SpriteBatch spriteBatch, GameTime gameTime, float delta);
    protected abstract void Draw3D(GameTime gameTime, float delta);
    public abstract void Exit();
    
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
        if (spriteBatch == null)
        {
            Draw3D(gameTime, delta);
            return;
        }
        Draw2D(spriteBatch, gameTime, delta);
    }
 
}