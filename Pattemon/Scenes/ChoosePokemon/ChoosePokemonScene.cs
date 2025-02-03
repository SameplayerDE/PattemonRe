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
using Utils = PatteLib.Utils;

namespace Pattemon.Scenes.ChoosePokemon;

public class ChoosePokemonScene : SceneA
{

    private const int CHOOSE_STARTER_MAIN_FADE_IN = 0;
    private const int CHOOSE_STARTER_MAIN_WAIT_FADE_IN = 1;
    private const int CHOOSE_STARTER_MAIN_LOOP = 2;
    private const int CHOOSE_STARTER_MAIN_FADE_OUT = 3;
    private const int CHOOSE_STARTER_MAIN_WAIT_FADE_OUT = 4;

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
    private ImageFont _fontbg;
    private ImageFont _font;
    
    public ChoosePokemonScene(Game game, object args = null, string contentDirectory = "Content") : base(game, args, contentDirectory)
    {
    }

    public override bool Init()
    {
        _worldShader = _content.Load<Effect>("Shaders/WorldShader");
        _buildingShader = _content.Load<Effect>("Shaders/BuildingShader");
        
        /*
        v0.x = ((-30 * 0xffff) / 360);  // Rotation X (-30 Grad)
        v0.y = ((0 * 0xffff) / 360);     // Rotation Y (0 Grad)
        v0.z = ((0 * 0xffff) / 360);     // Rotation Z (0 Grad)

        Camera_InitWithTarget(param1, (300 << FX32_SHIFT), &v0, ((22 * 0xffff) / 360), 0, 1, camera);
        
        ov78_021D2108(&param0->unk_00, ((-30 * 0xffff) / 360), ((-50 * 0xffff) / 360), 6);
        ov78_021D2108(&param0->unk_10, (300 << FX32_SHIFT), (200 << FX32_SHIFT), 6);
        ov78_021D2108(&param0->unk_20, 0, (36 * FX32_ONE), 6);
        */
       
       //_camera = new Camera();
       //_camera.InitWithTarget(
       //    new Vector3(0, 0, 0),       // Target
       //    300.0f,                     // Distance
       //    new Vector3(-30, 0, 0),      // Rotation (X = -30°)
       //    MathHelper.ToRadians(22),    // FoV
       //    CameraProjectionType.Perspective,
       //    true
       //);
        //_camera.InitWithPosition(new Vector3(0, 9, 14) * 16, 10, new Vector3(MathHelper.ToRadians(-30), 0, 0), MathHelper.ToRadians(26), CameraProjectionType.Perspective);
        ////_normalCamera.InitWithTarget(_target, 10, Vector3.Zero, MathHelper.ToRadians(75), CameraProjectionType.Perspective, true);
        //_camera.SetClipping(0.01f, 100000f);
        
        
        
        _selectorTexture = _content.Load<Texture2D>("Icons/Choose_Selector");
        
        var gltfFile = GLTFLoader.Load(@"A:\ModelExporter\Platin\export_output\output_assets\psel_all\psel_all");
        _openAnimation = GameModel.From(_graphics, gltfFile);
        _openAnimation.OnAnimationCompleted += () => _state = CHOOSE_STARTER_MAIN_LOOP;
        
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

        _font = ImageFont.LoadFromFile(_graphics, "Content/Fonts/Font_0.json");
        _fontbg = ImageFont.LoadFromFile(_graphics, "Content/Fonts/Font_0_bg.json");
        
        return true;
    }
    
    public override bool Exit()
    {
        return true; // Exit wurde implementiert
    }
    
    public override bool Update(GameTime gameTime, float delta)
    {
        _camera = Camera.CreateFromPret(76800, 60082, 0, 0, false, ((22 * 0xffff) / 360), 1, 100);
        _camera.SetAsActive();
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
            case CHOOSE_STARTER_MAIN_LOOP:
            {
                // check selection
                // update graphics
                // if selection is made state++
                //ExecuteLoop(gameTime, delta);
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
        }
        if (_state == CHOOSE_STARTER_MAIN_LOOP)
        {
            RenderCore.DrawModel(gameTime, _buildingShader, _case);
            RenderCore.DrawModel(gameTime, _buildingShader, _ballA, offset: new Vector3(-44, -4, 32));
            RenderCore.DrawModel(gameTime, _buildingShader, _ballB, offset: new Vector3(0, -4, 62));
            RenderCore.DrawModel(gameTime, _buildingShader, _ballC, offset: new Vector3(38, -4, 26));
            
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
            
            RenderCore.WriteText(_font, _fontbg, "Winziglaub CHELAST", cursorPosition, ColorUtils.FromHex("515159"), ColorUtils.FromHex("a6a6ae"));
            RenderCore.WriteText(_font, _fontbg, "Möchtest du dieses Pokemon", cursorPosition + new Vector2(0, _font.Meta.GlyphHeight), ColorUtils.FromHex("515159"), ColorUtils.FromHex("a6a6ae"));
            
            spriteBatch.End();
        }
    }
}