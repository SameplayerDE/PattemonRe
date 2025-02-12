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
    
    private enum State
    {
        Process = 0,
        FadeOut,
        WaitFadeOut,
    }
    
    private Texture2D _dummy;
    private Texture2D _selector;
    private Texture2D _icons;
    
    private Graphics.Window _window;
    
    private const int _windowX = 0;
    private const int _windowY = 0;
    private const int _windowH = 0;
    private const int _windowW = 0;
    private const int _iconX = 0;
    private const int _iconY = 0;
    private const int _iconW = 28;
    private const int _iconH = 24;
    private const int _textX = 0;
    private const int _textY = 0;

    private int _optionCursor;
    private List<int> _availableOptions = new();
    private State _state = 0;

    public MenuScene(string name, Game game, string contentDirectory = "Content") : base(name, game, contentDirectory)
    {
    }

    public override bool Load()
    {
        _window = Graphics.Window.Create(38 * 4, 0, 13 * 8, 17 * 8);
        _dummy = Content.Load<Texture2D>("MenuComplete");
        _selector = Content.Load<Texture2D>("MenuSelector");
        _icons = Content.Load<Texture2D>("Icons/MenuIcons");
        return true;
    }

    protected override bool Unload()
    {
        Content.Unload();
        return true;
    }

    public override bool Init()
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
        return true;
    }

    public override void Exit()
    {
        // Speichere den aktuellen Menüindex
        _state = 0;
        ApplicationStorage.ContextMenuIndex = _optionCursor;
    }

    public override bool Update(GameTime gameTime, float delta)
    {
        if (_state == State.Process)
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
                    _state++;
                }
                else if (selectedOption == 6) // Zurück
                {
                    return true;
                }
            }
        }

        if (_state == State.FadeOut)
        {
            RenderCore.StartScreenTransition(1000, RenderCore.TransitionType.AlphaOut);
            SceneManager.Next(Scene.PlayerMenuOptionMenu);
            _state++;
        }

        if (_state == State.WaitFadeOut)
        {
            if (RenderCore.IsScreenTransitionDone())
            {
                return true;
            }
        }

        return false;
    }

    protected override void Draw2D(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
        RenderCore.SetTopScreen();
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _window.Draw(spriteBatch, gameTime, delta);
        //spriteBatch.Draw(_dummy, (new Vector2(38, 0) * 4), Color.White);
        spriteBatch.Draw(_selector, (new Vector2(39, 1 + _optionCursor * 6) * 4), Color.White);

        for (int i = 0; i < _availableOptions.Count; i++)
        {
            int optionIndex = _availableOptions[i];

            var iconPosition = new Vector2(38, 0) * 4 + new Vector2(8, 8 + _iconH * i);
            var sourceRectangle = new Rectangle(0, _iconH * optionIndex, _iconW, _iconH);

            if (i == _optionCursor)
            {
                sourceRectangle = new Rectangle(_iconW, _iconH * optionIndex, _iconW, _iconH);
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
