using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace Pattemon.Audio;

public class AudioCore
{
    public static int Mode = AudioMode.Stereo;
    public static int Volume = 100;

    private static List<SoundEffect> _soundEffects = [];
    private static Song _currentSong;
    private static Song _nextSong;

    public static void Init(ContentManager content)
    {
        //_soundEffects[0x00] = content.Load<SoundEffect>("dummy");
    }

    public static void Update(GameTime gameTime)
    {
        if (_currentSong == null)
        {
            if (_nextSong == null)
            {
                return;
            }
            _currentSong = _nextSong;
            _nextSong = null;
        }
    }
    
    public static void PlaySound(int soundId, int volume = 100)
    {
        // play a sound at given volume
        
    }

    public static bool LoadMusic(int soundId)
    {
        // loads the next song
        return false;
    }

    public static bool FadeMusicOut(int speed)
    {
        // fade the music
        return false;
    }
    
    public static void StartMusic(int soundId)
    {
        // play a sound at given volume
    }
}