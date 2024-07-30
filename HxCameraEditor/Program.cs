using System;
using PatteLib;

Console.WriteLine((float)NitroUtils.Fx32ToDecimal(2731713));
Console.WriteLine((uint)NitroUtils.DecimalToFx32((decimal)666.9221));

using var game = new HxCameraEditor.Game1();
game.Run();