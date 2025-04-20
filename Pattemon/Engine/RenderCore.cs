using System;
using System.ComponentModel;
using HxGLTF.Implementation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PatteLib;
using PatteLib.Graphics;

namespace Pattemon.Engine;

public struct ColorCombination(Color foreground, Color background)
{
    static ColorCombination() {
        ColorCombination.Default = new ColorCombination(ColorUtils.FromHex("383838"), ColorUtils.FromHex("d8d8d8"));
        ColorCombination.Dark = new ColorCombination(ColorUtils.FromHex("515159"), ColorUtils.FromHex("a6a6ae"));
        ColorCombination.LightGray = new ColorCombination(ColorUtils.FromHex("5a5a52"), ColorUtils.FromHex("adbdbd"));
        ColorCombination.Area = new ColorCombination(ColorUtils.FromHex("000000"), ColorUtils.FromHex("9696a6"));
    }

    public static ColorCombination Default { get; private set; }
    public static ColorCombination Dark { get; private set; }
    public static ColorCombination LightGray { get; private set; }
    public static ColorCombination Area { get; private set; }
    
    public Color Foreground = foreground;
    public Color Background = background;

    public static ColorCombination Invert(ColorCombination combination)
    {
        return new ColorCombination(combination.Background, combination.Foreground);
    }
}

public static class RenderCore
{
    public enum TransitionType
    {
        AlphaIn,
        AlphaOut,
        SlideIn,
        SlideOut
    }

    public const float ScreenScale = 1f;
    public static readonly Point OriginalScreenSize = new Point(256, 192);
    public static readonly Point PreferedScreenSize = (OriginalScreenSize.ToVector2() * ScreenScale).ToPoint();
    public static readonly Vector2 ScreenCenter = PreferedScreenSize.ToVector2() / 2;

    private static Texture2D _pixel;
    private static float _transitionProgress;
    private static bool _isTransitioning;
    private static int _transitionSpeed;
    private static TransitionType _transitionType;
    
    private static GraphicsDevice _graphicsDevice;
    private static SpriteBatch _spriteBatch;
    
    public static GraphicsDevice GraphicsDevice => _graphicsDevice;
    public static SpriteBatch SpriteBatch => _spriteBatch;
    
    private static RenderTarget2D _topScreen;
    private static RenderTarget2D _bottomScreen;
    
    public static RenderTarget2D TopScreen => _topScreen;
    public static RenderTarget2D BottomScreen => _bottomScreen;
    
    private static ImageFontRenderer _fontRenderer;
    private static ImageFont _fontOutline;
    private static ImageFont _fontBase;
    
    public static void Init(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = spriteBatch;
        
        _fontRenderer = new ImageFontRenderer(graphicsDevice, spriteBatch, null);
        _fontBase = ImageFont.LoadFromFile(graphicsDevice, "Content/Fonts/Font_0.json");
        _fontOutline = ImageFont.LoadFromFile(graphicsDevice, "Content/Fonts/Font_0_bg.json");
        
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
        
        _topScreen = new RenderTarget2D(
            _graphicsDevice,
            PreferedScreenSize.X, PreferedScreenSize.Y,
            false,
            _graphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents
        );
        
        _bottomScreen = new RenderTarget2D(
            _graphicsDevice,
            PreferedScreenSize.X, PreferedScreenSize.Y,
            false,
            _graphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents
        );
    }
    
    public static void WriteText(string text, Vector2 position)
    {
        WriteText(text, position, ColorCombination.Default);
    }
    
    public static void WriteText(string text, Vector2 position, ColorCombination combination)
    {
        DrawText(_fontOutline, text, position, combination.Background);
        DrawText(_fontBase, text, position, combination.Foreground);
    }
    
    public static void DrawText(ImageFont font, string text, Vector2 position)
    {
        DrawText(font, text, position, Color.White);
    }
    
    public static void DrawText(ImageFont font, string text, Vector2 position, Color tint)
    {
        _fontRenderer.SetFont(font);
        _fontRenderer.DrawText(text, position, tint);
    }
    
    public static void StartScreenTransition(int speed, TransitionType type)
    {
        _isTransitioning = true;
        _transitionProgress = 0f;
        _transitionSpeed = speed;
        _transitionType = type;
    }

    public static bool IsScreenTransitionDone()
    {
        return !_isTransitioning;
    }
    
    public static void UpdateTransition(GameTime gameTime)
    {
        if (!_isTransitioning) return;

        _transitionProgress += (float)gameTime.ElapsedGameTime.TotalMilliseconds / _transitionSpeed;

        if (_transitionProgress >= 1f)
        {
            _transitionProgress = 1f;
            _isTransitioning = false;
        }
    }

    public static void RenderTransition()
    {
        if (!_isTransitioning)
        {
            return;
        }
        
        RenderCore.SetTopScreen();
        _spriteBatch.Begin();
        switch (_transitionType)
        {
            case TransitionType.AlphaIn: // Alpha-In
                _spriteBatch.Draw(_pixel, new Rectangle(0, 0, PreferedScreenSize.X, PreferedScreenSize.Y), new Color(Color.Black, 1f - _transitionProgress));
                break;

            case TransitionType.AlphaOut: // Alpha-Out
                _spriteBatch.Draw(_pixel, new Rectangle(0, 0, PreferedScreenSize.X, PreferedScreenSize.Y), new Color(Color.Black, _transitionProgress));
                break;

            case TransitionType.SlideIn: // Slide-In from bottom
                _spriteBatch.Draw(_pixel, new Rectangle(0, (int)(PreferedScreenSize.Y * (1f - _transitionProgress)), PreferedScreenSize.X, PreferedScreenSize.Y), Color.Black);
                break;

            case TransitionType.SlideOut: // Slide-Out to bottom
                _spriteBatch.Draw(_pixel, new Rectangle(0, (int)(PreferedScreenSize.Y * _transitionProgress), PreferedScreenSize.X, PreferedScreenSize.Y), Color.Black);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        _spriteBatch.End();
        
        SetBottomScreen();
        //not at the same time but delayed so it looks like it is on woosh
        _spriteBatch.Begin();
        switch (_transitionType)
        {
            case TransitionType.AlphaIn: // Alpha-In
                _spriteBatch.Draw(_pixel, new Rectangle(0, 0, PreferedScreenSize.X, PreferedScreenSize.Y), new Color(Color.Black, 1f - _transitionProgress));
                break;

            case TransitionType.AlphaOut: // Alpha-Out
                _spriteBatch.Draw(_pixel, new Rectangle(0, 0, PreferedScreenSize.X, PreferedScreenSize.Y), new Color(Color.Black, _transitionProgress));
                break;

            case TransitionType.SlideIn: // Slide-In from bottom
                _spriteBatch.Draw(_pixel, new Rectangle(0, (int)(PreferedScreenSize.Y * (1f - _transitionProgress)), PreferedScreenSize.X, PreferedScreenSize.Y), Color.Black);
                break;

            case TransitionType.SlideOut: // Slide-Out to bottom
                _spriteBatch.Draw(_pixel, new Rectangle(0, (int)(PreferedScreenSize.Y * _transitionProgress), PreferedScreenSize.X, PreferedScreenSize.Y), Color.Black);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        _spriteBatch.End();
    }
    
    public static void SetTopScreen()
    {
        _graphicsDevice.SetRenderTarget(_topScreen);
    }
    
    public static void SetBottomScreen()
    {
        _graphicsDevice.SetRenderTarget(_bottomScreen);
    }
    
    public static void Reset()
    {
        _graphicsDevice.SetRenderTarget(null);
    }
    
    public static void DrawModel(GameTime gameTime, Effect effect, GameModel model, bool alpha = false, Vector3 offset = default)
    {
        foreach (var scene in model.Scenes)
        {
            foreach (var nodeIndex in scene.Nodes)
            {
                var node = model.Nodes[nodeIndex];
                DrawNode(gameTime, effect, model, node, alpha, offset);
            }
        }
    }
    
    public static void DrawNode(GameTime gameTime, Effect effect, GameModel model, GameNode node, bool alpha = false, Vector3 offset = default)
    {
            
        //skinnig here?
            
        if (node.HasMesh)
        {
            //DrawMesh(node, gameModel.Meshes[node.MeshIndex]);
            DrawMesh(gameTime, effect, model, node, node.Mesh, alpha, offset);
        }
            
        if (node.HasChildren)
        {
            foreach (var child in node.Children)
            {
                DrawNode(gameTime, effect, model, node.Model.Nodes[child], alpha, offset);
            }
        }
    }
    
    public static void DrawMesh(GameTime gameTime, Effect effect, GameModel model, GameNode node, GameMesh mesh, bool alpha = false, Vector3 offset = default)
    {
        var alphaMode = 0;

        var worldMatrix = Matrix.CreateScale(model.Scale) *
                          Matrix.CreateFromQuaternion(model.Rotation) *
                          Matrix.CreateTranslation(model.Translation) *
                          Matrix.CreateTranslation(offset);

        effect.Parameters["World"].SetValue(node.GlobalTransform * worldMatrix);
        effect.Parameters["View"].SetValue(Camera.ActiveCamera.ViewMatrix);
        effect.Parameters["Projection"].SetValue(Camera.ActiveCamera.ProjectionMatrix);

        if (model.IsPlaying)
        {
            if (model.Skins is { Length: > 0 })
            {
                var skin = model.Skins[0];
                if (skin.JointMatrices.Length > 180)
                {
                    effect.Parameters["SkinningEnabled"]?.SetValue(false);
                }
                else
                {
                    effect.Parameters["Bones"]?.SetValue(skin.JointMatrices);
                    effect.Parameters["NumberOfBones"]?.SetValue(skin.JointMatrices.Length);
                    effect.Parameters["SkinningEnabled"]?.SetValue(true);
                }
            }
            else
            {
                effect.Parameters["SkinningEnabled"]?.SetValue(false);
            }
        }
        else
        {
            effect.Parameters["SkinningEnabled"]?.SetValue(false);
        }

        if (alpha)
        {
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
        }
        
        foreach (var primitive in mesh.Primitives)
        {
            if (ShouldSkipPrimitive(primitive, effect, alpha, ref alphaMode))
            {
                continue;
            }

            SetPrimitiveMaterialParameters(gameTime, primitive, effect);

            foreach (var pass in effect.Techniques[Math.Max(alphaMode - 1, 0)].Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0,
                    primitive.VertexBuffer.VertexCount / 3);
            }
            
        }
    }
    
    public static bool ShouldSkipPrimitive(GameMeshPrimitives primitive, Effect effect, bool alpha, ref int alphaMode)
    {
        if (primitive.Material != null)
        {
            var material = primitive.Material;
            
            switch (material.AlphaMode)
            {
                case "OPAQUE":
                    if (alpha)
                    {
                        return true;
                    }
                    alphaMode = 0;
                    break;
                case "MASK":
                    if (alpha)
                    {
                        return true;
                    }
                    alphaMode = 1;
                    break;
                case "BLEND":
                    if (!alpha)
                    {
                        return true;
                    }
                    alphaMode = 2;
                    break;
            }

            effect.Parameters["AlphaMode"]?.SetValue(alphaMode);
        }
        return false;
    }
    
    public static void SetPrimitiveMaterialParameters(GameTime gameTime, GameMeshPrimitives primitive, Effect effect)
    {
        GraphicsDevice.SetVertexBuffer(primitive.VertexBuffer);

        effect.Parameters["TextureEnabled"]?.SetValue(false);
        effect.Parameters["NormalMapEnabled"]?.SetValue(false);
        effect.Parameters["OcclusionMapEnabled"]?.SetValue(false);
        effect.Parameters["EmissiveTextureEnabled"]?.SetValue(false);

        if (primitive.Material != null)
        {
            var material = primitive.Material;
                
            //if (_debugTexture)
            //{
            //    Console.WriteLine(material.Name);
            //}
                
            effect.Parameters["EmissiveColorFactor"]?.SetValue(material.EmissiveFactor.ToVector4());
            effect.Parameters["BaseColorFactor"]?.SetValue(material.BaseColorFactor.ToVector4());
            effect.Parameters["AdditionalColorFactor"]?.SetValue(Color.White.ToVector4());
            effect.Parameters["AlphaCutoff"]?.SetValue(material.AlphaCutoff);

            if (material.HasTexture)
            {
                effect.Parameters["TextureEnabled"]?.SetValue(true);
                effect.Parameters["TextureDimensions"]?.SetValue(material.BaseTexture.Texture.Bounds.Size.ToVector2());
                effect.Parameters["Texture"]?.SetValue(material.BaseTexture.Texture);
                effect.Parameters["ShouldAnimate"]?.SetValue(false);
               //SetTextureAnimation(gameTime, material, effect);
               //SetTextureEffects(gameTime, material, effect);

                GraphicsDevice.SamplerStates[0] = material.BaseTexture.Sampler.SamplerState;
            }
        }
    }
    
}