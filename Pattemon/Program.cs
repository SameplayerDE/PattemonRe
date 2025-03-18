using System;
using System.IO;
using PatteLib.Gameplay.Scripting;
using Pattemon.Gameplay.Scripting.Commands;

CommandFactory.RegisterCommand("LockAll", args =>
{
    if (args.Length == 0)
    {
        return new LockAllCommand();
    }
    throw new Exception();
});

CommandFactory.RegisterCommand("ReleaseAll", args =>
{
    if (args.Length == 0)
    {
        return new ReleaseAllCommand();
    }
    throw new Exception();
});

using var game = new Pattemon.PatteGame();
game.Run();