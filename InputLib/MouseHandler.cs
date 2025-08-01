﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace InputLib;

public enum MouseButton
{
    Left, Middle, Right
}

public static class MouseHandler
{

    private static MouseState _currMouseState;
    private static MouseState _prevMouseState;

    public static Point Position => _currMouseState.Position;
    
    internal static void Update()
    {
        if (InputHandler.IsLocked)
        {
            return;
        }
        _prevMouseState = _currMouseState;
        _currMouseState = Mouse.GetState();
    }

    public static bool IsButtonDown(MouseButton button)
    {
        if (button == MouseButton.Left)
        {
            return _currMouseState.LeftButton == ButtonState.Pressed;
        }
        if (button == MouseButton.Middle)
        {
            return _currMouseState.MiddleButton == ButtonState.Pressed;
        }
        if (button == MouseButton.Right)
        {
            return _currMouseState.RightButton == ButtonState.Pressed;
        }

        return false;
    }
    
    public static bool WasButtonDown(MouseButton button)
    {
        if (button == MouseButton.Left)
        {
            return _prevMouseState.LeftButton == ButtonState.Pressed;
        }
        if (button == MouseButton.Middle)
        {
            return _prevMouseState.MiddleButton == ButtonState.Pressed;
        }
        if (button == MouseButton.Right)
        {
            return _prevMouseState.RightButton == ButtonState.Pressed;
        }

        return false;
    }
    
    public static bool IsButtonDownOnce(MouseButton button)
    {
        return IsButtonDown(button) && !WasButtonDown(button);
    }

    public static bool IsAreaOnce(MouseButton button, Rectangle area)
    {
        return IsButtonDownOnce(button) && area.Contains(_currMouseState.Position);
    }
    
    public static Point GetMouseDelta()
    {
        return new Point(_currMouseState.X - _prevMouseState.X, _currMouseState.Y - _prevMouseState.Y);
    }
    
    public static int GetMouseWheelValue()
    {
        return _currMouseState.ScrollWheelValue;
    }
    
    public static int GetMouseWheelValueDelta()
    {
        return _currMouseState.ScrollWheelValue - _prevMouseState.ScrollWheelValue;;
    }

}