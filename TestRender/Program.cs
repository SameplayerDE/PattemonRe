using System;
using TestRender;
using Utils = PatteLib.Utils;

namespace TestRendering;

public static class Program
{
    [STAThread]
    private static void Main()
    {
        
        Console.WriteLine(Utils.Q412ToDouble(1473));
        Console.WriteLine(Q4_12_ToFloat(1473));
        Console.WriteLine(Q4_12_ToFloat(2731713));
        Console.WriteLine(Q4_12_ToFloat(-10750));
        Console.WriteLine(Utils.Q412ToDouble(-10750));
        
        using var game = new Game1();
        game.Run();
    }
    
    static float Q4_12_ToFloat( int fx ) {
        return (float)fx / (float)(1 << 12);
    }
}