using System;
using System.IO;
using PatteLib.Gameplay.Scripting;
using Pattemon;
using Pattemon.Data;


var processor = new ScriptProcessor();
processor.ParseScript(File.ReadAllLines("Content/Scripts/501.sk"));
processor.ExecuteSection("Script 1");

using var game = new PatteGame();
game.Run();