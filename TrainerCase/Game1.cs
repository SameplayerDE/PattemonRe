using System;
using System.Collections.Generic;
using MeltySynth;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TrainerCase;

public class Game1 : Game
{
    private readonly Point _preferredDimensions = new Point(256, 192);
    
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private RenderTarget2D _badgeTarget;
    private RenderTarget2D _badgeCaseTarget;
    private RenderTarget2D _badgeCaseTopTarget;
    
    private Texture2D _badgeCase;
    private Texture2D _badgeCaseTop;
    private Texture2D _sparkAnimationSheet0;
    private Texture2D _sparkAnimationSheet1;
    private Texture2D _clickAnimationSheet;
    
    private MouseState _previousMouseState;
    private MouseState _currentMouseState;
    
    private List<Badge> _badges = [];
    private const int MaxTrailPoints = 5;
    private List<Point> _mouseTrail = [];
    
    private MidiPlayer midiPlayer;
    private MidiFile midiFile;
    
    private SpriteAnimation _sparkAnimation0;
    private SpriteAnimation _sparkAnimation1;
    private SpriteAnimation _clickAnimation;
    
    private bool _clicked;
    private bool _open;
    private bool _isOpening;
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        _graphics.PreferredBackBufferWidth = _preferredDimensions.X;
        _graphics.PreferredBackBufferHeight = _preferredDimensions.Y;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        _badgeTarget = new RenderTarget2D(GraphicsDevice, _preferredDimensions.X, _preferredDimensions.Y, false, SurfaceFormat.Color, DepthFormat.None);
        _badgeCaseTarget = new RenderTarget2D(GraphicsDevice, _preferredDimensions.X, _preferredDimensions.Y, false, SurfaceFormat.Color, DepthFormat.None);
        _badgeCaseTopTarget = new RenderTarget2D(GraphicsDevice, _preferredDimensions.X, _preferredDimensions.Y, false, SurfaceFormat.Color, DepthFormat.None);

        _badges = [
            Badge.CreateBadge(0, Content),
            Badge.CreateBadge(1, Content),
            Badge.CreateBadge(2, Content),
            Badge.CreateBadge(3, Content),
            Badge.CreateBadge(4, Content),
            Badge.CreateBadge(5, Content),
            Badge.CreateBadge(6, Content),
            Badge.CreateBadge(7, Content),
        ];
        _badgeCase = Content.Load<Texture2D>("Case");
        _badgeCaseTop = Content.Load<Texture2D>("CaseTop");
        
        //midiPlayer = new MidiPlayer(@"C:\Users\asame\Music\Pokemon Platinum\SoundFonts\BANK_BGM_FIELD.sf2");
        midiPlayer = new MidiPlayer(@"C:\Users\asame\Music\Pokemon Platinum\SEQ_SE_DP_BADGE_C.sf2");
        midiFile = new MidiFile(@"C:\Users\asame\Music\Pokemon Platinum\SEQ_SE_DP_BADGE_C.mid", 0x34);
        
        _sparkAnimationSheet0 = Content.Load<Texture2D>("Spark_0");
        _sparkAnimation0 = new SpriteAnimation(_sparkAnimationSheet0, 40, 40, 4, 0.12f);
        
        _sparkAnimationSheet1 = Content.Load<Texture2D>("Spark_1");
        _sparkAnimation1 = new SpriteAnimation(_sparkAnimationSheet1, 64, 64, 4, 0.06f);
        
        _clickAnimationSheet = Content.Load<Texture2D>("Button_Wave");
        _clickAnimation = new SpriteAnimation(_clickAnimationSheet, 64, 64, 3, 0.03f);
        _clickAnimation.SetLoop(false);
        _clickAnimation.Stop();
    }

    private bool InsideClient(int x, int y)
    {
        var windowRect = Window.ClientBounds;
        if (x >= 0 && y >= 0 && x < windowRect.Width && y < windowRect.Height)
        {
            return true;
        }
        return false;
    }

    private int GetBadgeAt(int x, int y)
    {
        const int offsetX = 3 * 8;
        const int offsetY = 5 * 8;
        const int rowCount = 4;
        const int size = 40;
        const int space = 7 * 8;
        for (int i = 0; i < _badges.Count; i++)
        {
            int tempX = offsetX + (i % rowCount) * space;
            int tempY = offsetY + (i / rowCount) * space;

            if (x >= tempX && y >= tempY && x < tempX + size && y < tempY + size)
            {
                return i;
            }
        }
        return -1;
    }
    
    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
        }
        
        _sparkAnimation0.Update(gameTime);
        _sparkAnimation1.Update(gameTime);
        _clickAnimation.Update(gameTime);
        
        _previousMouseState = _currentMouseState;
        _currentMouseState = Mouse.GetState();
        
        if (_clicked)
        {
            if (!_clickAnimation.IsPlaying)
            {
                _clicked = false;
            }
        }
        
        var currentPosition = _currentMouseState.Position;
        if (!InsideClient(currentPosition.X, currentPosition.Y))
        {
            return;
        }
        
        var pixelColour = Color.Transparent;

        if (_currentMouseState.LeftButton == ButtonState.Pressed)
        {
            if (_previousMouseState.LeftButton == ButtonState.Released)
            {
                if (currentPosition.X >= 14 * 8)
                {
                    if (currentPosition.Y >= 19 * 8)
                    {
                        if (currentPosition.X < 14 * 8 + 32)
                        {
                            if (currentPosition.Y < 19 * 8 + 32)
                            {
                                if (!_clicked)
                                {
                                    _open = !_open;
                                    _clickAnimation.Play();
                                    _clicked = true;
                                }
                            }
                        }
                        return;
                    }
                }
            }
            
            if (_mouseTrail.Count > MaxTrailPoints)
            {
                _mouseTrail.RemoveAt(0);
            }
            _mouseTrail.Add(currentPosition);

            var badge = GetBadgeAt(currentPosition.X, currentPosition.Y);
            if (badge != -1)
            {
                Color[] retrievedColor = new Color[1];
                _badgeCase.GetData(0, new Rectangle(currentPosition.X, currentPosition.Y, 1, 1), retrievedColor, 0, 1);
                pixelColour = retrievedColor[0];
            }
        }
        else
        {
            _mouseTrail.Clear();
        }
        
        if (pixelColour is { R: 0x60, G: 0x60, B: 0x60 })
        {
            if (_previousMouseState.LeftButton == ButtonState.Released)
            {
                var badge = GetBadgeAt(currentPosition.X, currentPosition.Y);
                _badges[badge].CycleState();
            }
        }
        
        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_badgeCaseTarget);
        GraphicsDevice.Clear(Color.Transparent);
        _spriteBatch.Begin();
        _spriteBatch.Draw(_badgeCase, Vector2.Zero, Color.White);
        _spriteBatch.End();
        
        GraphicsDevice.SetRenderTarget(_badgeTarget);
        GraphicsDevice.Clear(Color.Transparent);
        _spriteBatch.Begin();
        
        const int offsetX = 3 * 8;
        const int offsetY = 5 * 8;
        const int rowCount = 4;
        const int space = 7 * 8;
        for (int i = 0; i < _badges.Count; i++)
        {
            var badge = _badges[i];
            
            int x = offsetX + (i % rowCount) * space;
            int y = offsetY + (i / rowCount) * space;
            
            _spriteBatch.Draw(badge.GetTexture(), new Vector2(x, y), Color.White);
            if (badge.GetState() == BadgeState.Clean1)
            {
                _sparkAnimation1.Draw(_spriteBatch, new Vector2(x - 8, y), Color.White);
            }
            else if (badge.GetState() == BadgeState.Clean0)
            {
                _sparkAnimation0.Draw(_spriteBatch, new Vector2(x, y), Color.White);
            }
        }

        _spriteBatch.End();
        
        GraphicsDevice.SetRenderTarget(_badgeCaseTopTarget);
        GraphicsDevice.Clear(Color.Transparent);
        _spriteBatch.Begin();
        _spriteBatch.Draw(_badgeCaseTop, _open ? new Rectangle(0, 0, 256, 8) : new Rectangle(0, 0, 256, 152), new Rectangle(0, 0, 256, 152), Color.White);
        _spriteBatch.End();
        
        GraphicsDevice.SetRenderTarget(null);
        _spriteBatch.Begin();
        _spriteBatch.Draw(_badgeCaseTarget, Vector2.Zero, Color.White);
        _spriteBatch.Draw(_badgeTarget, Vector2.Zero, Color.White);
        _spriteBatch.Draw(_badgeCaseTopTarget, Vector2.Zero, Color.White);

        if (_clicked)
        {
            _clickAnimation.Draw(_spriteBatch, new Vector2(12, 17) * 8, Color.White);
        }
        
        _spriteBatch.End();
        
        base.Draw(gameTime);
    }
}