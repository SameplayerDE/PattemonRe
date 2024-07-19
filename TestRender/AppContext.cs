using System.Collections.Generic;
using System.Diagnostics;
using HxGLTF.Implementation;
using PatteLib.Gameplay;
using PatteLib.Graphics;

namespace TestRender;

public static class AppContext
{
    public static Dictionary<int, SpriteCollection> OverWorldSprites = [];
    public static Dictionary<int, GameModel> OverWorldModels = [];

    public static int CurrentHeaderId;
    public static int CurrentChunkId;
    public static EventContainer CurrentEventContainer;
}