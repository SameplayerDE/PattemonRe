using InputLib;
using Microsoft.Xna.Framework;

namespace Pattemon.Engine;

public class Core
{
    public static void ReadInput()
    {
        KeyboardHandler.Update();
        MouseHandler.Update();
    }
}