using Microsoft.Xna.Framework;
using Pattemon.Engine;

namespace Pattemon.Scenes.PoketchApps;

public abstract class PoketchApp(Game game, object args = null, string contentDirectory = "Content") : SceneA(game, args, contentDirectory);