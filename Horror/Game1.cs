using Horror.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Horror;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteAnimationPlayer _animationPlayer;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _animationPlayer = new SpriteAnimationPlayer(Services);
        
        _animationPlayer.AddAnimation(SpriteAnimation.Load(GraphicsDevice, "Content/Animations/PlayerWalkUp.json"));
        _animationPlayer.Play("PlayerWalkUp");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _animationPlayer.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

       _spriteBatch.Begin();
       if (_animationPlayer.CurrentTexture != null)
       {
           _spriteBatch.Draw(_animationPlayer.CurrentTexture, Vector2.Zero, Color.White);
       }
       _spriteBatch.End();

        base.Draw(gameTime);
    }
}