using System;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PatteLib.Data;
using PatteLib.Gameplay.Scripting;
using PatteLib.World;
using Pattemon.Audio;
using Pattemon.Data;
using Pattemon.Engine;
using Pattemon.Graphics;
using Pattemon.Scenes;
using Pattemon.Scenes.ChoosePokemon;
using Pattemon.Scenes.FieldMenu;
using Pattemon.Scenes.OptionMenu;
using Pattemon.Scenes.Television;
using Pattemon.Scenes.WorldMap;
using FieldScene = Pattemon.Scenes.Field.FieldScene;

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
    
    private SceneAManager _sceneAManager;
    private HeaderManager _headerManager;
    private WorldTimeManager _worldTimeManager;
    
    private OptionMenuScene _optionMenuScene;
    private ChoosePokemonScene _choosePokemonScene;
    private FieldMenuScene _fieldMenuScene;
    private ScriptProcessor _scriptProcessor;

    private float _worldTimeUpdateTimer;
    
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
        _sceneAManager = new SceneAManager();
        Services.AddService(_sceneAManager);
        
        _scriptProcessor = new ScriptProcessor();
        Services.AddService(_scriptProcessor);
        
        _preferedScreenSize = RenderCore.PreferedScreenSize;
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        _headerManager = new HeaderManager();
        HeaderManager.RootDirectory = @"Content\WorldData\Headers";
        _headerManager.Load();
        Services.AddService(_headerManager);
        
        _worldTimeManager = new WorldTimeManager();
        Services.AddService(_worldTimeManager);
        
        Services.AddService(new Bag());
        
        _optionMenuScene = new OptionMenuScene(this);
        _choosePokemonScene = new ChoosePokemonScene(this);
        _fieldMenuScene = new FieldMenuScene(this);
        
        GraphicsCore.Init(GraphicsDevice);
        GraphicsCore.Load(Content);
        Graphics.Window.Load(Content);
        RenderCore.Init(GraphicsDevice, _spriteBatch);
        TransitionCore.Init();
        DualScreenCore.Init();
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
        
        DualScreenCore.FocusScreenRectangle = new Rectangle(0, 0, RenderCore.OriginalScreenSize.X, RenderCore.OriginalScreenSize.Y);
        DualScreenCore.UnfocusScreenRectangle = new Rectangle(0, RenderCore.OriginalScreenSize.Y, RenderCore.OriginalScreenSize.X, RenderCore.OriginalScreenSize.Y);
        
        DualScreenCore.TopScreenRectangle = DualScreenCore.FocusScreenRectangle;
        DualScreenCore.BottomScreenRectangle = DualScreenCore.UnfocusScreenRectangle;
        
        _sceneAManager.Next(new TelevisionScene(this));
            
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
        TransitionCore.Update(gameTime, delta);
        DualScreenCore.Update(gameTime, delta);
        RenderCore.UpdateTransition(gameTime);
        AudioCore.Update(gameTime);

        _worldTimeUpdateTimer += delta;

        if (_worldTimeUpdateTimer > 1)
        {
            _worldTimeUpdateTimer -= 1;
            _worldTimeManager.Update(gameTime);
        }

        if (!DualScreenCore.IsSwappingScreens)
        {
            if (KeyboardHandler.IsKeyDownOnce(Keys.Tab))
            {
                DualScreenCore.SwapScreens();
            }
            _sceneAManager.Update(gameTime, delta);
        }
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        delta = Core.GetDelta(delta);
        
        RenderCore.SetTopScreen();
        GraphicsDevice.Clear(Color.Black);
        RenderCore.SetBottomScreen();
        GraphicsDevice.Clear(Color.Black);
        
        _sceneAManager.Draw(_spriteBatch, gameTime, delta);
        
        RenderCore.RenderTransition();
        TransitionCore.Draw();
        RenderCore.Reset();
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
        if (!DualScreenCore.IsBottomScreenInFocus)
        {
            _spriteBatch.Draw(RenderCore.TopScreen, DualScreenCore.TopScreenRectangle, Color.White);
            if (DualScreenCore.IsSwappingScreens)
            {
                _spriteBatch.Draw(RenderCore.BottomScreen, DualScreenCore.BottomScreenRectangle, Color.White);
            }
        }
        else
        {
            if (DualScreenCore.IsSwappingScreens)
            {
                _spriteBatch.Draw(RenderCore.TopScreen, DualScreenCore.TopScreenRectangle, Color.White);
            }
            _spriteBatch.Draw(RenderCore.BottomScreen, DualScreenCore.BottomScreenRectangle, Color.White);
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
        
        Rectangle dst = DualScreenCore.FocusScreenRectangle;
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

        DualScreenCore.FocusScreenRectangle = dst;
        DualScreenCore.UnfocusScreenRectangle.Width =  DualScreenCore.FocusScreenRectangle.Width;
        DualScreenCore.UnfocusScreenRectangle.Height =  DualScreenCore.FocusScreenRectangle.Height;
        
        if (_verticalFit)
        {
            DualScreenCore.UnfocusScreenRectangle.X = _barSize;
            DualScreenCore.UnfocusScreenRectangle.Y =  DualScreenCore.FocusScreenRectangle.Height;
        }
        if (_horizontalFit)
        {
            DualScreenCore.UnfocusScreenRectangle.Y = _barSize;
            DualScreenCore.UnfocusScreenRectangle.X =  DualScreenCore.FocusScreenRectangle.Width;
        }
        
        if (DualScreenCore.IsBottomScreenInFocus)
        {
            DualScreenCore.BottomScreenRectangle =  DualScreenCore.FocusScreenRectangle;
            DualScreenCore.TopScreenRectangle =  DualScreenCore.UnfocusScreenRectangle;
        }
        else
        {
            DualScreenCore.TopScreenRectangle =  DualScreenCore.FocusScreenRectangle;
            DualScreenCore.BottomScreenRectangle = DualScreenCore.UnfocusScreenRectangle;
        }
    }

    private void OnResize(object sender, EventArgs e)
    {
        PerformScreenFit();
    }

    #endregion
}