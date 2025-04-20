using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace Pattemon.Engine;

public static class AudioCore
{
    private static readonly Dictionary<int, Song> _songs = new();
    private static readonly Dictionary<int, SoundEffect> _soundEffects = new();

    public static void LoadSong(ContentManager content, int id, string path)
    {
        if (!_songs.ContainsKey(id))
        {
            _songs[id] = content.Load<Song>(path);
        }
    }
    
    public static void PlaySong(int id, bool loop = true)
    {
        if (!_songs.TryGetValue(id, out var song))
        {
            return;
        }
        MediaPlayer.IsRepeating = loop;
        MediaPlayer.Play(song);
    }
}