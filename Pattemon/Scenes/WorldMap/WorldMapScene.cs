using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PatteLib.World;
using Pattemon.Engine;

namespace Pattemon.Scenes.WorldMap;

public class WorldMapScene : SceneA
{
    
    private MatrixData _worldMatrix;
    
    public WorldMapScene(Game game, object args = null, string contentDirectory = "Content") : base(game, args, contentDirectory)
    {
    }

    public override bool Init()
    {
        // load map matrix
        _worldMatrix = MatrixData.LoadFromFile(@$"Content/WorldData/Matrices/0.json");
        // load text for each town or route
        // load textures for everything that is being drawn
        return true;
    }

    
    public override bool Exit()
    {
        // unload all assets
        // free memory
        return true;
    }
    
    public override bool Update(GameTime gameTime, float delta)
    {
        // state handling
        // transtion
        // wait for transition
        // main
            // react on inputs
        // transition
        return false;
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
        
    }
}