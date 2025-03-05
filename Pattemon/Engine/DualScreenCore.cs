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
    
    public static bool IsDisabled { private set; get; }

    public static void Enable()
    {
        IsDisabled = false;
    }

    public static void Disable()
    {
        IsDisabled = true;
    }
    
    public static void Init()
    {
    }
    
   //public static Rectangle GetScaledRectangle(Rectangle original)
   //{
   //    float scaleX = GetScaleX();
   //    float scaleY = GetScaleY();

   //    int scaledX = (int)(original.X * scaleX);
   //    int scaledY = (int)(original.Y * scaleY);
   //    int scaledWidth = (int)(original.Width * scaleX);
   //    int scaledHeight = (int)(original.Height * scaleY);

   //    return new Rectangle(scaledX, scaledY, scaledWidth, scaledHeight);
   //}
    
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

    public static void SwapScreens() // 0 = default, 1 = to top, 2 = to bottom
    {
        SwapScreens(SwapDefault, 2.0f);
    }
    
    public static void SwapScreens(int type) // 0 = default, 1 = to top, 2 = to bottom
    {
        SwapScreens(type, 2.0f);
    }
    
    public static void SwapScreens(float speed) // 0 = default, 1 = to top, 2 = to bottom
    {
        SwapScreens(SwapDefault, speed);
    }
    
    public static void SwapScreens(int type, float speed) // 0 = default, 1 = to top, 2 = to bottom
    {
        if (IsDisabled)
        {
            return;
        }
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