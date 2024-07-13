using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace PatteLib.Graphics
{
    public class ImageFontRenderer
    {
        private ImageFont _font;
        private SpriteBatch _spriteBatch;

        public ImageFontRenderer(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, ImageFont font)
        {
            _spriteBatch = spriteBatch;
            _font = font;
        }

        public void DrawText(string text, Vector2 position)
        {
            DrawText(text, position, Color.White, 1, 1);
        }

        public void DrawText(string text, Vector2 position, Color tint)
        {
            DrawText(text, position, tint, 1, 1);
        }

        public void DrawText(string text, Vector2 position, Color tint, int sx, int sy)
        {
            Vector2 currentPosition = position;
            Color currentTint = tint;
            int currentSX = sx;
            int currentSY = sy;

            Queue<(char charToDraw, int scaleX, int scaleY, Color colorTint)> renderQueue = new Queue<(char, int, int, Color)>();

            for (int i = 0; i < text.Length; i++)
            {
                char character = text[i];

                if (character == '§' && i + 1 < text.Length)
                {
                    i++;
                    var command = text[i];

                    switch (command)
                    {
                        case 'l':
                            currentSX = 1;
                            currentSY = 2;
                            break;
                        case 'r':
                            currentTint = Color.White;
                            currentSX = 1;
                            currentSY = 1;
                            break;
                        case 'a':
                            currentTint = new Color(170, 255, 0); // Hellgrün
                            break;
                        case 'b':
                            currentTint = new Color(0, 255, 255); // Aqua
                            break;
                        case 'c':
                            currentTint = new Color(255, 85, 85); // Hellrot/Pink
                            break;
                        case 'd':
                            currentTint = new Color(255, 85, 255); // Hellpurpur
                            break;
                        case 'e':
                            currentTint = new Color(255, 255, 85); // Gelb
                            break;
                        case 'f':
                            currentTint = new Color(255, 255, 255); // Weiß
                            break;
                        case '0':
                            currentTint = new Color(0, 0, 0); // Schwarz
                            break;
                        case '1':
                            currentTint = new Color(0, 0, 170); // Dunkelblau
                            break;
                        case '2':
                            currentTint = new Color(0, 170, 0); // Dunkelgrün
                            break;
                        case '3':
                            currentTint = new Color(0, 170, 170); // Dunkelaqua
                            break;
                        case '4':
                            currentTint = new Color(170, 0, 0); // Dunkelrot
                            break;
                        case '5':
                            currentTint = new Color(170, 0, 170); // Lila
                            break;
                        case '6':
                            currentTint = new Color(255, 170, 0); // Gold
                            break;
                        case '7':
                            currentTint = new Color(170, 170, 170); // Grau
                            break;
                        case '8':
                            currentTint = new Color(85, 85, 85); // Dunkelgrau
                            break;
                        case '9':
                            currentTint = new Color(85, 85, 255); // Blau
                            break;
                    }
                }

                else if (character == ' ')
                {
                    renderQueue.Enqueue((' ', currentSX, currentSY, currentTint));
                }
                else
                {
                    renderQueue.Enqueue((character, currentSX, currentSY, currentTint));
                }
            }

            while (renderQueue.Count > 0)
            {
                var (charToDraw, scaleX, scaleY, colorTint) = renderQueue.Dequeue();

                if (charToDraw == ' ')
                {
                    currentPosition.X += _font.SpaceWidth * currentSX;
                }
                else if (_font.HasChar(charToDraw))
                {
                    var charInfo = _font.GetChar(charToDraw);
                    if (charInfo.HasValue)
                    {
                        DrawCharacter(_spriteBatch, charToDraw, currentPosition, colorTint, scaleX, scaleY);
                        currentPosition.X += charInfo.Value.w * scaleX; // Advance position based on character width and scale
                    }
                }
            }
        }

        private void DrawCharacter(SpriteBatch spriteBatch, char character, Vector2 position, Color tint, int sx, int sy)
        {
            if (_font.HasChar(character))
            {
                var charInfo = _font.GetChar(character) ?? throw new Exception();
                Rectangle sourceRect = new Rectangle(charInfo.x, charInfo.y, charInfo.w, charInfo.h);
                spriteBatch.Draw(_font.Texture, position, sourceRect, tint, 0f, Vector2.Zero, new Vector2(sx, sy), SpriteEffects.None, 0f);
            }
        }
    }
}
