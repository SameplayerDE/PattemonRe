using System;
using Microsoft.Xna.Framework;

namespace Pattemon.Engine;

public static class TransitionCore
{
    public static float TransitionProgress { private set; get; }
    private static bool _isTransitioning;
    private static int _transitionSpeed;
    // transition

    public static void Init()
    {
        
    }
    
    public static bool IsScreenTransitionDone()
    {
        return !_isTransitioning;
    }

    public static void StartTransition(int duration)
    {
        // transition = transition
        _isTransitioning = true;
        TransitionProgress = 0;
        _transitionSpeed = 1000 / duration;
    }
    
    public static void Update(GameTime gameTime, float delta)
    {
        if (!_isTransitioning)
        {
            return;
        }
        
        if (TransitionProgress < 1f)
        {
            TransitionProgress = Math.Min(TransitionProgress + delta * _transitionSpeed, 1.0f);
            // transition.Update(progress)
        }
        else
        {
            _isTransitioning = false;
        }
    }

    public static void Draw()
    {
        // transition.Draw()
    }
}