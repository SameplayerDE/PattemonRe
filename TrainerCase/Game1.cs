using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
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
    
    private MouseState _previousMouseState;
    private MouseState _currentMouseState;
    
    private List<Badge> _badges = [];
    
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
    
    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        _previousMouseState = _currentMouseState;
        _currentMouseState = Mouse.GetState();
        
        var currentPosition = _currentMouseState.Position;
        if (!InsideClient(currentPosition.X, currentPosition.Y))
        {
            return;
        }
        
        var pixelColour = Color.Transparent;

        if (_currentMouseState.LeftButton == ButtonState.Pressed)
        { 
            Color[] retrievedColor = new Color[1];
            _badgeCase.GetData(0, new Rectangle(currentPosition.X, currentPosition.Y, 1, 1), retrievedColor, 0, 1);
            pixelColour = retrievedColor[0];
        }
        
        Console.WriteLine(pixelColour);
        
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
        }

        _spriteBatch.End();
        
        GraphicsDevice.SetRenderTarget(_badgeCaseTopTarget);
        GraphicsDevice.Clear(Color.Transparent);
        _spriteBatch.Begin();
        _spriteBatch.End();
        
        GraphicsDevice.SetRenderTarget(null);
        _spriteBatch.Begin();
        _spriteBatch.Draw(_badgeCaseTarget, Vector2.Zero, Color.White);
        _spriteBatch.Draw(_badgeTarget, Vector2.Zero, Color.White);
        _spriteBatch.Draw(_badgeCaseTopTarget, Vector2.Zero, Color.White);
        _spriteBatch.End();
        
        base.Draw(gameTime);
    }
}