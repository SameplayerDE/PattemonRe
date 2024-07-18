using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PatteLib.Graphics;

namespace TestRender;

public enum AnimationPlayMode
{
    Linear,
    Bounce,
    EaseIn
}

public class TextureAnimation : SpriteCollection
{
    public string[] ForMaterial;
    public TimeSpan FrameDuration;
    public TimeSpan BounceWaitDuration;
    public TimeSpan AnimationTimer;
    public TimeSpan BounceWaitTimer;
    public int FrameIndex = 0;
    public int FramesCount => _count;
    public AnimationPlayMode PlayMode;
    private bool _isReversing = false;
    private bool _isWaiting = false;

    public Texture2D CurrentFrame => Sprites[FrameIndex];
    
    public TextureAnimation(IServiceProvider serviceProvider, string path, string fileName, float frameDuration, int count, string[] @for, AnimationPlayMode playMode = AnimationPlayMode.Linear, float bounceWaitDuration = 0.5f) : base(serviceProvider)
    {
        ForMaterial = @for;
        _basePath = path;
        _count = count;
        _fileName = fileName;
        FrameDuration = TimeSpan.FromSeconds(frameDuration);
        BounceWaitDuration = TimeSpan.FromSeconds(bounceWaitDuration);
        
        PlayMode = playMode;
    }

    public void Load()
    {
        Load(_basePath, _fileName, _count);
       //for (var i = 0; i < FramesCount; i++)
       //{
       //    var texturePath = $@"Animations\{_basePath}\{_fileName}_{i + 1}";
       //    Frames.Add(contentManager.Load<Texture2D>(texturePath));
       //}
    }

    public void Update(GameTime gameTime)
    {
        if (_isWaiting)
        {
            BounceWaitTimer += gameTime.ElapsedGameTime;
            if (BounceWaitTimer >= BounceWaitDuration)
            {
                _isWaiting = false;
                BounceWaitTimer = TimeSpan.Zero;
            }
            return;
        }

        AnimationTimer += gameTime.ElapsedGameTime;
        if (AnimationTimer <= FrameDuration) return;
        AnimationTimer -= FrameDuration;

        switch (PlayMode)
        {
            case AnimationPlayMode.Linear:
                FrameIndex = (FrameIndex + 1) % FramesCount;
                break;

            case AnimationPlayMode.Bounce:
                if (_isReversing)
                {
                    FrameIndex--;
                    if (FrameIndex <= 0)
                    {
                        FrameIndex = 0;
                        _isReversing = false;
                        _isWaiting = true;
                    }
                }
                else
                {
                    FrameIndex++;
                    if (FrameIndex >= FramesCount - 1)
                    {
                        FrameIndex = FramesCount - 1;
                        _isReversing = true;
                        _isWaiting = true;
                    }
                }
                break;

            case AnimationPlayMode.EaseIn:
                // Implement EaseIn logic if needed
                // For simplicity, using linear for now
                FrameIndex = (FrameIndex + 1) % FramesCount;
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}