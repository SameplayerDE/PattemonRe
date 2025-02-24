using System;
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
using Pattemon.Scenes.ChoosePokemon;
using Pattemon.Scenes.Inventory;
using Pattemon.Scenes.OptionMenu;
using Pattemon.Scenes.Television;
using Pattemon.Scenes.TrainerCard;
using Pattemon.Scenes.WorldMap;

namespace Pattemon.Scenes.FieldMenu;

public class FieldMenuScene : SceneA
{

    private const int _stateProcess = 0;
    private const int _stateExit = 1;
    private const int _stateApplicationInit = 2;
    private const int _stateApplicationProcess = 3;
    private const int _stateApplicationExit = 4;
    private int _state = _stateProcess;

    private static int _index = 0;
    private int _fade = 0;
    
    private Window _window;

    private int _windowX;
    private int _windowY;
    private int _windowH;
    private int _windowW;
    private int _iconX;
    private int _iconY;
    private int _iconW;
    private int _iconH;
    private int _iconCenterX;
    private int _iconCenterY;
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
    
    // runtime vars
    private float _iconRotationValue;
    private float _iconRotationTime;
    private const float _iconRotationStrength = 0.3f;
    private const float _iconRotationSpeed = 0.5f;
    
    private float _iconScaleValue = 1;
    private float _iconScaleMinValue = 1;
    private float _iconScaleMaxValue = 1.3f;
    private float _iconScaleTime;
    private int _iconScaleState = 2; // 0 = grow, 1 = shrink, 2 = done
    private const float _iconScaleSpeed = 3f;
    
    public FieldMenuScene(Game game, object args = null, string contentDirectory = "Content") : base(game, args, contentDirectory)
    {
    }

    public override bool Init()
    {
        ResetIconAnimationVars(2);
        
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
                if (_fade == 1)
                {
                    if (RenderCore.IsScreenTransitionDone())
                    {
                        _fade = 0;
                        _state = _stateApplicationInit;
                        MessageSystem.Publish("Application Open");
                    }
                    return false;
                }
                if (_fade == 2)
                {
                    if (RenderCore.IsScreenTransitionDone())
                    {
                        _fade = 0;
                    }
                    return false;
                }
                ProcessInput();
                
                // calculate scale for icon animation
                if (_iconScaleState != 2)
                {
                    _iconScaleTime += delta;
                    switch (_iconScaleState)
                    {
                        case 0:
                        {
                            // grow
                            _iconScaleValue += _iconScaleSpeed * delta;
                            if (_iconScaleValue >= _iconScaleMaxValue)
                            {
                                _iconScaleValue = _iconScaleMaxValue;
                                _iconScaleState = 1;
                            }
                            break;
                        }
                        case 1:
                        {
                            // shrink
                            _iconScaleValue -= _iconScaleSpeed * delta;
                            if (_iconScaleValue <= _iconScaleMinValue)
                            {
                                _iconScaleValue = _iconScaleMinValue;
                                _iconScaleState = 2;
                            }
                            break;
                        }
                    }
                }
                else
                {
                    _iconRotationTime += delta;

                    const float maxTilt = 1.5f;
                    const float frequency = 1f;
                    float angleSpeed = MathF.PI * 2 * frequency;
                    float speed = MathF.Cos(_iconRotationTime * angleSpeed);
                    _iconRotationValue += speed * delta * maxTilt;
                }
                break;
            }
            case _stateExit:
            {
                return true;
            }
            case _stateApplicationInit:
            {
                if (RenderCore.IsScreenTransitionDone())
                {
                    if (Process.Init())
                    {
                        _state = _stateApplicationProcess;
                    }
                }
                break;
            }
            case _stateApplicationProcess:
            {
                if (Process.Update(gameTime, delta))
                {
                    _state = _stateApplicationExit;
                }
                break;
            }
            case _stateApplicationExit:
            {
                if (Process.Exit())
                {
                    Process = null;
                    MessageSystem.Publish("Application Close");
                    _state = _stateProcess;
                    _iconRotationTime = 0f;
                    _iconRotationValue = 0f;
                    _fade = 2;
                    RenderCore.StartScreenTransition(500, RenderCore.TransitionType.AlphaIn);
                }
                break;
            }
        }
        return false;
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
        if (_state == _stateApplicationProcess)
        {
            Process.Draw(spriteBatch, gameTime, delta);
        }
        if (_state != _stateProcess)
        {
            return;
        }
        RenderCore.SetTopScreen();
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _window.Draw(spriteBatch, gameTime, delta);
        spriteBatch.Draw(_cursorTexture, (new Vector2(39, 1 + _index * 6) * 4), Color.White);
        
        for (int i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            
            var iconCenter = new Vector2(_iconCenterX, _iconCenterY);
            var iconOffset = iconCenter;
            var iconPosition = new Vector2(_iconX, _iconY + _iconSpacing * i) + iconOffset;
            var sourceRectangle = new Rectangle(0, _iconH * entry.IconIndex, _iconW, _iconH);

            if (i == _index)
            {
                sourceRectangle = new Rectangle(_iconW, _iconH * entry.IconIndex, _iconW, _iconH);
                spriteBatch.Draw(_iconSheetTexture, iconPosition, sourceRectangle, Color.White, _iconRotationValue, iconCenter, Vector2.One * _iconScaleValue, SpriteEffects.None, 0f);
            }
            else
            {
                spriteBatch.Draw(_iconSheetTexture, iconPosition, sourceRectangle, Color.White, 0f, iconCenter, Vector2.One, SpriteEffects.None, 0f);
            }
            
            RenderCore.WriteText(entry.Text, new Vector2(_textX, _textY + _textSpacing * i), ColorCombination.Dark);
        }
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
                OnClick = () =>
                {
                    // open pokedex
                    _state = _stateExit;
                }
            });
        }
        if (PlayerData.HasPokemon)
        {
            _entries.Add(new FieldMenuEntry()
            {
                IconIndex = 1,
                Text = "POKEMON",
                OnClick = () =>
                {
                    // open team
                    _state = _stateExit;
                }
            });
        }
        _entries.Add(new FieldMenuEntry()
        {
            IconIndex = 2,
            Text = "BEUTEL",
            OnClick = () =>
            {
                if (!PlayerData.HasPoketch)
                {
                    MessageSystem.Publish("Poketch", new ChoosePokemonScene(_game));
                    PlayerData.HasPoketch = true;
                }
                _state = _stateExit;
               // if (!HasProcess)
               // {
               //     RenderCore.StartScreenTransition(250, RenderCore.TransitionType.AlphaOut);
               //     Process = new InventoryScene(_game);
               //     _fade = 1;
               // }
            }
        });
        _entries.Add(new FieldMenuEntry()
        {
            IconIndex = 3,
            Text = "Spielername",
            OnClick = () =>
            {
                if (!HasProcess)
                {
                    RenderCore.StartScreenTransition(250, RenderCore.TransitionType.AlphaOut);
                    Process = new TrainerCardScene(_game);
                    _fade = 1;
                }
            }
        });
        _entries.Add(new FieldMenuEntry()
        {
            IconIndex = 4,
            Text = "SICHERN",
            OnClick = () =>
            {
                // save
                _state = _stateExit;
            }
        });
        _entries.Add(new FieldMenuEntry()
        {
            IconIndex = 5,
            Text = "OPTIONEN",
            OnClick = () =>
            {
                if (!HasProcess)
                {
                    RenderCore.StartScreenTransition(250, RenderCore.TransitionType.AlphaOut);
                    Process = new OptionMenuScene(_game);
                    _fade = 1;
                }
            }
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
        _iconCenterX = _iconW / 2;
        _iconCenterY = _iconH / 2;
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
            ResetIconAnimationVars();
            
            _index = Utils.Wrap(_index, 0, _entries.Count - 1);
            // animate new index icon
        }
    }

    public void ResetIconAnimationVars(int scaleState = 0)
    {
        _iconScaleTime = 0f;
        _iconScaleState = scaleState;
        _iconScaleValue = 1;
            
        _iconRotationTime = 0f;
        _iconRotationValue = 0f;
    }
    
    #endregion
    
}