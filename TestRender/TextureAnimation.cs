using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TestRender;

public class TextureAnimation
{
    private string _basePath;
    private string _fileName;
    public string ForMaterial;
    public List<Texture2D> Frames = [];
    public TimeSpan FrameDuration;
    public TimeSpan AnimationTimer;
    public int FrameIndex = 0;
    public int FramesCount;
    
    public Texture2D CurrentFrame => Frames[FrameIndex];

    public TextureAnimation(string path, string fileName, float duration, int count, string @for)
    {
        ForMaterial = @for;
        _basePath = path;
        _fileName = fileName;
        FrameDuration = TimeSpan.FromSeconds(duration);
        FramesCount = count;
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
        AnimationTimer += gameTime.ElapsedGameTime;
        if (AnimationTimer <= FrameDuration) return;
        AnimationTimer -= FrameDuration;
        FrameIndex = (FrameIndex + 1) % FramesCount;
    }
}