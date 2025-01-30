using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pattemon.Engine;

public static class RenderCore
{
    public enum TransitionType
    {
        AlphaIn,
        AlphaOut,
        SlideIn,
        SlideOut
    }
    
    public static readonly Point PreferedScreenSize = new Point(256, 192);
    public static readonly Vector2 ScreenCenter = new Vector2(128f, 96f);

    private static Texture2D _pixel;
    private static float _transitionProgress;
    private static bool _isTransitioning;
    private static int _transitionSpeed;
    private static TransitionType _transitionType;
    
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
        
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
        
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

    public static void StartScreenTransition(int speed, TransitionType type)
    {
        _isTransitioning = true;
        _transitionProgress = 0f;
        _transitionSpeed = speed;
        _transitionType = type;
    }

    public static bool IsScreenTransitionDone()
    {
        return !_isTransitioning;
    }
    
    public static void UpdateTransition(GameTime gameTime)
    {
        if (!_isTransitioning) return;

        _transitionProgress += (float)gameTime.ElapsedGameTime.TotalMilliseconds / _transitionSpeed;

        if (_transitionProgress >= 1f)
        {
            _transitionProgress = 1f;
            _isTransitioning = false;
        }
    }

    public static void RenderTransition()
    {
        if (!_isTransitioning)
        {
            return;
        }
        
        RenderCore.SetTopScreen();
        _spriteBatch.Begin();
        switch (_transitionType)
        {
            case TransitionType.AlphaIn: // Alpha-In
                _spriteBatch.Draw(_pixel, new Rectangle(0, 0, PreferedScreenSize.X, PreferedScreenSize.Y), new Color(Color.Black, 1f - _transitionProgress));
                break;

            case TransitionType.AlphaOut: // Alpha-Out
                _spriteBatch.Draw(_pixel, new Rectangle(0, 0, PreferedScreenSize.X, PreferedScreenSize.Y), new Color(Color.Black, _transitionProgress));
                break;

            case TransitionType.SlideIn: // Slide-In from bottom
                _spriteBatch.Draw(_pixel, new Rectangle(0, (int)(PreferedScreenSize.Y * (1f - _transitionProgress)), PreferedScreenSize.X, PreferedScreenSize.Y), Color.Black);
                break;

            case TransitionType.SlideOut: // Slide-Out to bottom
                _spriteBatch.Draw(_pixel, new Rectangle(0, (int)(PreferedScreenSize.Y * _transitionProgress), PreferedScreenSize.X, PreferedScreenSize.Y), Color.Black);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        _spriteBatch.End();
        
        SetBottomScreen();
        //not at the same time but delayed so it looks like it is on woosh
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