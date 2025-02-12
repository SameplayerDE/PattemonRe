using System;
using System.Collections.Generic;
using System.Linq;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PatteLib.Data;
using Pattemon.Audio;
using Pattemon.Engine;
using Pattemon.Graphics;

namespace Pattemon.Scenes.OptionMenu;

public class OptionMenuScene(Game game) : SceneA(game)
{
    private class OptionEntry
    {
        public int count;
        public int index;
        public string[] options;
    }
    
    private static class State {
        public const int SetupMenuVisuals = 0;
        public const int WaitForFadeIn = 1;
        public const int HandleInput = 2;
        public const int ConfirmNewSettings = 3;
        public const int WaitConfirmNewSettings = 4;
        public const int HandleYesNoInput = 5;
        public const int StartVisualTeardown = 6;
        public const int WaitForFadeOut = 7;
        public const int Teardown = 8;
    }
    
    private static class Entry {
        public const int TextSpeed = 0;
        public const int SoundMode = 1;
        public const int BattleScene = 2;
        public const int BattleStyle = 3;
        public const int ButtonMode = 4;
        public const int MessageBox = 5;
        public const int Close = 6;
    }
    
    private Texture2D _backgroundTexture;
    private Texture2D _cursorTexture;
    private SpriteFont _font;
    
    private int _state = 0;
    private int _cursor = 0;

    private List<OptionEntry> _entries = [
        new OptionEntry
        {
            count = 3,
            index = 0,
            options = ["1", "2", "3"],
        },
        new OptionEntry
        {
            count = AudioMode.Count,
            index = 0,
            options = ["mono", "stereo", "surround"],
        },
        new OptionEntry
        {
            count = 2,
            index = 0,
            options = ["ein", "aus"],
        },
        new OptionEntry
        {
            count = 2,
            index = 0,
            options = ["tausch", "folge"],
        },
        new OptionEntry
        {
            count = 3,
            index = 0,
            options = ["normal", "start = x", "L = A"],
        },
        new OptionEntry
        {
            count = 20,
            index = 0,
            options = [],
        },
    ];
    
    private Options _options;

    public override bool Init()
    {
        // alloc data for options
        // get current settings
        _options = Data.OptionMenu.OptionMenuData.Options;

        _backgroundTexture = _content.Load<Texture2D>("DummyOptions");
        _cursorTexture = _content.Load<Texture2D>("OptionSelector");
        _font = _content.Load<SpriteFont>("Font");
        GraphicsCore.LoadTexture("dummy", @"A:\Coding\Survival\TestRender\Content\Pokemon\Turtwig\male_front.png");
        
        return true;
    }

    public override bool Exit()
    {
        // fetch data
        // set settings
        // save settings
        GraphicsCore.FreeTexture("dummy");
        Data.OptionMenu.OptionMenuData.Options = _options;
        // free data
        return true;
    }
    
    public override bool Update(GameTime gameTime, float delta)
    {
        // fetch data
        // choice
        Console.WriteLine(_state);
        Console.WriteLine(_cursor);
        
        switch (_state)
        {
            case State.SetupMenuVisuals:
                _entries[Entry.TextSpeed].index = _options.TextSpeed;
                _entries[Entry.SoundMode].index = _options.SoundMode;
                _entries[Entry.BattleScene].index = _options.Animations;
                _entries[Entry.BattleStyle].index = _options.BattleStyle;
                _entries[Entry.ButtonMode].index = _options.ButtonMapping;
                _entries[Entry.MessageBox].index = _options.TextBoxStyle;
                RenderCore.StartScreenTransition(500, RenderCore.TransitionType.SlideOut);
                break;
            case State.WaitForFadeIn:
                if (!RenderCore.IsScreenTransitionDone())
                {
                    return false;
                }
                break;
            case State.HandleInput:
                
                
                // check for close
                if (KeyboardHandler.IsKeyDownOnce(Keys.A) && _cursor == Entry.Close)
                {
                    // check for changes
                    // 
                    // play sound
                    _state = State.StartVisualTeardown;
                }
                
                // process main input
                if (_cursor != Entry.Close) {
                    var entry = _entries[_cursor];

                    if (KeyboardHandler.IsKeyDownOnce(Keys.Left))
                    {
                        entry.index = (entry.index + entry.count - 1) % entry.count;
                    }
                    else if (KeyboardHandler.IsKeyDownOnce(Keys.Right))
                    {
                        entry.index = (entry.index + 1) % entry.count;
                    }
                    _entries[_cursor] = entry;
                }
                
                if (KeyboardHandler.IsKeyDownOnce(Keys.Up))
                {
                    _cursor = (_cursor + 7 - 1) % 7;
                    // play sound
                    AudioCore.PlaySound(0x00);
                }
                else if (KeyboardHandler.IsKeyDownOnce(Keys.Down))
                {
                    _cursor = (_cursor + 1) % 7;
                    // play sound
                    AudioCore.PlaySound(0x00);
                }
                
                return false;
                break;
            case State.ConfirmNewSettings:
                //
                break;
            case State.WaitConfirmNewSettings:
                // if
                return false;
                break;
            case State.HandleYesNoInput:
                // if
                return false;
                break;
            case State.StartVisualTeardown:
                RenderCore.StartScreenTransition(500, RenderCore.TransitionType.SlideIn);
                // transition 3, 0, 0, 0x0, 6, 1
                break;
            case State.WaitForFadeOut:
                return RenderCore.IsScreenTransitionDone();
        }
        _state++;
        return false;
    }
    
    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
        RenderCore.SetBottomScreen();
        _graphics.Clear(Color.Black);
        
        RenderCore.SetTopScreen();
        _graphics.Clear(Color.Black);
        
        spriteBatch.Begin();
        spriteBatch.Draw(_backgroundTexture, Vector2.Zero, Color.White);
        spriteBatch.Draw(_cursorTexture, (new Vector2(8, 24 + 16 * _cursor)), Color.White);
        for (int i = 0; i < _entries.Count(); i++)
        {
            Color gray = new Color(90, 90, 82);
            Color lightGray = new Color(173, 189, 189);
            Color red = new Color(239, 33, 16);
            Color lightRed = new Color(255, 173, 189);

            var entry = _entries[i];
            
            if (i == Entry.MessageBox)
            {
                int x = 48 + (12 * 8 + 4) + 8;
                int y = i * 16 + 24 - 2;
                string output = "motiv  " + (entry.index + 1);
                spriteBatch.DrawString(_font, output.ToUpper(), new Vector2(x + 0, y + 1), lightRed);
                spriteBatch.DrawString(_font, output.ToUpper(), new Vector2(x + 1, y + 0), lightRed);
                spriteBatch.DrawString(_font, output.ToUpper(), new Vector2(x + 1, y + 1), lightRed);
                spriteBatch.DrawString(_font, output.ToUpper(), new Vector2(x, y), red);
                continue;
            }
            
            if (i == Entry.ButtonMode)
            {
                int x = (12 * 8 + 4) + 8;
                int y = i * 16 + 24 - 2;
                for (int j = 0; j < entry.count; j++)
                {
                    spriteBatch.DrawString(_font, entry.options[j].ToUpper(), new Vector2(x + 0, y + 1), entry.index == j ? lightRed : lightGray);
                    spriteBatch.DrawString(_font, entry.options[j].ToUpper(), new Vector2(x + 1, y + 0), entry.index == j ? lightRed : lightGray);
                    spriteBatch.DrawString(_font, entry.options[j].ToUpper(), new Vector2(x + 1, y + 1), entry.index == j ? lightRed : lightGray);
                    spriteBatch.DrawString(_font, entry.options[j].ToUpper(), new Vector2(x, y), entry.index == j ? red : gray);x += (int)_font.MeasureString(entry.options[j]).X + 16;
                }
                continue;
            }
            
            for (int j = 0; j < entry.count; j++)
            {
                int x = j * 48 + (12 * 8 + 4) + 8;
                int y = i * 16 + 24 - 2;

                
                spriteBatch.DrawString(_font, entry.options[j].ToUpper(), new Vector2(x + 0, y + 1), entry.index == j ? lightRed : lightGray);
                spriteBatch.DrawString(_font, entry.options[j].ToUpper(), new Vector2(x + 1, y + 0), entry.index == j ? lightRed : lightGray);
                spriteBatch.DrawString(_font, entry.options[j].ToUpper(), new Vector2(x + 1, y + 1), entry.index == j ? lightRed : lightGray);
                spriteBatch.DrawString(_font, entry.options[j].ToUpper(), new Vector2(x, y), entry.index == j ? red : gray);
            }
        }
        spriteBatch.End();
    }
}