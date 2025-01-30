using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pattemon.Engine;

public class SceneManager
{
    private Dictionary<string, Scene> _registeredScenes = new();

    private Scene _currentScene;
    private Scene _nextScene;

    public void RegisterScene(Scene scene)
    {
        _registeredScenes.TryAdd(scene.Name, scene);
    }

    public Scene GetScene(string id)
    {
        return _registeredScenes.GetValueOrDefault(id);
    }
    
    public void Next(string id)
    {
        if (_registeredScenes.TryGetValue(id, out var scene))
        {
            _nextScene = scene;
        }
        else
        {
            throw new ArgumentException($"Scene with ID '{id}' is not registered.");
        }
    }

    public void Update(GameTime gameTime)
    {
        if (_currentScene == null)
        {
            if (_nextScene != null)
            {
                _currentScene = _nextScene;
                Prepare(_currentScene);
                _nextScene = null;
            }
            else
            {
                return;
            }
        }
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_currentScene.Update(gameTime, delta))
        {
            _currentScene.Exit();
            _currentScene = null;
        }
    }

    private void Prepare(Scene scene)
    {
        scene.SetManager(this);
        scene.Load();
        scene.Init();
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (_currentScene == null)
        {
            return;
        }
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _currentScene?.Draw(null, gameTime, delta);
        _currentScene?.Draw(spriteBatch, gameTime, delta);
    }
}