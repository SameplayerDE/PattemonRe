using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pattemon;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D _areaIcon;
    private SpriteFont _font;
    
    private TimeSpan _animationTimer;
    private TimeSpan _animationFadeIn = TimeSpan.FromSeconds(0.32f);
    private TimeSpan _animationFadeOut = TimeSpan.FromSeconds(0.32f);
    private TimeSpan _animationStay = TimeSpan.FromSeconds(2f);

    private bool _animationRunning = true;
    private int _animationStep = 0;
    private Vector2 _position = new Vector2(4, -38);
    
    private RenderTarget2D _renderTarget;
    public Rectangle RenderTargetRectangle;
    private Point _preferedScreenSize;
        
    public Game1()
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
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _areaIcon = Content.Load<Texture2D>("Town1");
        _font = Content.Load<SpriteFont>("Font");
        
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

        _renderTarget = new RenderTarget2D(GraphicsDevice, _preferedScreenSize.X, _preferedScreenSize.Y, false,
            GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
        RenderTargetRectangle = new Rectangle(0, 0, _preferedScreenSize.X, _preferedScreenSize.Y);
            
        PerformScreenFit();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            _animationRunning = true;
            //Exit();
        }

        if (_animationRunning)
        {
            _animationTimer += gameTime.ElapsedGameTime;

            if (_animationStep == 0) // Fade In
            {
                var t = (float)(_animationTimer.TotalSeconds / _animationFadeIn.TotalSeconds);
                _position.Y = MathHelper.Lerp(-38, 0, t);

                if (_animationTimer >= _animationFadeIn)
                {
                    _animationTimer = TimeSpan.Zero;
                    _animationStep = 1;
                }
            }
            else if (_animationStep == 1) // Stay at Y = 0
            {
                if (_animationTimer >= _animationStay)
                {
                    _animationTimer = TimeSpan.Zero;
                    _animationStep = 2;
                }
            }
            else if (_animationStep == 2) // Fade Out
            {
                var t = (float)(_animationTimer.TotalSeconds / _animationFadeOut.TotalSeconds);
                _position.Y = MathHelper.Lerp(0, -38, t);
                
                if (_animationTimer >= _animationFadeOut)
                {
                    _animationTimer = TimeSpan.Zero;
                    _animationStep = 0;
                    _animationRunning = false;
                }
            }
        }

        
        //_position = new Vector2(134 - _areaIcon.Width, 38 - _areaIcon.Height);
       
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin(samplerState: SamplerState.PointWrap);
        

        _spriteBatch.Draw(_areaIcon, _position, Color.White);
        //_spriteBatch.DrawString(_font, "Zweiblattdorf", _position + new Vector2(11, 15), new Color(150, 150, 166));
        //_spriteBatch.DrawString(_font, "Zweiblattdorf", _position + new Vector2(12, 14), new Color(150, 150, 166));
        //_spriteBatch.DrawString(_font, "Zweiblattdorf", _position + new Vector2(11, 14), Color.Black);
        _spriteBatch.End();
        
        GraphicsDevice.SetRenderTarget(null);

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
        _spriteBatch.Draw(_renderTarget, RenderTargetRectangle, Color.White);
        _spriteBatch.End();
    }

    #region ScreenFit

    public void PerformScreenFit()
    {
        var outputAspect = Window.ClientBounds.Width / (float) Window.ClientBounds.Height;
        var preferredAspect = _preferedScreenSize.X / (float) _preferedScreenSize.Y;

        Rectangle dst;
        if (outputAspect <= preferredAspect)
        {
            // output is taller than it is wider, bars on top/bottom
            int presentHeight = (int) ((Window.ClientBounds.Width / preferredAspect) + 0.5f);
            int barHeight = (Window.ClientBounds.Height - presentHeight) / 2;
            dst = new Rectangle(0, barHeight, Window.ClientBounds.Width, presentHeight);
        }
        else
        {
            // output is wider than it is tall, bars left/right
            int presentWidth = (int) ((Window.ClientBounds.Height * preferredAspect) + 0.5f);
            int barWidth = (Window.ClientBounds.Width - presentWidth) / 2;
            dst = new Rectangle(barWidth, 0, presentWidth, Window.ClientBounds.Height);
        }

        RenderTargetRectangle = dst;

        //GameSceneManager.Instance.RenderContext.RenderTargetRectangle = RenderTargetRectangle;
        //GameSceneManager.Instance.RenderContext.RenderTargetScale = Vector2
        //    .Divide(RenderTargetRectangle.Size.ToVector2(), _preferedScreenSize.ToVector2()).Length();
        //GameSceneManager.Instance.RenderContext.Camera.BuildViewMatrix();
    }

    private void OnResize(object sender, EventArgs e)
    {
        var rectangle = GraphicsDevice.PresentationParameters.Bounds;
        var width = rectangle.Width;
        var height = rectangle.Height;

        var outputAspect = Window.ClientBounds.Width / (float) Window.ClientBounds.Height;
        var preferredAspect = _preferedScreenSize.X / (float) _preferedScreenSize.Y;

        var factorWidth = width / (float) _preferedScreenSize.X;
        var factorHeight = height / (float) _preferedScreenSize.Y;

        Rectangle dst;
        if (outputAspect <= preferredAspect)
        {
            // output is taller than it is wider, bars on top/bottom
            int presentHeight = (int) ((Window.ClientBounds.Width / preferredAspect) + 0.5f);
            int barHeight = (Window.ClientBounds.Height - presentHeight) / 2;
            dst = new Rectangle(0, barHeight, Window.ClientBounds.Width, presentHeight);
        }
        else
        {
            // output is wider than it is tall, bars left/right
            int presentWidth = (int) ((Window.ClientBounds.Height * preferredAspect) + 0.5f);
            int barWidth = (Window.ClientBounds.Width - presentWidth) / 2;
            dst = new Rectangle(barWidth, 0, presentWidth, Window.ClientBounds.Height);
        }

        /*if (width >= height)
        {
            _renderTargetRectangle.Width = (int)(_preferedScreenSize.X / factorWidth);
            _renderTargetRectangle.Height = height;
        }
        else
        {
            _renderTargetRectangle.Width = width;
            _renderTargetRectangle.Height = (int)(_preferedScreenSize.Y / factorHeight);
        }*/

        //_renderTargetRectangle.X = (width - _renderTargetRectangle.Width) / 2;
        //_renderTargetRectangle.Y = (height - _renderTargetRectangle.Height) / 2;

        RenderTargetRectangle = dst;

        //GameSceneManager.Instance.RenderContext.RenderTargetRectangle = RenderTargetRectangle;
        //GameSceneManager.Instance.RenderContext.RenderTargetScale = Vector2
        //    .Divide(RenderTargetRectangle.Size.ToVector2(), _preferedScreenSize.ToVector2()).Length();
        //GameSceneManager.Instance.RenderContext.Camera.BuildViewMatrix();
    }

    #endregion
}