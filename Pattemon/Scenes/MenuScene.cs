using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PatteLib;
using Pattemon.Engine;

namespace Pattemon.Scenes;

public class MenuScene : Scene
{
    private Texture2D _dummy;
    private Texture2D _selector;
    private Texture2D _icons;
    
    private const int IconWidth = 28;
    private const int IconHeight = 24;
    
    private int _optionCursor;
    
    public MenuScene(string name, Game game, string contentDirectory = "Content") : base(name, game, contentDirectory)
    {
    }

    public override void Load()
    {
        _dummy = Content.Load<Texture2D>("MenuComplete");
        _selector = Content.Load<Texture2D>("MenuSelector");
        _icons = Content.Load<Texture2D>("Icons/MenuIcons");
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
            if (_optionCursor == 5)
            {
                SceneManager.Push(new OptionScene("name", _game));
                SceneManager.Pop();
            }
            else if (_optionCursor == 6)
            {
                SceneManager.Pop();
            }
        }
    }

    protected override void Draw2D(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
        RenderCore.SetTopScreen();
        spriteBatch.Begin();
        spriteBatch.Draw(_dummy, (new Vector2(38, 0) * 4), Color.White);
        spriteBatch.Draw(_selector, (new Vector2(39, 1 + _optionCursor * 6) * 4), Color.White);
        
        for (int i = 0; i < 7; i++)
        {
            var iconPosition = new Vector2(38, 0) * 4 + new Vector2(8, 8 + IconHeight * i);
            var sourceRectangle = new Rectangle(0, IconHeight * i, IconWidth, IconHeight);
            
            if (i == _optionCursor)
            {
                sourceRectangle = new Rectangle(IconWidth, IconHeight * i, IconWidth, IconHeight);
            }

            spriteBatch.Draw(_icons, iconPosition, sourceRectangle, Color.White);
        }
        spriteBatch.End();
    }

    protected override void Draw3D(GameTime gameTime, float delta)
    {
        // throw new System.NotImplementedException();
    }
}