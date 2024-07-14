using Microsoft.Xna.Framework.Graphics;

namespace PatteLib.Graphics;

public class MessageRenderer
{
    private GraphicsDevice _graphicsDevice;
    private SpriteBatch _spriteBatch;
    private ImageFontRenderer _imageFontRenderer;
    
    public MessageRenderer(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = spriteBatch;
    }

    public void DrawMessage()
    {
        
    }
}