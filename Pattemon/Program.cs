using System.IO;
using PatteLib.Gameplay.Scripting;
using Pattemon;


var processor = new ScriptProcessor();
processor.ParseScript(File.ReadAllLines("Content/Scripts/501.sk"));
processor.ExecuteSection("Script 1");

using var game = new Game1();
game.Run();