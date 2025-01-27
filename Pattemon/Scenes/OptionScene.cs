using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PatteLib;
using Pattemon.Engine;

namespace Pattemon.Scenes;

public class OptionScene : Scene
{
    private Texture2D _dummy;
    private Texture2D _selector;
    
    private int _optionCursor;
    
    public OptionScene(string name, Game game, string contentDirectory = "Content") : base(name, game, contentDirectory)
    {
    }

    public override void Load()
    {
        _dummy = Content.Load<Texture2D>("DummyOptions");
        _selector = Content.Load<Texture2D>("OptionSelector");
    }

    public override void Unload()
    {
        Content.Unload();
    }

    public override void Init()
    {
        _optionCursor = 0;
    }

    public override void Update(GameTime gameTime, float delta)
    {
        if (KeyboardHandler.IsKeyDownOnce(Keys.Down))
        {
            _optionCursor++;
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.Up))
        {
            _optionCursor--;
        }
        _optionCursor = Utils.Wrap(_optionCursor, 0, 6);

        if (KeyboardHandler.IsKeyDownOnce(Keys.Enter))
        {
            if (_optionCursor == 6)
            {
                SceneManager.Pop();
            }
        }
    }

    protected override void Draw2D(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
        RenderCore.SetTopScreen();
        GraphicsDevice.Clear(Color.Black);
        spriteBatch.Begin();
        spriteBatch.Draw(_dummy, Vector2.Zero, Color.White);
        spriteBatch.Draw(_selector, (new Vector2(2, 6 + _optionCursor * 4) * 4), Color.White);
        spriteBatch.End();
        
        RenderCore.SetBottomScreen();
        GraphicsDevice.Clear(Color.Black);
    }

    protected override void Draw3D(GameTime gameTime, float delta)
    {
        // throw new System.NotImplementedException();
    }
}