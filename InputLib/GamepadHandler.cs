using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace InputLib;

public static class GamepadHandler
{
    private static GamePadState _currGamePadState;
    private static GamePadState _prevGamePadState;

    public static void Update()
    {
        _prevGamePadState = _currGamePadState;
        _currGamePadState = GamePad.GetState(PlayerIndex.One);
    }

    public static bool IsButtonDownOnce(Buttons button)
    {
        return _currGamePadState.IsButtonDown(button) && _prevGamePadState.IsButtonUp(button);
    }

    public static bool IsButtonDown(Buttons button)
    {
        return _currGamePadState.IsButtonDown(button);
    }
    
    public static Vector2 GetLeftStick()
    {
        return _currGamePadState.ThumbSticks.Left;
    }

    public static Vector2 GetRightStick()
    {
        return _currGamePadState.ThumbSticks.Right;
    }

    public static float GetLeftTrigger()
    {
        return _currGamePadState.Triggers.Left;
    }

    public static float GetRightTrigger()
    {
        return _currGamePadState.Triggers.Right;
    }
}