using InputLib;
using Microsoft.Xna.Framework;

namespace Pattemon.Engine;

public class Core
{

    public static float GetDelta(float delta)
    {
        return delta / (1f / 30f);
    }
    
    public static void ReadInput()
    {
        KeyboardHandler.Update();
        MouseHandler.Update();
    }
}