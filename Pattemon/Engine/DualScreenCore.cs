using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pattemon.Engine;

public abstract class SwapTransition
{
    protected Rectangle FocusScreenRectangle;
    protected Rectangle UnfocusScreenRectangle;
    protected Rectangle TopScreenRectangle;
    protected Rectangle BottomScreenRectangle;
    public abstract void Update(GameTime gameTime, float delta);
}

public static class DualScreenCore
{
    public const int SwapDefault = 0;
    public const int SwapTop = 1;
    public const int SwapBottom = 2;
    
    public static bool IsSwappingScreens { private set; get; }
    public static bool IsBottomScreenInFocus { private set; get; }
    public static float TransitionProgress { private set; get; }
    private static float _transitionSpeed = 2.0f;
    
    public static Rectangle FocusScreenRectangle;
    public static Rectangle TopScreenRectangle;
    
    public static Rectangle UnfocusScreenRectangle;
    public static Rectangle BottomScreenRectangle;
    
    // the actual current position and scale of the screen
    
    public static void Init()
    {
    }
    
    public static void Update(GameTime gameTime, float delta)
    {
        if (IsSwappingScreens)
        {
            if (TransitionProgress < 1.0f)
            {
                TransitionProgress = Math.Min(TransitionProgress + delta * _transitionSpeed, 1.0f);
                if (IsBottomScreenInFocus)
                {
                    BottomScreenRectangle.X = (int)MathHelper.Lerp(UnfocusScreenRectangle.X, FocusScreenRectangle.X, TransitionProgress);
                    BottomScreenRectangle.Y = (int)MathHelper.Lerp(UnfocusScreenRectangle.Y, FocusScreenRectangle.Y, TransitionProgress);
                    BottomScreenRectangle.Width = (int)MathHelper.Lerp(UnfocusScreenRectangle.Width, FocusScreenRectangle.Width, TransitionProgress);
                    BottomScreenRectangle.Height = (int)MathHelper.Lerp(UnfocusScreenRectangle.Height, FocusScreenRectangle.Height, TransitionProgress);
                    
                    //TopScreenRectangle.X = (int)MathHelper.Lerp(FocusScreenRectangle.X, UnfocusScreenRectangle.X, TransitionProgress);
                    //TopScreenRectangle.Y = (int)MathHelper.Lerp(FocusScreenRectangle.Y, UnfocusScreenRectangle.Y, TransitionProgress);
                    //TopScreenRectangle.Width = (int)MathHelper.Lerp(FocusScreenRectangle.Width, UnfocusScreenRectangle.Width, TransitionProgress);
                    //TopScreenRectangle.Height = (int)MathHelper.Lerp(FocusScreenRectangle.Height, UnfocusScreenRectangle.Height, TransitionProgress);
                }
                else
                {
                    //TopScreenRectangle.X = (int)MathHelper.Lerp(UnfocusScreenRectangle.X, FocusScreenRectangle.X, TransitionProgress);
                    //TopScreenRectangle.Y = (int)MathHelper.Lerp(UnfocusScreenRectangle.Y, FocusScreenRectangle.Y, TransitionProgress);
                    //TopScreenRectangle.Width = (int)MathHelper.Lerp(UnfocusScreenRectangle.Width, FocusScreenRectangle.Width, TransitionProgress);
                    //TopScreenRectangle.Height = (int)MathHelper.Lerp(UnfocusScreenRectangle.Height, FocusScreenRectangle.Height, TransitionProgress);
                    
                    BottomScreenRectangle.X = (int)MathHelper.Lerp(FocusScreenRectangle.X, UnfocusScreenRectangle.X, TransitionProgress);
                    BottomScreenRectangle.Y = (int)MathHelper.Lerp(FocusScreenRectangle.Y, UnfocusScreenRectangle.Y, TransitionProgress);
                    BottomScreenRectangle.Width = (int)MathHelper.Lerp(FocusScreenRectangle.Width, UnfocusScreenRectangle.Width, TransitionProgress);
                    BottomScreenRectangle.Height = (int)MathHelper.Lerp(FocusScreenRectangle.Height, UnfocusScreenRectangle.Height, TransitionProgress);
                }
            }
            else
            {
                IsSwappingScreens = false;
            }
        }
    }

    public static void SwapScreens(float speed = 2.0f, int type = 0) // 0 = default, 1 = to top, 2 = to bottom
    {
        if (!IsSwappingScreens)
        {
            _transitionSpeed = speed;
            if (type == 1 && IsBottomScreenInFocus)
            {
                type = 0;
            }
            if (type == 2 && !IsBottomScreenInFocus)
            {
                type = 0;
            }
            
            if (type == 0)
            {
                IsBottomScreenInFocus = !IsBottomScreenInFocus;
                TransitionProgress = 0f;
                IsSwappingScreens = true;
            }
        }
    }
}