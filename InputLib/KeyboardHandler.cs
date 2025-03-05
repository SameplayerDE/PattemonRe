using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace InputLib;

public static class KeyboardHandler
{
    private static KeyboardState _currKeyboardState;
    private static KeyboardState _prevKeyboardState;

    internal static void Update()
    {
        _prevKeyboardState = _currKeyboardState;
        _currKeyboardState = Keyboard.GetState();
    }

    public static bool IsKeyDownOnce(Keys key)
    {
        return _currKeyboardState.IsKeyDown(key) && _prevKeyboardState.IsKeyUp(key);
    }

    public static bool IsKeyDown(Keys key)
    {
        return _currKeyboardState.IsKeyDown(key);
    }
    
    public static Vector3 GetDirection()
    {
        var direction = Vector3.Zero;

        if (KeyboardHandler.IsKeyDown(Keys.Q))
        {
            direction += Vector3.Down;
        }
        else if (KeyboardHandler.IsKeyDown(Keys.E))
        {
            direction += Vector3.Up;
        }

        if (KeyboardHandler.IsKeyDown(Keys.W))
        {
            direction += Vector3.Forward;
        }
        else if (KeyboardHandler.IsKeyDown(Keys.S))
        {
            direction += Vector3.Backward;
        }
        
        if (KeyboardHandler.IsKeyDown(Keys.A))
        {
            direction += Vector3.Left;
        }
        else if (KeyboardHandler.IsKeyDown(Keys.D))
        {
            direction += Vector3.Right;
        }
        
        if (direction.Length() != 0)
        {
            direction.Normalize();
        }
        
        return direction;
    }
    
}