using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pattemon.Graphics;

namespace Pattemon.PoketchApps.DigitalClock;

public class DigitalClockPoketchApp(Game game, object args = null, string contentDirectory = "Content") : PoketchApp(game, args, contentDirectory)
{

    private const int _cellSize = 8;
    
    private Texture2D _background;
    private Texture2D _font;
    
    public override bool Init()
    {
       _background = GraphicsCore.LoadTexture("digitalClock_background", "Assets/poketch_digitalclock_background.png");
       _font = GraphicsCore.LoadTexture("digitalClock_font", "Assets/poketch_digitalclock_font.png");
       return true;
    }

    public override bool Exit()
    {
        GraphicsCore.FreeTexture("digitalClock_background");
        GraphicsCore.FreeTexture("digitalClock_font");
        return true;
    }

    public override void Process(GameTime gameTime, float delta)
    {
        
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
        spriteBatch.Begin(transformMatrix: Matrix.CreateTranslation(new Vector3(16, 16, 0)));
        spriteBatch.Draw(_background, Vector2.Zero, Color.White);
        
        DateTime now = DateTime.Now;
        int hour = now.Hour;
        int minute = now.Minute;
        
        int firstDigit = hour / 10;
        int secondDigit = hour % 10;
        int thirdDigit = minute / 10;
        int fourthDigit = minute % 10;
        
        DrawDigit(spriteBatch, firstDigit, 1, 5);
        DrawDigit(spriteBatch, secondDigit, 6, 5);
        DrawDigit(spriteBatch, thirdDigit, 13, 5);
        DrawDigit(spriteBatch, fourthDigit, 18, 5);
        spriteBatch.End();
    }
    
    private void DrawDigit(SpriteBatch spriteBatch, int digit, int x, int y)
    {
        const int digitWidth = 32;
        const int digitHeight = 72;

        Rectangle sourceRect = new Rectangle(digit * digitWidth, 0, digitWidth, digitHeight);
        spriteBatch.Draw(_font, new Vector2(x * _cellSize, y * _cellSize), sourceRect, Color.White);
    }
}