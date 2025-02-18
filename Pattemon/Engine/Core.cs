using InputLib;
using Microsoft.Xna.Framework;

namespace Pattemon.Engine;

public class Core
{

    public const float TargetFramesPerSecond = 30f;
    
    public static float GetDelta(float delta)
    {
        return delta / (1f / TargetFramesPerSecond);
    }
    
    public static float ToFrames(float ms)
    {
        return TargetFramesPerSecond * ms;
    }
    
    public static void ReadInput()
    {
        KeyboardHandler.Update();
        GamepadHandler.Update();
        MouseHandler.Update();
    }
}