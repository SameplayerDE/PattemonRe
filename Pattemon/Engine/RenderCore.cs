using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pattemon.Engine;

public static class RenderCore
{
    public static readonly Point PreferedScreenSize = new Point(256, 192);

    private static GraphicsDevice _graphicsDevice;
    private static SpriteBatch _spriteBatch;
    
    public static GraphicsDevice GraphicsDevice => _graphicsDevice;
    public static SpriteBatch SpriteBatch => _spriteBatch;
    
    private static RenderTarget2D _topScreen;
    private static RenderTarget2D _bottomScreen;

    public static RenderTarget2D TopScreen => _topScreen;
    public static RenderTarget2D BottomScreen => _bottomScreen;
    
    public static void Init(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = spriteBatch;
        
        _topScreen = new RenderTarget2D(
            _graphicsDevice,
            PreferedScreenSize.X, PreferedScreenSize.Y,
            false,
            _graphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents
        );
        
        _bottomScreen = new RenderTarget2D(
            _graphicsDevice,
            PreferedScreenSize.X, PreferedScreenSize.Y,
            false,
            _graphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents
        );
    }

    public static void SetTopScreen()
    {
        _graphicsDevice.SetRenderTarget(_topScreen);
    }
    
    public static void SetBottomScreen()
    {
        _graphicsDevice.SetRenderTarget(_bottomScreen);
    }
    
    public static void Reset()
    {
        _graphicsDevice.SetRenderTarget(null);
    }
    
    
}