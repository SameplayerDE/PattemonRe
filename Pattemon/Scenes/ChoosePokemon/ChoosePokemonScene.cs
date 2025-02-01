using System;
using HxGLTF;
using HxGLTF.Implementation;
using InputLib;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pattemon.Engine;
using Utils = PatteLib.Utils;

namespace Pattemon.Scenes.ChoosePokemon;

public class ChoosePokemonScene : SceneA
{

    private const int PlayOpenAnimation = 0;
    private const int ReplaceMeshes = 1;
    private const int SelectPokemon = 2;
    private const int CleanUp = 3;

    private int _state = PlayOpenAnimation;
    
    private Effect _worldShader;
    private Effect _buildingShader;
    private Camera _camera;
    
    private Texture2D _selectorTexture;
    private int _index = 0;
    
    private GameModel _openAnimation;
    private GameModel _ballA;
    private GameModel _ballB;
    private GameModel _ballC;
    private GameModel _case;
    private Vector3 _position;
    private float _speed = 2.0f; // Geschwindigkeit der Bewegung
    
    public ChoosePokemonScene(Game game, object args = null, string contentDirectory = "Content") : base(game, args, contentDirectory)
    {
    }

    public override bool Init()
    {
        _worldShader = _content.Load<Effect>("Shaders/WorldShader");
        _buildingShader = _content.Load<Effect>("Shaders/BuildingShader");
        
        _camera = new Camera();
        _camera.InitWithPosition(new Vector3(0, 9, 14) * 16, 10, new Vector3(MathHelper.ToRadians(-30), 0, 0), MathHelper.ToRadians(22), CameraProjectionType.Perspective);
        //_normalCamera.InitWithTarget(_target, 10, Vector3.Zero, MathHelper.ToRadians(75), CameraProjectionType.Perspective, true);
        _camera.SetClipping(0.01f, 100000f);
        
        _selectorTexture = _content.Load<Texture2D>("Icons/Choose_Selector");
        
        var gltfFile = GLTFLoader.Load(@"A:\ModelExporter\Platin\export_output\output_assets\psel_all\psel_all");
        _openAnimation = GameModel.From(_graphics, gltfFile);

        _openAnimation.OnAnimationCompleted += () => _state = ReplaceMeshes;
        
        gltfFile = GLTFLoader.Load(@"A:\ModelExporter\Platin\export_output\output_assets\psel_trunk\psel_trunk");
        _case = GameModel.From(_graphics, gltfFile);
        _case.Scale /= 16;
        
        gltfFile = GLTFLoader.Load(@"A:\ModelExporter\Platin\export_output\output_assets\psel_mb_a\psel_mb_a");
        _ballA = GameModel.From(_graphics, gltfFile);
        _ballA.Scale /= 16;
        
        gltfFile = GLTFLoader.Load(@"A:\ModelExporter\Platin\export_output\output_assets\psel_mb_b\psel_mb_b");
        _ballB = GameModel.From(_graphics, gltfFile);
        _ballB.Scale /= 16;
         
        gltfFile = GLTFLoader.Load(@"A:\ModelExporter\Platin\export_output\output_assets\psel_mb_c\psel_mb_c");
        _ballC = GameModel.From(_graphics, gltfFile);
        _ballC.Scale /= 16;

        return true;
    }

    public override bool Update(GameTime gameTime, float delta)
    {
        
        if (_state == PlayOpenAnimation)
        {
            if (RenderCore.IsScreenTransitionDone())
            {
                if (!_openAnimation.IsPlaying)
                {
                    _openAnimation.Play(0);
                }
                _openAnimation.Update(gameTime);
            }
        }

        if (_state == ReplaceMeshes)
        {
            _state = SelectPokemon;
            
            if (_index == 0)
            {
                _ballA.Play(0);
            }

            if (_index == 1)
            {
                _ballB.Play(0);
            }

            if (_index == 2)
            {
                _ballC.Play(0);
            }
        }
        
        if (_state == SelectPokemon)
        {
            bool changed = false;
            if (KeyboardHandler.IsKeyDownOnce(Keys.Left))
            {
                _index--;
                changed = true;
            }
            if (KeyboardHandler.IsKeyDownOnce(Keys.Right))
            {
                _index++;
                changed = true;
            }

            if (changed)
            {
                _index = Utils.Wrap(_index, 0, 2);
                _ballA.Stop();
                _ballB.Stop();
                _ballC.Stop();

                if (_index == 0)
                {
                    _ballA.Play(0);
                }

                if (_index == 1)
                {
                    _ballB.Play(0);
                }

                if (_index == 2)
                {
                    _ballC.Play(0);
                }
            }
            
            _ballA.Update(gameTime);
            _ballB.Update(gameTime);
            _ballC.Update(gameTime);
        }
        
        _camera.SetAsActive();
        _camera.CaptureTarget(ref _position);
        _camera.ComputeViewMatrix();
        
        return true;
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (_state == PlayOpenAnimation)
        {
            RenderCore.DrawModel(gameTime, _buildingShader, _openAnimation);
        }

        if (_state >= ReplaceMeshes)
        {
            float divisor = 1f;
            Vector3 scale = Vector3.One / divisor;

            _case.Scale = scale;
            _ballA.Scale = scale;
            _ballB.Scale = scale;
            _ballC.Scale = scale;
            
            RenderCore.DrawModel(gameTime, _buildingShader, _case);
            RenderCore.DrawModel(gameTime, _buildingShader, _ballA, offset: new Vector3(-44, -4, 32) / divisor);
            RenderCore.DrawModel(gameTime, _buildingShader, _ballB, offset: new Vector3(0, -4, 62) / divisor);
            RenderCore.DrawModel(gameTime, _buildingShader, _ballC, offset: new Vector3(38, -4, 26) / divisor);
            
            var cursorPosition = Vector2.Zero;
            
            switch (_index) {
                case 0:
                    cursorPosition.X = 78;
                    cursorPosition.Y = 55;
                    break;
                case 1:
                    cursorPosition.X = 130;
                    cursorPosition.Y = 82;
                    break;
                case 2:
                    cursorPosition.X = 172;
                    cursorPosition.Y = 50;
                    break;
            }
                
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(_selectorTexture, cursorPosition + new Vector2(0, MathF.Sin((float)gameTime.TotalGameTime.TotalSeconds * 5) * 5), null, Color.White, 0f, _selectorTexture.Bounds.Center.ToVector2(), 1f, SpriteEffects.None, 0f);
            spriteBatch.End();
        }
        
        
    }

    public override bool Exit()
    {
        return true; // Exit wurde implementiert
    }
}