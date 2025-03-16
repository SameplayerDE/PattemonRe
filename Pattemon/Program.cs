using System;
using System.IO;
using PatteLib.Gameplay.Scripting;
using Pattemon;
using Pattemon.Data;
using Pattemon.Engine;

LanguageCore.Load("field_menu");
Console.WriteLine(LanguageCore.Get("field_menu", "bag"));
Console.WriteLine(LanguageCore.Get("field_menu", "menu.name"));

//var processor = new ScriptProcessor();
//processor.ParseScript(File.ReadAllLines("Content/Scripts/501.sk"));
//processor.ExecuteSection("Script 1");
//
//using var game = new PatteGame();
//game.Run();