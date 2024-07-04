using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace TestRender;

public class Music
{
    public int Id;
    public string Name;
    public Song Song;
    public TimeSpan LoopStart = TimeSpan.Zero;
    public TimeSpan End = TimeSpan.MaxValue;

    private bool _shouldLoop;
    
    public Music(int id, string name)
    {
        Name = name;
    }

    public void LoadContent(ContentManager contentManager)
    {
        Song = contentManager.Load<Song>(Name);
        if (End.Duration() > Song.Duration)
        {
            End = Song.Duration;
        }
    }
    
    public void Play()
    {
        MediaPlayer.Play(Song);
    }

    public void Update(GameTime gameTime)
    {
        if (MediaPlayer.PlayPosition >= End)
        {
            MediaPlayer.Play(Song, LoopStart);
        }
    }

    public void Stop()
    {
        MediaPlayer.Stop();
    }
}