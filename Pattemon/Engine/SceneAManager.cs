using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pattemon.Engine;

public class SceneAManager
{
    private SceneA _currentScene;
    private SceneA _nextScene;

    public void Next(SceneA scene)
    {
        _nextScene = scene;
    }
    
    public void Update(GameTime gameTime, float delta)
    {
        if (_currentScene == null)
        {
            if (_nextScene != null)
            {
                _currentScene = _nextScene;
                _currentScene.Init();
                _nextScene = null;
            }
            else
            {
                return;
            }
        }

        if (_currentScene.Update(gameTime, delta))
        {
            _currentScene.Exit();
            _currentScene = null;
        }
    }
    
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
        _currentScene?.Draw(spriteBatch, gameTime, delta);
    }
}