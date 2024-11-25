using System;
using Microsoft.Xna.Framework;
using PatteLib;
using PatteLib.Data;
using TestRender;
using Utils = PatteLib.Utils;

namespace TestRendering;

public static class Program
{
    [STAThread]
    private static void Main()
    {
        //ExperienceTableHandler.LoadData(@"Content\Pokemon\experiences.json");
        //string expRate = "EXP_RATE_MEDIUM_SLOW";
        //int currExp = 10233;
        //int currLevel = ExperienceTableHandler.GetLevel(expRate, currExp);
        //int totalExpNext = ExperienceTableHandler.GetExp(expRate, currLevel + 1);
        //int expToNext = totalExpNext - currExp;
//
        //Console.WriteLine(currLevel);
        //Console.WriteLine($"{currExp} / {totalExpNext} ({expToNext})");
        
        using var game = new Game1();
        game.Run();
    }
}