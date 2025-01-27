using System;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pattemon.Engine;
using Pattemon.Scenes;

namespace Pattemon;

public class PatteGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Point _preferedScreenSize;
    private int _barSize = 0;
    private bool _hasBlackBars; // if the screen fits perfect or not
    private bool _verticalFit; // if the screen fits vertically
    private bool _horizontalFit; // if the screen fits horizontally
    
    private bool _isBottomScreenFocus = false;
    
    private RenderTarget2D _topScreen;
    private Rectangle _topScreenRectangle;
    
    private RenderTarget2D _bottomScreen;
    private Rectangle _bottomScreenRectangle;
    
    private Rectangle _focusScreenRectangle;
    private Rectangle _unfocusScreenRectangle;

    private float _transitionProgress = 1.0f;
    private const float TransitionSpeed = 2.0f;

    private SceneManager _sceneManager;
    
    private Texture2D _topDummy;
    private Texture2D _bottomDummy;
    
    public PatteGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
            
        IsMouseVisible = true;
        Window.AllowUserResizing = true;

        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);

        _preferedScreenSize = new Point(256 * 1, 192 * 1);
    }

    protected override void Initialize()
    {
        _sceneManager = new SceneManager();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        _topDummy = Content.Load<Texture2D>("TopScreen");
        _bottomDummy = Content.Load<Texture2D>("BottomScreen");
        
        _graphics.PreferredBackBufferWidth = _preferedScreenSize.X;
        _graphics.PreferredBackBufferHeight = _preferedScreenSize.Y;
        _graphics.PreferredBackBufferFormat = SurfaceFormat.Color;
        _graphics.PreferredDepthStencilFormat = DepthFormat.Depth24; // <-- set depth here
        _graphics.HardwareModeSwitch = false;
        _graphics.PreferMultiSampling = false;
        _graphics.IsFullScreen = false;
        _graphics.ApplyChanges();

        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnResize;
        
        _focusScreenRectangle = new Rectangle(0, 0, _preferedScreenSize.X, _preferedScreenSize.Y);
        _unfocusScreenRectangle = new Rectangle(0, _preferedScreenSize.Y, _preferedScreenSize.X, _preferedScreenSize.Y);
        
        _topScreen = new RenderTarget2D(
            GraphicsDevice,
            _preferedScreenSize.X, _preferedScreenSize.Y,
            false,
            GraphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24
        );
        _topScreenRectangle = _focusScreenRectangle;
        
        _bottomScreen = new RenderTarget2D(
            GraphicsDevice,
            _preferedScreenSize.X, _preferedScreenSize.Y,
            false,
            GraphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24
        );
        _bottomScreenRectangle = _unfocusScreenRectangle;
            
        PerformScreenFit();
    }

    protected override void Update(GameTime gameTime)
    {
        var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (!IsActive)
        {
            return;
        }
        Core.ReadInput();

        if (KeyboardHandler.IsKeyDownOnce(Keys.Tab) && _transitionProgress >= 1.0f)
        {
            _isBottomScreenFocus = !_isBottomScreenFocus;
            _transitionProgress = 0f;
        }
        
        if (KeyboardHandler.IsKeyDownOnce(Keys.Escape) && _transitionProgress >= 1.0f)
        {
            _sceneManager.Push(new OptionScene("name", this));
        }
        
        if (KeyboardHandler.IsKeyDownOnce(Keys.X) && _transitionProgress >= 1.0f)
        {
            _sceneManager.Push(new MenuScene("menu", this));
        }
        
        if (_transitionProgress < 1.0f)
        {
            _transitionProgress = Math.Min(_transitionProgress + delta * TransitionSpeed, 1.0f);

            if (_isBottomScreenFocus)
            {
                _bottomScreenRectangle.X = (int)MathHelper.Lerp(_unfocusScreenRectangle.X, _focusScreenRectangle.X, _transitionProgress);
                _bottomScreenRectangle.Y = (int)MathHelper.Lerp(_unfocusScreenRectangle.Y, _focusScreenRectangle.Y, _transitionProgress);
            }
            else
            {
                _bottomScreenRectangle.X = (int)MathHelper.Lerp(_focusScreenRectangle.X, _unfocusScreenRectangle.X, _transitionProgress);
                _bottomScreenRectangle.Y = (int)MathHelper.Lerp(_focusScreenRectangle.Y, _unfocusScreenRectangle.Y, _transitionProgress);
            }
        }
        
        _sceneManager.Update(gameTime);
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_topScreen);
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();
        _spriteBatch.Draw(_topDummy, Vector2.Zero, Color.White);
        _spriteBatch.End();
        _sceneManager.Draw(_spriteBatch, gameTime);
        
        GraphicsDevice.SetRenderTarget(_bottomScreen);
        GraphicsDevice.Clear(Color.Red);
        _spriteBatch.Begin();
        _spriteBatch.Draw(_bottomDummy, Vector2.Zero, Color.White);
        _spriteBatch.End();
        
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
        if (!_isBottomScreenFocus)
        {
            _spriteBatch.Draw(_topScreen, _topScreenRectangle, Color.White);
            if (_transitionProgress < 1.0f)
            {
                _spriteBatch.Draw(_bottomScreen, _bottomScreenRectangle, Color.White);
            }
        }
        else
        {
            if (_transitionProgress < 1.0f)
            {
                _spriteBatch.Draw(_topScreen, _topScreenRectangle, Color.White);
            }
            _spriteBatch.Draw(_bottomScreen, _bottomScreenRectangle, Color.White);
        }
        _spriteBatch.End();
    }

    #region ScreenFit

    private void PerformScreenFit()
    {
        var outputAspect = Window.ClientBounds.Width / (float) Window.ClientBounds.Height;
        var preferredAspect = _preferedScreenSize.X / (float) _preferedScreenSize.Y;

        Console.WriteLine(outputAspect);
        Console.WriteLine(preferredAspect);
        
        Rectangle dst = _focusScreenRectangle;
        if (outputAspect < preferredAspect)
        {
            // output is taller than it is wider, bars on top/bottom
            _verticalFit = false;
            _horizontalFit = true;
            _hasBlackBars = true;
            int presentHeight = (int) ((Window.ClientBounds.Width / preferredAspect) + 0.5f);
            int barHeight = (Window.ClientBounds.Height - presentHeight) / 2;
            _barSize = barHeight;
            dst = new Rectangle(0, barHeight, Window.ClientBounds.Width, presentHeight);
        }
        else if (outputAspect > preferredAspect)
        {
            // output is wider than it is tall, bars left/right
            _verticalFit = true;
            _horizontalFit = false;
            _hasBlackBars = true;
            int presentWidth = (int) ((Window.ClientBounds.Height * preferredAspect) + 0.5f);
            int barWidth = (Window.ClientBounds.Width - presentWidth) / 2;
            _barSize = barWidth;
            dst = new Rectangle(barWidth, 0, presentWidth, Window.ClientBounds.Height);
        }
        else
        {
            _hasBlackBars = false;
            _verticalFit = true;
            _horizontalFit = true;
        }

        _focusScreenRectangle = dst;
        _unfocusScreenRectangle.Width = _focusScreenRectangle.Width;
        _unfocusScreenRectangle.Height = _focusScreenRectangle.Height;
        
        if (_verticalFit)
        {
            _unfocusScreenRectangle.X = _barSize;
            _unfocusScreenRectangle.Y = _focusScreenRectangle.Height;
        }
        else if (_horizontalFit)
        {
            _unfocusScreenRectangle.Y = _barSize;
            _unfocusScreenRectangle.X = _focusScreenRectangle.Width;
        }
        
        if (_isBottomScreenFocus)
        {
            _bottomScreenRectangle = _focusScreenRectangle;
            _topScreenRectangle = _focusScreenRectangle;
        }
        else
        {
            _topScreenRectangle = _focusScreenRectangle;
            _bottomScreenRectangle = _unfocusScreenRectangle;
        }
    }

    private void OnResize(object sender, EventArgs e)
    {
        PerformScreenFit();
    }

    #endregion
}