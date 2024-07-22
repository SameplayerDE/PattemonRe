using System;
using Microsoft.Xna.Framework;
using PatteLib;
using TestRender;
using Utils = PatteLib.Utils;

namespace TestRendering;

public static class Program
{
    [STAThread]
    private static void Main()
    {
        Console.WriteLine(MathHelper.ToDegrees((float)NitroUtils.Fx32ToDecimal(1473))); //
        //Console.WriteLine(NitroUtils.Fx32ToDecimal(5377)); //
        //Console.WriteLine(NitroUtils.DecimalToFx32((decimal)MathHelper.ToRadians(180)));
        //Console.WriteLine(NitroUtils.DecimalToFx32(0));
        //Console.WriteLine((short)NitroUtils.GetU16IntFromAngle(180));
        //Console.WriteLine((short)NitroUtils.GetU16IntFromAngle(270));
        //Console.WriteLine((short)NitroUtils.GetU16IntFromAngle(360));
        //Console.WriteLine((uint)NitroUtils.DecimalToFx32(111));
        using var game = new Game1();
        game.Run();
    }
}