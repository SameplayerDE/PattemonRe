using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Pattemon.Graphics;

public class Window
{
    public int X;
    public int Y;
    public int Width;
    public int Height;

    private static Texture2D _texture;
    private Rectangle[] _sourceRects;
    
    private const int TileSize = 8; 
    
    public static void Load(ContentManager contentManager)
    {
        _texture = contentManager.Load<Texture2D>("window");
    }
    
    public static Window Create(int x, int y, int width, int height, int type = 0)
    {
        var window = new Window
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            _sourceRects = new Rectangle[9]
        };

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                window._sourceRects[i * 3 + j] = new Rectangle(j * 8, i * 8, 8, 8);
            }
        }
        return window;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
        if (_texture == null)
        {
            return;
        }

        int centerW = Width - 2 * TileSize;
        int centerH = Height - 2 * TileSize;

        // Draw the nine-tile grid
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int drawX = X + (j == 0 ? 0 : (j == 1 ? TileSize : TileSize + centerW));
                int drawY = Y + (i == 0 ? 0 : (i == 1 ? TileSize : TileSize + centerH));

                int drawW = (j == 1) ? centerW : TileSize;
                int drawH = (i == 1) ? centerH : TileSize;

                var destRect = new Rectangle(drawX, drawY, drawW, drawH);
                spriteBatch.Draw(_texture, destRect, _sourceRects[i * 3 + j], Color.White);
            }
        }
    }
}