using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TrainerCase;

public class SpriteAnimation
{
    private Texture2D _spriteSheet;
    private readonly int _frameWidth;
    private readonly int _frameHeight;
    private readonly int _frameCount;
    private readonly float _frameTime;
    
    private int _currentFrame;
    private float _timer;
    private bool _loop = true;
    
    public bool IsPlaying { get; private set; } = true;
    
    public SpriteAnimation(Texture2D spriteSheet, int frameWidth, int frameHeight, int frameCount, float frameTime)
    {
        _spriteSheet = spriteSheet ?? throw new ArgumentNullException(nameof(spriteSheet));
        _frameWidth = frameWidth;
        _frameHeight = frameHeight;
        _frameCount = frameCount;
        _frameTime = frameTime;
    }

    public void SetLoop(bool loop)
    {
        _loop = loop;
    }
    
    public void Play()
    {
        IsPlaying = true;
        _currentFrame = 0;
        _timer = 0;
    }
    
    public void Pause()
    {
        IsPlaying = false;
    }
    
    public void Stop()
    {
        IsPlaying = false;
        _currentFrame = 0;
        _timer = 0;
    }
    
    public void Update(GameTime gameTime)
    {
        if (!IsPlaying)
        {
            return;
        }

        _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (!(_timer >= _frameTime))
        {
            return;
        }
        
        _timer -= _frameTime;
        var tempFrame = _currentFrame;
        _currentFrame = (_currentFrame + 1) % _frameCount;
        if (_loop)
        {
            return;
        }
        if (_currentFrame < tempFrame)
        {
            Stop();
        }
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color)
    {
        int row = _currentFrame / (_spriteSheet.Width / _frameWidth);
        int column = _currentFrame % (_spriteSheet.Width / _frameWidth);

        var sourceRectangle = new Rectangle(column * _frameWidth, row * _frameHeight, _frameWidth, _frameHeight);
        spriteBatch.Draw(_spriteSheet, position, sourceRectangle, color);
    }
}