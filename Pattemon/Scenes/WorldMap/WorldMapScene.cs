using System;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PatteLib.Data;
using PatteLib.World;
using Pattemon.Engine;

namespace Pattemon.Scenes.WorldMap;

public class WorldMapScene : SceneA
{
    
    private MatrixData _worldMatrix;
    private HeaderManager _headerManager;
    private int _x;
    private int _y;
    
    public WorldMapScene(Game game, object args = null, string contentDirectory = "Content") : base(game, args, contentDirectory)
    {
    }

    public override bool Init()
    {
        // load map matrix
        _worldMatrix = MatrixData.LoadFromFile(@$"Content/WorldData/Matrices/0.json");
        _headerManager = _services.GetService<HeaderManager>();
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
        ProcessInput();
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
    
    #region

    private void ProcessInput()
    {
        var positionChanged = false;
        if (KeyboardHandler.IsKeyDownOnce(Keys.Left))
        {
            _x--;
            positionChanged = true;
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.Right))
        {
            _x++;
            positionChanged = true;
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.Up))
        {
            _y--;
            positionChanged = true;
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.Down))
        {
            _y++;
            positionChanged = true;
        }
        if (positionChanged)
        {
            _x = Math.Clamp(_x, 0, _worldMatrix.Width - 1);
            _y = Math.Clamp(_y, 0, _worldMatrix.Height - 1);
            
            var cellData = _worldMatrix.Get(_x, _y);
            if (!Equals(cellData, MatrixCellData.Empty))
            {
                var header = _headerManager.GetHeaderById(cellData.HeaderId);
                Console.WriteLine(header.LocationName);
            }
        }
    }
    
    #endregion
}