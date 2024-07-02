using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TestRender;

public enum AnimationPlayMode
{
    Linear,
    Bounce,
    EaseIn
}

public class TextureAnimation
{
    private string _basePath;
    private string _fileName;
    public string[] ForMaterial;
    public List<Texture2D> Frames = new List<Texture2D>();
    public TimeSpan FrameDuration;
    public TimeSpan BounceWaitDuration;
    public TimeSpan AnimationTimer;
    public TimeSpan BounceWaitTimer;
    public int FrameIndex = 0;
    public int FramesCount;
    public AnimationPlayMode PlayMode;
    private bool _isReversing = false;
    private bool _isWaiting = false;

    public Texture2D CurrentFrame => Frames[FrameIndex];

    public TextureAnimation(string path, string fileName, float frameDuration, int count, string[] @for, AnimationPlayMode playMode = AnimationPlayMode.Linear, float bounceWaitDuration = 0.5f)
    {
        ForMaterial = @for;
        _basePath = path;
        _fileName = fileName;
        FrameDuration = TimeSpan.FromSeconds(frameDuration);
        BounceWaitDuration = TimeSpan.FromSeconds(bounceWaitDuration);
        FramesCount = count;
        PlayMode = playMode;
    }

    public void LoadContent(ContentManager contentManager)
    {
        for (var i = 0; i < FramesCount; i++)
        {
            var texturePath = $@"{_basePath}\{_fileName}_{i + 1}";
            Frames.Add(contentManager.Load<Texture2D>(texturePath));
        }
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