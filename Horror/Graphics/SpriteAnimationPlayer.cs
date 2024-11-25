using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Horror.Graphics;

public class SpriteAnimationPlayer(IServiceProvider serviceProvider)
{
    private readonly Dictionary<string, SpriteAnimation> _animations = [];
    public SpriteAnimation CurrentAnimation { get; private set; }
    public SpriteAnimation NextAnimation { get; private set; }
    public int CurrentFrame { get; private set; }
    public Texture2D CurrentTexture;
    public bool IsPaused { get; private set; }
    
    private bool _queuedPause;
    private int _frameCounter;
    private string _currentAnimationKey;
    private bool _waitForNextFrame;
    private bool _resetIndexOnSwitch;

    public void AddAnimation(SpriteAnimation animation)
    {
        ArgumentNullException.ThrowIfNull(animation);
        _animations.Add(animation.Name, animation);
    }
    
    public void Update(GameTime gameTime)
    {
        if (CurrentAnimation == null || IsPaused)
        {
            return;
        }
        
        _frameCounter++;

        if (_frameCounter < 2)
        {
            return;
        }
        
        _frameCounter = 0;
        CurrentFrame = (CurrentFrame + 1) % CurrentAnimation.Duration;
        
        if (_queuedPause)
        {
            IsPaused = true;
        }

        foreach (var keyFrame in CurrentAnimation.KeyFrames)
        {
            if (keyFrame.Frame == CurrentFrame)
            {
                CurrentTexture = CurrentAnimation.Textures[keyFrame.TextureIndex];
                TriggerEventsForCurrentFrame();
                return;
            }
        }
    }

    private void TriggerEventsForCurrentFrame()
    {
        foreach (var keyFrame in CurrentAnimation.KeyFrames)
        {
            if (keyFrame.Frame == CurrentFrame && keyFrame.Events != null)
            {
                foreach (var eventName in keyFrame.Events)
                {
                    if (CurrentAnimation.EventDictionary.TryGetValue(eventName, out var dataEvent))
                    {
                        var eventHandler = (DataEventHandler)serviceProvider.GetService(typeof(DataEventHandler));
                        eventHandler?.Publish(dataEvent);
                    }
                }
            }
        }
    }
    
    public void Play(string key, bool waitForNextFrame = false, bool resetIndexOnSwitch = true)
    {
        if (!_animations.TryGetValue(key, out var value))
        {
            throw new KeyNotFoundException();
        }
        _currentAnimationKey = key;
        SetCurrentAnimation(value, waitForNextFrame, resetIndexOnSwitch);
    }
    
    public void Pause()
    {
        _queuedPause = true;
    }

    public void Resume()
    {
        IsPaused = false;
    }
    
    private void SetCurrentAnimation(SpriteAnimation animation, bool waitForNextFrame = false, bool resetIndexOnSwitch = true)
    {
        ArgumentNullException.ThrowIfNull(animation);

        if (CurrentAnimation == animation)
        {
            if (IsPaused)
            {
                Resume();
            }
            return;
        }

        if (!_animations.ContainsValue(animation))
        {
            throw new InvalidOperationException("animation must be added before setting it as current.");
        }

        _waitForNextFrame = waitForNextFrame;
        _resetIndexOnSwitch = resetIndexOnSwitch;

        if (!waitForNextFrame)
        {
            ApplyAnimationChange(animation);
        }
        else
        {
            NextAnimation = animation;
        }
    }
    
    private void ApplyAnimationChange(SpriteAnimation animation)
    {
        CurrentAnimation = animation;
        
        if (_resetIndexOnSwitch)
        {
            CurrentFrame = 0;
        }
        else if (CurrentFrame >= CurrentAnimation.Duration)
        {
            CurrentFrame %= CurrentAnimation.Duration;
        }
    }
}