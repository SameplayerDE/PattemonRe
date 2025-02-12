using System;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pattemon.Audio;
using Pattemon.Engine;
using Pattemon.Graphics;
using Pattemon.Scenes;
using Pattemon.Scenes.ChoosePokemon;
using Pattemon.Scenes.FieldMenu;
using Pattemon.Scenes.OptionMenu;

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
    
    private Rectangle _topScreenRectangle;
    private Rectangle _bottomScreenRectangle;
    
    private Rectangle _focusScreenRectangle;
    private Rectangle _unfocusScreenRectangle;

    private float _transitionProgress = 1.0f;
    private const float TransitionSpeed = 2.0f;

    private SceneManager _sceneManager;
    private SceneAManager _sceneAManager;
    
    private OptionMenuScene _optionMenuScene;
    private ChoosePokemonScene _choosePokemonScene;
    private FieldMenuScene _fieldMenuScene;
    
    public PatteGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
            
        IsMouseVisible = true;
        Window.AllowUserResizing = true;

        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
    }

    protected override void Initialize()
    {
        _sceneManager = new SceneManager();
        _sceneAManager = new SceneAManager();
        _preferedScreenSize = RenderCore.PreferedScreenSize;
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        _optionMenuScene = new OptionMenuScene(this);
        _choosePokemonScene = new ChoosePokemonScene(this);
        _fieldMenuScene = new FieldMenuScene(this);
        
        GraphicsCore.Init(GraphicsDevice);
        GraphicsCore.Load(Content);
        Graphics.Window.Load(Content);
        RenderCore.Init(GraphicsDevice, _spriteBatch);
        AudioCore.Init(Content);
        
        _optionMenuScene.Init();
        _choosePokemonScene.Init();
        _fieldMenuScene.Init();
        
        _graphics.PreferredBackBufferWidth = RenderCore.OriginalScreenSize.X;
        _graphics.PreferredBackBufferHeight = RenderCore.OriginalScreenSize.Y;
        _graphics.PreferredBackBufferFormat = SurfaceFormat.Color;
        _graphics.PreferredDepthStencilFormat = DepthFormat.Depth24; // <-- set depth here
        _graphics.HardwareModeSwitch = false;
        _graphics.PreferMultiSampling = false;
        _graphics.IsFullScreen = false;
        _graphics.ApplyChanges();

        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnResize;
        
        _focusScreenRectangle = new Rectangle(0, 0, RenderCore.OriginalScreenSize.X, RenderCore.OriginalScreenSize.Y);
        _unfocusScreenRectangle = new Rectangle(0, RenderCore.OriginalScreenSize.Y, RenderCore.OriginalScreenSize.X, RenderCore.OriginalScreenSize.Y);
        
        _topScreenRectangle = _focusScreenRectangle;
        _bottomScreenRectangle = _unfocusScreenRectangle;
            
        PerformScreenFit();
    }

    protected override void Update(GameTime gameTime)
    {
        var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        delta = Core.GetDelta(delta);
        
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

        if (_transitionProgress >= 1.0f)
        {
            if (KeyboardHandler.IsKeyDownOnce(Keys.Z))
            {
                _sceneAManager.Next(new FieldMenuScene(this));
            }
            if (KeyboardHandler.IsKeyDownOnce(Keys.F))
            {
                _sceneAManager.Next(new OptionMenuScene(this));
            }
            if (KeyboardHandler.IsKeyDownOnce(Keys.P))
            {
                _sceneAManager.Next(new ChoosePokemonScene(this));
            }
            _sceneManager.Update(gameTime);
            _sceneAManager.Update(gameTime, delta);
            
            //_optionMenuScene.Update(gameTime, delta);
            //_choosePokemonScene.Update(gameTime, delta);
            //_fieldMenuScene.Update(gameTime, delta);
        }
        
        RenderCore.UpdateTransition(gameTime);
        AudioCore.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        delta = Core.GetDelta(delta);
        
        RenderCore.SetTopScreen();
        GraphicsDevice.Clear(Color.Red);
        RenderCore.SetBottomScreen();
        GraphicsDevice.Clear(Color.Blue);
        
        _sceneManager.Draw(_spriteBatch, gameTime);
        _sceneAManager.Draw(_spriteBatch, gameTime, delta);
        
        //RenderCore.SetTopScreen();
        //_optionMenuScene.Draw(_spriteBatch, gameTime, delta);
        //_choosePokemonScene.Draw(_spriteBatch, gameTime, delta);
        //_fieldMenuScene.Draw(_spriteBatch, gameTime, delta);
        
        RenderCore.RenderTransition();
        RenderCore.Reset();
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
        if (!_isBottomScreenFocus)
        {
            _spriteBatch.Draw(RenderCore.TopScreen, _topScreenRectangle, Color.White);
            if (_transitionProgress < 1.0f)
            {
                _spriteBatch.Draw(RenderCore.BottomScreen, _bottomScreenRectangle, Color.White);
            }
        }
        else
        {
            if (_transitionProgress < 1.0f)
            {
                _spriteBatch.Draw(RenderCore.TopScreen, _topScreenRectangle, Color.White);
            }
            _spriteBatch.Draw(RenderCore.BottomScreen, _bottomScreenRectangle, Color.White);
        }
        _spriteBatch.End();
    }

    #region ScreenFit

    private void PerformScreenFit()
    {
        var outputAspect = Window.ClientBounds.Width / (float) Window.ClientBounds.Height;
        var preferredAspect = _preferedScreenSize.X / (float) _preferedScreenSize.Y;

        //Console.WriteLine(outputAspect);
        //Console.WriteLine(preferredAspect);
        
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
            _barSize = 0;
            dst = new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);
        }

        _focusScreenRectangle = dst;
        _unfocusScreenRectangle.Width = _focusScreenRectangle.Width;
        _unfocusScreenRectangle.Height = _focusScreenRectangle.Height;
        
        if (_verticalFit)
        {
            _unfocusScreenRectangle.X = _barSize;
            _unfocusScreenRectangle.Y = _focusScreenRectangle.Height;
        }
        if (_horizontalFit)
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