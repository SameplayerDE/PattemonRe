using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pattemon.Engine;

public class SceneManager
{
    private Scene _currentScene;
    private Scene _nextScene;

    public void Push(Scene scene)
    {
        scene.SetManager(this);
        _nextScene = scene;
    }
    
    public void Pop()
    {
        _currentScene = null;
    }
    
    public void Update(GameTime gameTime)
    {
        if (_currentScene == null)
        {
            if (_nextScene == null)
            {
                return;
            }
            _currentScene = _nextScene;
            _nextScene = null;
            
            _currentScene.Load();
            _currentScene.Init();
        }
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _currentScene.Update(gameTime ,delta);
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (_currentScene == null)
        {
            return;
        }
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _currentScene.Draw(spriteBatch, gameTime ,delta);
    }
    
}