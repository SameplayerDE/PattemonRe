using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BattleGround
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private RenderTarget2D _mainScreen;
        private RenderTarget2D _subScreen;
        
        private Effect _paletteEffect;
        private bool _swap = false;
        
        // assets

        private Pokemon _pokemon0;
        private Pokemon _pokemon1;
        
        private Palette _normal;
        private Palette _shiny;
        
        private Texture2D _front;
        private Texture2D _back;
        
        private Texture2D _battleBackground;
        private Texture2D _battlePlateNear;
        private Texture2D _battlePlateFar;
        private float xOffset = 0;
        private float xBackgroundOffset = 0;
        private float xInfoOffset = 0;
        
        private Texture2D _main;
        private Texture2D _mainOverlay;
        
        private Texture2D _enemyInfo;
        private Texture2D _info;
        
        private Texture2D _sub;
        private Texture2D _subBottomMenu;
        private float yBottomOffset = 0;
        
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _mainScreen = new RenderTarget2D(GraphicsDevice, 256, 192);
            _subScreen = new RenderTarget2D(GraphicsDevice, 256, 192);
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _paletteEffect = Content.Load<Effect>("PaletteSwapSprite");

            _pokemon0 = Pokemon.FromName(GraphicsDevice, "Abra");
            _pokemon1 = Pokemon.FromName(GraphicsDevice, "Chimchar");
            
            _main = Texture2D.FromFile(GraphicsDevice, "Assets/Dummy/main.png");
            _enemyInfo = Texture2D.FromFile(GraphicsDevice, "Assets/Interface/enemy_info.png");
            _info = Texture2D.FromFile(GraphicsDevice, "Assets/Interface/info.png");
            _sub = Texture2D.FromFile(GraphicsDevice, "Assets/Interface/sub_bg.png");
            _subBottomMenu = Texture2D.FromFile(GraphicsDevice, "Assets/Interface/bottom.png");
            _mainOverlay = Texture2D.FromFile(GraphicsDevice, "Assets/Dummy/sub_overlay.png");
            
            _battleBackground = Texture2D.FromFile(GraphicsDevice, "Assets/Dummy/battle_bg_0_d.png");
            _battlePlateNear = Texture2D.FromFile(GraphicsDevice, "Assets/Dummy/battle_plate_front.png");
            _battlePlateFar = Texture2D.FromFile(GraphicsDevice, "Assets/Dummy/battle_plate_back.png");
        }

        protected override void Update(GameTime gameTime)
        {

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            if (xOffset < 256)
            {
                xOffset += 256 * delta;
            }
            
            if (xBackgroundOffset < 64)
            {
                xBackgroundOffset += 64 * delta;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                xOffset = 0;
                xBackgroundOffset = 0;
                yBottomOffset = 0;
                xInfoOffset = 0;
            }

            if (xBackgroundOffset >= 64 && xOffset >= 256)
            {
                if (yBottomOffset < 48)
                {
                    yBottomOffset += 144 * delta;
                }
                
                if (xInfoOffset < 128)
                {
                    xInfoOffset += 512 * delta;
                }
            }

            if (xInfoOffset >= 128)
            {
                
            }
            
            base.Update(gameTime);
        }

        protected void DrawMain(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_mainScreen);
            GraphicsDevice.Clear(Color.Black);
            
            _spriteBatch.Begin();
            _spriteBatch.Draw(_battleBackground, new Vector2(-256 + 64 -xBackgroundOffset, 0), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0f);
            _spriteBatch.Draw(_battleBackground, new Vector2(64 - xBackgroundOffset, 0), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            _spriteBatch.End();
            
            _spriteBatch.Begin();
            _spriteBatch.Draw(_battlePlateNear, new Vector2(256 - 180 - xOffset, 8).ToPoint().ToVector2(), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            _spriteBatch.Draw(_battlePlateFar, new Vector2(-256 - 65 + xOffset, -45).ToPoint().ToVector2(), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            _spriteBatch.End();
            
            _pokemon0.Draw(_spriteBatch, 1, new Vector2(24, 80) + new Vector2(256 - xOffset, 0), new Rectangle(0, 0, 80, 80), _paletteEffect);
            _pokemon1.Draw(_spriteBatch, 0, new Vector2(153, 26) + new Vector2(-256 + xOffset, 0), new Rectangle(0, 0, 80, 80), _paletteEffect);
            
            _spriteBatch.Begin();
            _spriteBatch.Draw(_info, new Vector2(256 - xInfoOffset, (int)(94 + (xInfoOffset >= 128 ? Math.Sin(gameTime.TotalGameTime.TotalSeconds * 10) * 2 : 0))), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            _spriteBatch.Draw(_enemyInfo, new Vector2(-128 + xInfoOffset, 20), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            _spriteBatch.End();
            
            _spriteBatch.Begin();
            _spriteBatch.Draw(_mainOverlay, new Vector2(0, 0), null, Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            _spriteBatch.End();
            
            GraphicsDevice.SetRenderTarget(null);
        }
        
        protected void DrawSub(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_subScreen);
            GraphicsDevice.Clear(Color.Black);
            
            _spriteBatch.Begin();
            _spriteBatch.Draw(_sub, Vector2.Zero, Color.White);
            _spriteBatch.Draw(_subBottomMenu, new Vector2(0, 192 - yBottomOffset), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            _spriteBatch.End();
            
            GraphicsDevice.SetRenderTarget(null);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            
            DrawMain(gameTime);
            DrawSub(gameTime);
            
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_mainScreen, new Vector2(0, 0), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            _spriteBatch.Draw(_subScreen, new Vector2(0, 192), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            _spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}