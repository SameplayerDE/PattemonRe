using Microsoft.Xna.Framework.Input;

namespace InputLib;

public static class InputHandler
{
    internal static bool IsLocked = false;
    internal static bool IsWaitingForInput = false;
    internal static bool IsWaitingForButton = false;
    internal static bool IsWaitingForKey = false;
    internal static Buttons WaitButton;
    internal static Keys WaitKey;
    
    public static void Lock()
    {
        IsLocked = true;
    }
    
    public static void Release()
    {
        IsLocked = false;
    }

    public static void WaitFor(Keys key)
    {
        if (IsWaitingForInput)
        {
            return;
        }
        IsWaitingForInput = true;
        IsWaitingForKey = true;
        WaitKey = key;
    }
    
    public static void WaitFor(Buttons button)
    {
        if (IsWaitingForInput)
        {
            return;
        }
        IsWaitingForInput = true;
        IsWaitingForButton = true;
        WaitButton = button;
    }

    public static void ClearWait()
    {
        IsWaitingForInput = false;
        IsWaitingForButton = false;
        IsWaitingForKey = false;
    }
    
    public static void Update()
    {
        if (IsWaitingForInput)
        {
            if (IsWaitingForKey && KeyboardHandler.IsKeyDownOnce(WaitKey))
            {
                ClearWait();
            }
            else if (IsWaitingForButton && GamepadHandler.IsButtonDownOnce(WaitButton))
            {
                ClearWait();
            }
            return;
        }
        
        MouseHandler.Update();
        KeyboardHandler.Update();
        GamepadHandler.Update();
    }
}