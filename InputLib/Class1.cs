using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace InputLib;

public static class KeyboardHandler
{
    private static KeyboardState _currKeyboardState;
    private static KeyboardState _prevKeyboardState;

    public static void Update(GameTime gameTime)
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
}