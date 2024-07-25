using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using HxCameraEditor.Graphics;
using HxGLTF;
using HxGLTF.Implementation;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using PatteLib.Graphics;

namespace HxCameraEditor;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private RenderTarget2D _cameraViewPort;
    private Point _preferredDimensions = new Point(1280 / 2, 960 / 2);
    private Camera _camera;
    
    private Texture2D _overlay;
    private Texture2D _player;
    private Texture2D _shadow;
    
    private Dictionary<(string[] Materials, AnimationCompareFunction CompareFunction), TextureAnimation> _animations = [];
    private GameModel _model;
    private Effect _worldShader;
    private AlphaTestEffect _basicEffect;
    
    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;

    private Vector3 _target = new Vector3(0, 80, 0);

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
            
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1d / 120d);

        _graphics.PreferredBackBufferHeight = 960;
        _graphics.PreferredBackBufferWidth = 1280;
            
        _graphics.ApplyChanges();

        MediaPlayer.Volume = 0.3f;
    }

    protected override void Initialize()
    {
        _cameraViewPort = new RenderTarget2D(GraphicsDevice, _preferredDimensions.X, _preferredDimensions.Y, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24Stencil8);
        _camera = Camera.CameraLookMap[0];
        _camera.SetAsActive();
        
        var vertices = new VertexPositionTexture[4];
        
        vertices[0].Position = new Vector3(-0.5f, 0f, 0f);
        vertices[1].Position = new Vector3( 0.5f, 0f, 0f);
        vertices[2].Position = new Vector3( 0.5f, 1f, 0f);
        vertices[3].Position = new Vector3(-0.5f, 1f, 0f);

        vertices[0].TextureCoordinate = new Vector2(0, 1);
        vertices[1].TextureCoordinate = new Vector2(1, 1);
        vertices[2].TextureCoordinate = new Vector2(1, 0);
        vertices[3].TextureCoordinate = new Vector2(0, 0);
        
        var indices = new short[] { 0, 1, 2, 0, 3, 2 };

        _vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), vertices.Length, BufferUsage.WriteOnly);
        _vertexBuffer.SetData(vertices);
        
        _indexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), indices.Length, BufferUsage.WriteOnly);
        _indexBuffer.SetData(indices);
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _worldShader = Content.Load<Effect>("Shaders/WorldShader");
        _basicEffect = new AlphaTestEffect(GraphicsDevice);
        _overlay = Content.Load<Texture2D>("overlay_no_shine");

        _player = Texture2D.FromFile(GraphicsDevice, "Assets/Sprites/player.png");
        
        _model = GameModel.From(GraphicsDevice, GLTFLoader.Load("Assets/0042/0042.glb"));
        _animations = TextureAnimationLinker.LoadAnimations(GraphicsDevice, "Assets/Animations/AnimationLink.json");
    }

    protected override void Update(GameTime gameTime)
    {
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        KeyboardHandler.Update(gameTime);
        
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (KeyboardHandler.IsKeyDownOnce(Keys.R))
        {
            if (!File.Exists("Assets/camera.bin"))
            {
                Console.WriteLine("No Binary File Found");
                Console.WriteLine("Place It In [ Assets ] And Rename It To [ camera.bin ]");
                return;
            }

            GameCameraFile cameraFile = new GameCameraFile(File.ReadAllBytes("Assets/camera.bin"));

            _camera = CameraFactory.CreateFromDSPRE((int)cameraFile.distance, cameraFile.vertRot, cameraFile.horiRot, cameraFile.zRot, cameraFile.perspMode == GameCameraFile.ORTHO, cameraFile.fov, (int)cameraFile.nearClip, (int)cameraFile.farClip);
            _camera.SetAsActive();
            
            Console.WriteLine("Loaded Camera From Binary File");
        }

        const float speed = 64.0f;

        _target += KeyboardHandler.GetDirection() * speed * delta;

        _target.X = Math.Clamp(_target.X, -256, 256);
        _target.Z = Math.Clamp(_target.Z, -256, 256);
        
        _camera.CaptureTarget(ref _target);
        _camera.ComputeViewMatrix();

        foreach (var animation in _animations.Values)
        {
            animation.Update(gameTime);
        }

        
        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        GraphicsDevice.SetRenderTarget(_cameraViewPort);
        GraphicsDevice.Clear(Color.Black);
        DrawModel(gameTime,  _worldShader, _model, alpha: false);
        DrawModel(gameTime,  _worldShader, _model, alpha: true);
        
        //DrawSprite(gameTime, _basicEffect,  _target + new Vector3(0.5f, 0, 0.5f) * 8 - new Vector3(0, 0, 8), new Vector3(1, 1, 1) * 16, new Vector3(MathHelper.ToRadians(90), 0, 0), _shadow);
        DrawSprite(gameTime, _basicEffect,  _target, new Vector3(1, 1, 1) * 32, _camera.Rotation, _player);
        
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_cameraViewPort, new Rectangle(GraphicsDevice.Viewport.Bounds.Center - (_preferredDimensions.ToVector2() / 2).ToPoint(), (_preferredDimensions.ToVector2() / 1).ToPoint()), Color.White);
        _spriteBatch.Draw(_overlay, Vector2.Zero, Color.White);
        _spriteBatch.End();
        

        base.Draw(gameTime);
    }
    
    private void DrawSprite(GameTime gameTime, AlphaTestEffect effect, Vector3 position, Vector3 scale, Vector3 rotation, Texture2D texture)
    {
        
        var worldMatrix = Matrix.CreateScale(scale) *
                          Matrix.CreateRotationX(rotation.X) *
                          Matrix.CreateRotationY(rotation.Y) *
                          Matrix.CreateTranslation(position);

        effect.World = worldMatrix;
        effect.View = _camera.ViewMatrix;
        effect.Projection = _camera.ProjectionMatrix;
        effect.AlphaFunction = CompareFunction.Greater;
        effect.ReferenceAlpha = 0;
        effect.Texture = texture;
        effect.
            
        //effect.Parameters["World"].SetValue(worldMatrix);
        //effect.Parameters["View"].SetValue(_camera.View);
        //effect.Parameters["Projection"].SetValue(_camera.Projection);

        GraphicsDevice.SetVertexBuffer(_vertexBuffer);
        GraphicsDevice.Indices = _indexBuffer;
        GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, 4);
        }
    }
    
    private void DrawModel(GameTime gameTime, Effect effect, GameModel model, bool alpha = false, Vector3 offset = default)
    {
        if (Camera.ActiveCamera == null)
        {
            return;
        }
        
        foreach (var scene in model.Scenes)
        {
            foreach (var nodeIndex in scene.Nodes)
            {
                var node = model.Nodes[nodeIndex];
                DrawNode(gameTime, effect, model, node, alpha, offset);
            }
        }
    }

    private void DrawNode(GameTime gameTime, Effect effect, GameModel model, GameNode node, bool alpha = false, Vector3 offset = default)
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

    private void DrawMesh(GameTime gameTime, Effect effect, GameModel model, GameNode node, GameMesh mesh, bool alpha = false, Vector3 offset = default)
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

    private bool ShouldSkipPrimitive(GameMeshPrimitives primitive, Effect effect, bool alpha, ref int alphaMode)
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

    private void SetPrimitiveMaterialParameters(GameTime gameTime, GameMeshPrimitives primitive, Effect effect)
    {
        GraphicsDevice.SetVertexBuffer(primitive.VertexBuffer);

        effect.Parameters["TextureEnabled"]?.SetValue(false);
        effect.Parameters["NormalMapEnabled"]?.SetValue(false);
        effect.Parameters["OcclusionMapEnabled"]?.SetValue(false);
        effect.Parameters["EmissiveTextureEnabled"]?.SetValue(false);

        if (primitive.Material != null)
        {
            var material = primitive.Material;
                
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
                SetTextureAnimation(gameTime, material, effect);

                GraphicsDevice.SamplerStates[0] = material.BaseTexture.Sampler.SamplerState;
            }
        }
    }

    private void SetTextureAnimation(GameTime gameTime, GameMaterial material, Effect effect)
    {
        foreach (var animationPair in _animations)
        {
            AnimationCompareFunction compareFunction = animationPair.Key.CompareFunction;
            foreach (var keyMaterial in animationPair.Key.Materials)
            {
                TextureAnimation animation = null;
                if (compareFunction == AnimationCompareFunction.StartsWith)
                {
                    if (material.Name.StartsWith(keyMaterial))
                    {
                        animation = animationPair.Value;
                    }
                }
                else if (compareFunction == AnimationCompareFunction.Contains)
                {
                    if (material.Name.Contains(keyMaterial))
                    {
                        animation = animationPair.Value;
                    }
                }
                else if (compareFunction == AnimationCompareFunction.Equals)
                {
                    if (material.Name.Equals(keyMaterial))
                    {
                        animation = animationPair.Value;
                    }
                }

                if (animation == null)
                {
                    continue;
                }
                
                if (animation.Type == AnimationType.Texture)
                {
                    effect.Parameters["Texture"]?.SetValue(animation.Frames[animation.CurrentIndex]);
                    return;
                }

                effect.Parameters["ShouldAnimate"]?.SetValue(true);
                effect.Parameters["Offset"]?.SetValue(animation.Offset);
                
            }
        }
    }
    
}