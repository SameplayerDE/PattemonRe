using System;
using System.Collections.Generic;
using System.Globalization;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pattemon.PoketchApps.Calculator;

public class CalculatorPoketchApp(Game game, object args = null, string contentDirectory = "Content") : PoketchApp(game, args, contentDirectory)
{
    private CalculationOperator _operator = CalculationOperator.None;
    private string _inputBuffer = string.Empty;
    private double _inMemory;
    private bool _inputIsResult = false;
    
    public override bool Init()
    {
        //throw new System.NotImplementedException();
        return true;
    }

    public override bool Exit()
    {
        //throw new System.NotImplementedException();
        return true;
    }

    public override void Process(GameTime gameTime, float delta)
    {
        // process touch
        Console.WriteLine(_inputBuffer);
        if (KeyboardHandler.IsKeyDownOnce(Keys.D0))
        {
            AppendDigit('0');
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.D1))
        {
            AppendDigit('1');
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.D2))
        {
            AppendDigit('2');
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.D3))
        {
            AppendDigit('3');
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.D4))
        {
            AppendDigit('4');
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.D5))
        {
            AppendDigit('5');
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.D6))
        {
            AppendDigit('6');
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.D7))
        {
            AppendDigit('7');
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.D8))
        {
            AppendDigit('8');
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.D9))
        {
            AppendDigit('9');
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.OemPeriod))
        {
            AppendFloatingPoint();
        }
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
        //throw new System.NotImplementedException();
        spriteBatch.Begin();
        
        spriteBatch.End();
    }

    private void AppendDigit(char digit)
    {
        if (digit is < '0' or > '9') return; // Nur Ziffern erlauben

        if (_inputIsResult)
        {
            _inputBuffer = string.Empty;
        }
        
        if (_inputBuffer.Length == 0 && digit == '0') return; // Keine führenden Nullen

        if (_inputBuffer.Length < 10) // Maximal 10 Zeichen erlaubt
        {
            _inputBuffer += digit;
        }
    }
    
    private void AppendFloatingPoint()
    {
        if (_inputBuffer.Contains('.') || _inputBuffer.Length >= 9) return; // Nur 1 Dezimalpunkt erlaubt

        _inputBuffer += ".";
    }
    
    private void OperatorPressed(CalculationOperator calculationOperator)
    {
        if (calculationOperator == CalculationOperator.Clear)
        {
            _operator = CalculationOperator.None;
            _inputBuffer = string.Empty;
            _inputIsResult = false;
            _inMemory = 0;
            return;
        }
        
        if (_operator != CalculationOperator.None)
        {
            switch (_operator)
            {
                case CalculationOperator.Addition:
                    _inMemory += double.Parse(_inputBuffer);
                    _inputBuffer = _inMemory.ToString(CultureInfo.InvariantCulture);
                    _inputIsResult = true;
                    break;
            }
        }
        _operator = calculationOperator;
    }
}