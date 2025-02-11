using System;
using HxGLTF;
using HxGLTF.Implementation;
using InputLib;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PatteLib;
using PatteLib.Graphics;
using Pattemon.Engine;
using Pattemon.Graphics;
using Camera = Pattemon.Engine.Camera;
using Utils = PatteLib.Utils;

namespace Pattemon.Scenes.ChoosePokemon;

public class ChoosePokemonScene : SceneA
{

    private const int CHOOSE_STARTER_MAIN_FADE_IN = 0;
    private const int CHOOSE_STARTER_MAIN_WAIT_FADE_IN = 1;
    private const int CHOOSE_STARTER_MAIN_CAMERA_MOVE = 2;
    private const int CHOOSE_STARTER_MAIN_LOOP = 3;
    private const int CHOOSE_STARTER_MAIN_FADE_OUT = 4;
    private const int CHOOSE_STARTER_MAIN_WAIT_FADE_OUT = 5;

    private int _state = CHOOSE_STARTER_MAIN_FADE_IN;
    
    private Effect _worldShader;
    private Effect _buildingShader;
    private Camera _camera;
    
    private Texture2D _selectorTexture;
    private int _index = 0;

    private GameModel _background;
    private GameModel _openAnimation;
    private GameModel _ballA;
    private GameModel _ballB;
    private GameModel _ballC;
    private GameModel _case;
    private Vector3 _position;
    private float _speed = 2.0f; // Geschwindigkeit der Bewegung
    private Vector2 _cursorPosition = Vector2.Zero;
    
    public ChoosePokemonScene(Game game, object args = null, string contentDirectory = "Content") : base(game, args, contentDirectory)
    {
    }

    public override bool Init()
    {
        _worldShader = _content.Load<Effect>("Shaders/WorldShader");
        _buildingShader = _content.Load<Effect>("Shaders/BuildingShader");
        
        _camera = new Camera();
        
        _camera.InitWithTarget(new Vector3(0, 0, 0), 300, new Vector3(MathHelper.ToRadians(-30), 0, 0), MathHelper.ToRadians(22), CameraProjectionType.Perspective, false);
        _camera.SetAsActive();
        
        GraphicsCore.LoadTexture("textbox", "Content/textbox.png");
        _selectorTexture = _content.Load<Texture2D>("Icons/Choose_Selector");
        
        var gltfFile = GLTFLoader.Load(@"A:\ModelExporter\Platin\export_output\output_assets\psel_all\psel_all");
        _openAnimation = GameModel.From(_graphics, gltfFile);
        _openAnimation.OnAnimationCompleted += () => _state = CHOOSE_STARTER_MAIN_CAMERA_MOVE;
        
        gltfFile = GLTFLoader.Load(@"A:\ModelExporter\Platin\export_output\output_assets\pmsel_bg\pmsel_bg");
        _background = GameModel.From(_graphics, gltfFile);
        _background.Scale *= 3.6f;
        _background.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(180));
        
        gltfFile = GLTFLoader.Load(@"A:\ModelExporter\Platin\export_output\output_assets\psel_trunk\psel_trunk");
        _case = GameModel.From(_graphics, gltfFile);
        
        gltfFile = GLTFLoader.Load(@"A:\ModelExporter\Platin\export_output\output_assets\psel_mb_a\psel_mb_a");
        _ballA = GameModel.From(_graphics, gltfFile);
        
        gltfFile = GLTFLoader.Load(@"A:\ModelExporter\Platin\export_output\output_assets\psel_mb_b\psel_mb_b");
        _ballB = GameModel.From(_graphics, gltfFile);
         
        gltfFile = GLTFLoader.Load(@"A:\ModelExporter\Platin\export_output\output_assets\psel_mb_c\psel_mb_c");
        _ballC = GameModel.From(_graphics, gltfFile);
        return true;
    }
    
    public override bool Exit()
    {
        _game.Exit();
        return true; // Exit wurde implementiert
    }
    
    public override bool Update(GameTime gameTime, float delta)
    {
        switch (_state)
        {
            case CHOOSE_STARTER_MAIN_FADE_IN:
            {
                RenderCore.StartScreenTransition(1000, RenderCore.TransitionType.AlphaIn);
                _state++;
                break;
            }
            case CHOOSE_STARTER_MAIN_WAIT_FADE_IN:
            {
                if (RenderCore.IsScreenTransitionDone())
                {
                    if (!_openAnimation.IsPlaying)
                    {
                        _openAnimation.Play(0);
                    }
                    _openAnimation.Update(gameTime);
                }
                break;
            }
            case CHOOSE_STARTER_MAIN_CAMERA_MOVE:
            {
                _camera.InitWithTarget(new Vector3(0, 0, 36), 200, new Vector3(MathHelper.ToRadians(-50), 0, 0), MathHelper.ToRadians(22), CameraProjectionType.Perspective, false);
                _state++;
                break;
            }
            case CHOOSE_STARTER_MAIN_LOOP:
            {
                
                _camera.SetClipping(10f, 1000f);
                
                switch (_index) {
                    case 0:
                        _cursorPosition.X = 78;
                        _cursorPosition.Y = 55;
                        break;
                    case 1:
                        _cursorPosition.X = 130;
                        _cursorPosition.Y = 82;
                        break;
                    case 2:
                        _cursorPosition.X = 172;
                        _cursorPosition.Y = 50;
                        break;
                }

                _cursorPosition += new Vector2(0, MathF.Sin((float)gameTime.TotalGameTime.TotalSeconds * 5) * 5);
               
                // check selection
                // update graphics
                var indexChanged = false;
                
                if (KeyboardHandler.IsKeyDownOnce(Keys.Left))
                {
                    _index--;
                    indexChanged = true;
                }

                if (KeyboardHandler.IsKeyDownOnce(Keys.Right))
                {
                    _index++;
                    indexChanged = true;
                }
                if (indexChanged)
                {
                    _index = MathHelper.Clamp(_index, 0, 2);
                    _ballA.Stop();
                    _ballA.Reset();
                    _ballB.Stop();
                    _ballB.Reset();
                    _ballC.Stop();
                    _ballC.Reset();
                }
                if (_index == 0)
                {
                    _ballA.Play(0);
                    _ballA.Update(gameTime);
                }
                if (_index == 1)
                {
                    _ballB.Play(0);
                    _ballB.Update(gameTime);
                }
                if (_index == 2)
                {
                    _ballC.Play(0);
                    _ballC.Update(gameTime);
                }

                if (KeyboardHandler.IsKeyDownOnce(Keys.Enter))
                {
                    _state++;
                }
                // if selection is made state++
                //ExecuteLoop(gameTime, delta);
                break;
            }
            case CHOOSE_STARTER_MAIN_FADE_OUT:
            {
                RenderCore.StartScreenTransition(1000, RenderCore.TransitionType.AlphaOut);
                _state++;
                break;
            }
            case CHOOSE_STARTER_MAIN_WAIT_FADE_OUT:
            {
                if (RenderCore.IsScreenTransitionDone())
                {
                    GraphicsCore.FreeTexture("textbox");
                    _ballA.Dispose();
                    _ballB.Dispose();
                    _ballC.Dispose();
                    _case.Dispose();
                    _background.Dispose();
                    Exit();
                }
                break;
            }
        }
        
        _camera.ComputeViewMatrix();
        
        return false;
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        _graphics.Clear(ColorUtils.FromHex("182921"));
        RenderCore.DrawModel(gameTime, _buildingShader, _background, offset: new Vector3(0, -32, 38));
        if (_state == CHOOSE_STARTER_MAIN_WAIT_FADE_IN)
        {
            RenderCore.DrawModel(gameTime, _buildingShader, _openAnimation);
            RenderCore.DrawModel(gameTime, _buildingShader, _openAnimation, alpha: true);
        }
        if (_state >= CHOOSE_STARTER_MAIN_CAMERA_MOVE)
        {
            RenderCore.DrawModel(gameTime, _buildingShader, _case);
            RenderCore.DrawModel(gameTime, _buildingShader, _ballA, offset: new Vector3(-44, -4, 32));
            RenderCore.DrawModel(gameTime, _buildingShader, _ballB, offset: new Vector3(0, -4, 62));
            RenderCore.DrawModel(gameTime, _buildingShader, _ballC, offset: new Vector3(38, -4, 26));
            
            RenderCore.DrawModel(gameTime, _buildingShader, _case, alpha: true);
            RenderCore.DrawModel(gameTime, _buildingShader, _ballA, offset: new Vector3(-44, -4, 32), alpha: true);
            RenderCore.DrawModel(gameTime, _buildingShader, _ballB, offset: new Vector3(0, -4, 62), alpha: true);
            RenderCore.DrawModel(gameTime, _buildingShader, _ballC, offset: new Vector3(38, -4, 26), alpha: true);
            
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(_selectorTexture, _cursorPosition, null, Color.White, 0f, _selectorTexture.Bounds.Center.ToVector2(), 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(GraphicsCore.GetTexture("textbox"), new Vector2(0, 18) * 8, Color.White);
            RenderCore.WriteText("Möchtest du dieses Pokemon?", new Vector2(4, 19) * 8);
            spriteBatch.End();
        }
    }
}