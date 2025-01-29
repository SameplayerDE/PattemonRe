using System;
using System.Collections.Generic;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PatteLib;
using Pattemon.Data;
using Pattemon.Engine;
using Pattemon.Global;

namespace Pattemon.Scenes;

public class MenuScene : Scene
{
    private Texture2D _dummy;
    private Texture2D _selector;
    private Texture2D _icons;

    private const int IconWidth = 28;
    private const int IconHeight = 24;

    private int _optionCursor;
    private List<int> _availableOptions = new();

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
        // Erstelle die Liste der verfügbaren Optionen basierend auf Spielzustand
        _availableOptions.Clear();

        if (PlayerData.HasPokedex)
            _availableOptions.Add(0);
        if (PlayerData.HasPokemon)
            _availableOptions.Add(1);
        // Füge die anderen Menüpunkte hinzu
        _availableOptions.AddRange(new[] { 2, 3, 4, 5, 6 });

        // Stelle sicher, dass der Cursor auf einer gültigen Option steht
        _optionCursor = ApplicationStorage.ContextMenuIndex;
        _optionCursor = Math.Clamp(_optionCursor, 0, _availableOptions.Count - 1);
    }

    public override void Close()
    {
        // Speichere den aktuellen Menüindex
        ApplicationStorage.ContextMenuIndex = _optionCursor;
        base.Close();
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

        // Wrap den Cursor innerhalb der verfügbaren Optionen
        _optionCursor = Utils.Wrap(_optionCursor, 0, _availableOptions.Count - 1);

        if (KeyboardHandler.IsKeyDownOnce(Keys.Enter))
        {
            int selectedOption = _availableOptions[_optionCursor];

            if (selectedOption == 5) // Optionen
            {
                SceneManager.Push(new OptionScene("name", _game));
            }
            else if (selectedOption == 6) // Zurück
            {
                SceneManager.Pop();
            }
        }
    }

    protected override void Draw2D(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
        RenderCore.SetTopScreen();
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        spriteBatch.Draw(_dummy, (new Vector2(38, 0) * 4), Color.White);
        spriteBatch.Draw(_selector, (new Vector2(39, 1 + _optionCursor * 6) * 4), Color.White);

        for (int i = 0; i < _availableOptions.Count; i++)
        {
            int optionIndex = _availableOptions[i];

            var iconPosition = new Vector2(38, 0) * 4 + new Vector2(8, 8 + IconHeight * i);
            var sourceRectangle = new Rectangle(0, IconHeight * optionIndex, IconWidth, IconHeight);

            if (i == _optionCursor)
            {
                sourceRectangle = new Rectangle(IconWidth, IconHeight * optionIndex, IconWidth, IconHeight);
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
