using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
            DrawText(text, position, Color.White, 1);
        }
        
        public void DrawText(string text, Vector2 position, Color tint)
        {
            DrawText(text, position, tint, 1);
        }
        
        public void DrawText(string text, Vector2 position, Color tint, int scale)
        {
            Vector2 currentPosition = position;
            
            foreach (char character in text)
            {
                if (_font.HasChar(character))
                {
                    var charInfo = _font.GetChar(character);
                    if (charInfo.HasValue)
                    {
                        DrawCharacter(_spriteBatch, character, currentPosition, tint, scale);
                        currentPosition.X += charInfo.Value.w * scale; // Advance position based on character width and scale
                    }
                }
            }
        }
        
        private void DrawCharacter(SpriteBatch spriteBatch, char character, Vector2 position, Color tint, int scale)
        {
            if (_font.HasChar(character))
            {
                var charInfo = _font.GetChar(character) ?? throw new Exception();
                Rectangle sourceRect = new Rectangle(charInfo.x, charInfo.y, charInfo.w, charInfo.h);
                spriteBatch.Draw(_font.Texture, position, sourceRect, tint, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
        }
        
    }
}