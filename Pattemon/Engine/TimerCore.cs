using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Pattemon.Engine;

public static class TimerCore
{
    private class Timer
    {
        public readonly float Duration;
        public float Elapsed;
        public float Progress;
        public bool IsRunning;

        public Timer(float duration)
        {
            Duration = duration;
            Elapsed = 0f;
            Progress = Elapsed;
            IsRunning = true;
        }
    }
    
    private static readonly Dictionary<string, Timer> _timers = [];

    public static float Get(string key)
    {
        _timers.TryGetValue(key, out var timer);
        return timer?.Progress ?? 0f;
    }
    
    public static void Create(string key, float timeInMs)
    {
        _timers[key] = new Timer(timeInMs);
    }
    
    public static bool IsDone(string key)
    {
        return _timers.TryGetValue(key, out var timer) && timer.Progress >= 1f;
    }
    
    public static void Update(GameTime gameTime)
    {
        float delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        
        foreach (var timer in _timers.Values)
        {
            if (timer.IsRunning)
            {
                timer.Elapsed += delta;
                timer.Progress = timer.Elapsed / timer.Duration;
            }
        }
    }
    
    public static void Reset(string key)
    {
        if (_timers.TryGetValue(key, out var timer))
        {
            timer.Elapsed = 0;
            timer.IsRunning = true;
        }
    }
    
    public static void Remove(string key)
    {
        _timers.Remove(key);
    }
}