using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pattemon.Engine;

public class SceneManager
{
    private Stack<Scene> _sceneStack = [];

    public void Push(Scene scene)
    {
        scene.SetManager(this);
        scene.Init();
        scene.Load();
        _sceneStack.Push(scene);
    }
    
    public void Pop()
    {
        _sceneStack.Pop();
    }

    public void Clear()
    {
        _sceneStack.Clear();
    }
    
    public void Update(GameTime gameTime)
    {
        if (_sceneStack.Count == 0)
        {
            return;
        }

        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _sceneStack.Peek().Update(gameTime ,delta);
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (_sceneStack.Count == 0)
        {
            return;
        }
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _sceneStack.Peek().Draw(spriteBatch, gameTime ,delta);
    }
    
}