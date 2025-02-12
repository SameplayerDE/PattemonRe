using System.Collections;
using System.Collections.Generic;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PatteLib;
using Pattemon.Engine;
using Pattemon.Global;
using Pattemon.Graphics;

namespace Pattemon.Scenes.FieldMenu;

public class FieldMenuScene : SceneA
{

    private const int _stateProcess = 0;
    private const int _stateExit = 1;
    private int _state = _stateProcess;

    private static int _index = 0;
    
    private Window _window;

    private int _windowX;
    private int _windowY;
    private int _windowH;
    private int _windowW;
    private int _iconX;
    private int _iconY;
    private int _iconW;
    private int _iconH;
    private int _iconPaddingX;
    private int _iconPaddingY;
    private int _iconSpacing;
    private int _textX;
    private int _textY;
    private int _textW;
    private int _textH = 16;
    private int _textPaddingX;
    private int _textPaddingY;
    private int _textSpacing;
    
    // matrix
    private List<FieldMenuEntry> _entries = [];
    
    // assets
    private Texture2D _cursorTexture;
    private Texture2D _iconSheetTexture;
    
    
    
    public FieldMenuScene(Game game, object args = null, string contentDirectory = "Content") : base(game, args, contentDirectory)
    {
    }

    public override bool Init()
    {
        // load assets
        _cursorTexture = _content.Load<Texture2D>("MenuSelector");
        _iconSheetTexture = _content.Load<Texture2D>("Icons/MenuIcons");
        
        // prepare menu matrix
        PrepareMatrix();
        
        // prepare variables
        PrepareVars();
        
        _window = Window.Create(_windowX, _windowY, _windowW, _windowH);
        return true;
    }
    
    public override bool Exit()
    {
        _content.Unload();
        _content.Dispose();
        return true;
    }

    public override bool Update(GameTime gameTime, float delta)
    {
        switch (_state)
        {
            case _stateProcess:
            {
                ProcessInput();
                break;
            }
            case _stateExit:
            {
                return true;
            }
        }
        return false;
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
        RenderCore.SetTopScreen();
        _graphics.Clear(Color.Transparent);
        if (_state != _stateProcess)
        {
            return;
        }
        
        spriteBatch.Begin();
        _window.Draw(spriteBatch, gameTime, delta);

        for (int i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            
            var iconPosition = new Vector2(_iconX, _iconY + _iconSpacing * i);
            var sourceRectangle = new Rectangle(0, _iconH * entry.IconIndex, _iconW, _iconH);

            if (i == _index)
            {
                sourceRectangle = new Rectangle(_iconW, _iconH * entry.IconIndex, _iconW, _iconH);
            }

            spriteBatch.Draw(_iconSheetTexture, iconPosition, sourceRectangle, Color.White);
            
            RenderCore.WriteText(entry.Text, new Vector2(_textX, _textY + _textSpacing * i), ColorCombination.Dark);
        }
        
        spriteBatch.Draw(_cursorTexture, (new Vector2(39, 1 + _index * 6) * 4), Color.White);
        spriteBatch.End();
    }
    
    #region
    
    private void PrepareMatrix()
    {
        if (PlayerData.HasPokedex)
        {
            _entries.Add(new FieldMenuEntry()
            {
                IconIndex = 0,
                Text = "POKEDEX",
            });
        }
        if (PlayerData.HasPokemon)
        {
            _entries.Add(new FieldMenuEntry()
            {
                IconIndex = 1,
                Text = "POKEMON",
            });
        }
        _entries.Add(new FieldMenuEntry()
        {
            IconIndex = 2,
            Text = "BEUTEL"
        });
        _entries.Add(new FieldMenuEntry()
        {
            IconIndex = 3,
            Text = "Spielername"
        });
        _entries.Add(new FieldMenuEntry()
        {
            IconIndex = 4,
            Text = "SICHERN"
        });
        _entries.Add(new FieldMenuEntry()
        {
            IconIndex = 5,
            Text = "OPTIONEN"
        });
        _entries.Add(new FieldMenuEntry()
        {
            IconIndex = 6,
            Text = "BEENDEN",
            OnClick = () =>
            {
                _state = _stateExit;
            }
        });
    }

    private void PrepareVars()
    {
        _windowX = 152;
        _windowY = 0;
        _windowH = 16 + _entries.Count * 24;
        _windowW = 104;
        
        _iconPaddingX = _iconPaddingY = 8;
        _iconX = _windowX + _iconPaddingX;
        _iconY = _windowY + _iconPaddingY;
        _iconW = 28;
        _iconH = 24;
        _iconSpacing = _iconH;
        
        _textPaddingX = _iconPaddingX + _iconW;
        _textPaddingY = 12;
        _textX = _windowX + _textPaddingX;
        _textY = _windowY + _textPaddingY;
        _textSpacing = _textH + 8;
    }
    
    private void ProcessInput()
    {
        var indexChanged = false;
        if (KeyboardHandler.IsKeyDownOnce(Keys.Up))
        {
            _index--;
            indexChanged = true;
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.Down))
        {
            _index++;
            indexChanged = true;
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.Enter))
        {
            _entries[_index].OnClick?.Invoke();
        }

        if (indexChanged)
        {
            _index = Utils.Wrap(_index, 0, _entries.Count - 1);
            // animate new index icon
        }
    }
    
    #endregion
    
}