using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pattemon.Engine;

public static class DualScreenCore
{
    public static RenderTarget2D UpperScreen { private set; get; }
    public static RenderTarget2D LowerScreen { private set; get; }
    
    public static bool IsSwappingScreens;
    public static float TransitionProgress { private set; get; }
    private const float _transitionSpeed = 2.0f;

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
                //if (_isBottomScreenFocus)
                //{
                //    _bottomScreenRectangle.X = (int)MathHelper.Lerp(_unfocusScreenRectangle.X, _focusScreenRectangle.X, _transitionProgress);
                //    _bottomScreenRectangle.Y = (int)MathHelper.Lerp(_unfocusScreenRectangle.Y, _focusScreenRectangle.Y, _transitionProgress);
                //}
                //else
                //{
                //    _bottomScreenRectangle.X = (int)MathHelper.Lerp(_focusScreenRectangle.X, _unfocusScreenRectangle.X, _transitionProgress);
                //    _bottomScreenRectangle.Y = (int)MathHelper.Lerp(_focusScreenRectangle.Y, _unfocusScreenRectangle.Y, _transitionProgress);
                //}
            }
            else
            {
                IsSwappingScreens = false;
            }
        }
    }

    public static void SwapScreens()
    {
        if (!IsSwappingScreens)
        {
            TransitionProgress = 0f;
            IsSwappingScreens = true;
        }
    }
}