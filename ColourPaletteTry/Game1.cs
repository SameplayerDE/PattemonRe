using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ColourPaletteTry
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Effect _paletteEffect;
        
        private Texture2D _texture;
        private Palette _normal;
        private Palette _shiny;
        private bool _swap = false;
        
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _paletteEffect = Content.Load<Effect>("PaletteSwapSprite");

            _texture = Texture2D.FromFile(GraphicsDevice, "Pokemon/Turtwig/male_front.png");
            _normal = new Palette("Pokemon/Turtwig/normal.pal");
            _shiny = new Palette("Pokemon/Turtwig/shiny.pal");
            
            _paletteEffect.Parameters["NormalPalette"].SetValue(_normal.Colors.Select(c => c.ToVector4()).ToArray());
            _paletteEffect.Parameters["SwapPalette"].SetValue(_shiny.Colors.Select(c => c.ToVector4()).ToArray());
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _swap = Keyboard.GetState().IsKeyDown(Keys.K);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _paletteEffect.Parameters["UseSwapPalette"].SetValue(_swap);
            
            _spriteBatch.Begin(effect: _paletteEffect);
            _spriteBatch.Draw(_texture, Vector2.Zero, Color.White);
            _spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}