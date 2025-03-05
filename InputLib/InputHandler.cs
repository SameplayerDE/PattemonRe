namespace InputLib;

public static class InputHandler
{
    public static void Update()
    {
        MouseHandler.Update();
        KeyboardHandler.Update();
        GamepadHandler.Update();
    }
}