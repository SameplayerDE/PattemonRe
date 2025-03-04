using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pattemon.PoketchApps.Calculator;

public class CalculatorPoketchApp(Game game, object args = null, string contentDirectory = "Content") : PoketchApp(game, args, contentDirectory)
{
    private CalculationOperator _operator = CalculationOperator.None;
    private string _inputBuffer;
    
    private Stack<CalculationStep> _steps = [];
    
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
        // process touch
        
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
        throw new System.NotImplementedException();
    }

    private void AppendDigit(char digit)
    {
        if (digit is < '0' or > '9') return; // Nur Ziffern erlauben

        if (_inputBuffer.Length == 0 && digit == '0') return; // Keine führenden Nullen

        if (_inputBuffer.Length < 10) // Maximal 10 Zeichen erlaubt
        {
            _inputBuffer += digit;
        }
    }
    
    private void AppendDecimalPoint()
    {
        if (_inputBuffer.Contains('.') || _inputBuffer.Length >= 9) return; // Nur 1 Dezimalpunkt erlaubt

        _inputBuffer += ".";
    }
    
    private void OperatorPressed(CalculationOperator calculationOperator)
    {
        _steps.Push(new CalculationStep
        {
            Value = double.Parse(_inputBuffer),
            Operator = calculationOperator,
        });
    }
}