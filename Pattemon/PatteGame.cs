using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pattemon;

public class PatteGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private RenderTarget2D _renderTarget;
    public Rectangle RenderTargetRectangle;
    private Point _preferedScreenSize;
        
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
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
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
            Exit();
        }

        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
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