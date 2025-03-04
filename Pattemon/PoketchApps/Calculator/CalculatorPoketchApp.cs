using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pattemon.PoketchApps.Calculator;

public class CalculatorPoketchApp(Game game, object args = null, string contentDirectory = "Content") : PoketchApp(game, args, contentDirectory)
{
    private CalculationOperator _operator;
    private int _inMemory;
    private int _onDisplay;
    
    public override bool Init()
    {
        throw new System.NotImplementedException();
    }

    public override bool Exit()
    {
        throw new System.NotImplementedException();
    }

    public override void Process(GameTime gameTime, float delta)
    {
        throw new System.NotImplementedException();
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
        throw new System.NotImplementedException();
    }
}