using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using HxCameraEditor.Graphics;
using HxCameraEditor.UserInterface;
using HxCameraEditor.UserInterface.Models;
using HxGLTF;
using HxGLTF.Implementation;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using NativeFileDialogSharp;
using NativeFileDialogSharp.Native;
using Newtonsoft.Json.Linq;
using PatteLib.Graphics;
using PatteLib.World;
using Image = HxCameraEditor.UserInterface.Image;

namespace HxCameraEditor;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private UserInterfaceRenderer _interfaceRenderer;

    private RenderTarget2D _cameraViewPort;
    private Point _preferredDimensions = new Point(1280 / 2, 960 / 2);
    private Camera _camera;
    
    private Texture2D _overlay;
    private Texture2D _player;
    private Texture2D _kage;
    
    public Chunk CurrentChunk;
    public GameModel CurrentMesh => _model;
    public GameModel Building;
    
    private Dictionary<(string[] Materials, AnimationCompareFunction CompareFunction), TextureAnimation> _animations = [];
    private GameModel _model;
    private Effect _worldShader;
    private AlphaTestEffect _basicEffect;
    
    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;

    private UserInterfaceNode _node;
    
    private Vector3 _target = new Vector3(0.5f, 5.05f, -2.25f);

    private Binding<object> _rotation;
    private Binding<object> _distance;
    private Binding<object> _fieldOfView;
    private Binding<bool> _orthoMode;

    private MessageQueue _messageQueue;
    
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
    }

    protected override void Initialize()
    {
        _cameraViewPort = new RenderTarget2D(GraphicsDevice, _preferredDimensions.X, _preferredDimensions.Y, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24Stencil8);
        _camera = Camera.GetDefault();
        _camera.SetAsActive();

        _rotation = new Binding<object>(_camera.Rotation);
        _distance = new Binding<object>(_camera.Distance);
        _fieldOfView = new Binding<object>(_camera.FieldOfViewY);
        _orthoMode = new Binding<bool>(_camera.ProjectionType == CameraProjectionType.Orthographic);
        
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
        
        GenerateNode();
        
        base.Initialize();
    }

    private void GenerateNode()
    {
        _node = new VStack(
            new HStack(
                new Button(new Label("Reset Settings")).OnClick(() =>
                {
                    ResetSettings();
                }),
                new Button(new Label("Load Settings")).OnClick(() =>
                {
                    LoadSettingsFromFile();
                }),
                new Button(new Label("Save Settings")).OnClick(() =>
                {
                    SaveSettingsToFile();
                })
            ),
            new VStack(
                new VStack(
                    new HStack(
                        new Label("Camera Settings")
                    )
                ),
                new ScrollView(
                    new VStack(
                        new Label("Hallo"),
                        new Label("Hallo"),
                        new Label("Hallo"),
                        new Button(new Image("iconPlus")).OnClick(Exit),
                        new Button(new Image("iconMinus")),
                        new Label("Hallo"),
                        new Label("Hallo"),
                        new Label("Hallo"),
                        new Label("Hallo"),
                        new Label("Hallo"),
                        new Label("Hallo"),
                        new Label("Hallo"),
                        new Label("Hallo"),
                        new Label("Hallo"),
                        new Label("Hallo")
                    )
                ),
                new VStack(
                    new HStack(
                        new Label("Orthographic"),
                        new RadioButton().OnClick((isChecked) =>
                        {
                            _orthoMode.Value = isChecked;
                            _camera.SetProjectionType(isChecked ? CameraProjectionType.Orthographic : CameraProjectionType.Perspective);
                        }).SetIsCheckedBinding(_orthoMode)
                    ).SetAlignment(Alignment.Center),
                    new VStack(
                        new Label("Rotation"),
                        new Label(null, "defaultS").SetTextBinding(_rotation),
                        new Label("can also be changed with arrow keys", "defaultS"),
                        new HStack(
                            new HStack(
                                new VStack(
                                    new HStack(
                                        new Button(new Image("iconLeft")).OnClick(() =>
                                        {
                                            float amount = 1;
                                            if (KeyboardHandler.IsKeyDown(Keys.LeftShift))
                                            {
                                                amount *= 10;
                                            }
                                            else  if (KeyboardHandler.IsKeyDown(Keys.LeftControl))
                                            {
                                                amount /= 2;
                                            }
                                            _camera.AdjustRotationAroundTarget(new Vector3(0, -MathHelper.ToRadians(amount), 0));
                                        }).SetPadding(5),
                                        new VStack(
                                            new Button(new Image("iconUp")).OnClick(() =>
                                            {
                                                float amount = 1;
                                                if (KeyboardHandler.IsKeyDown(Keys.LeftShift))
                                                {
                                                    amount *= 10;
                                                }
                                                else  if (KeyboardHandler.IsKeyDown(Keys.LeftControl))
                                                {
                                                    amount /= 2;
                                                }
                                                _camera.AdjustRotationAroundTarget(new Vector3(-MathHelper.ToRadians(amount), 0, 0));
                                            }).SetPadding(5),
                                            new Button(new Image("iconDown")).OnClick(() =>
                                            {
                                                float amount = 1;
                                                if (KeyboardHandler.IsKeyDown(Keys.LeftShift))
                                                {
                                                    amount *= 10;
                                                }
                                                else  if (KeyboardHandler.IsKeyDown(Keys.LeftControl))
                                                {
                                                    amount /= 2;
                                                }
                                                _camera.AdjustRotationAroundTarget(new Vector3(MathHelper.ToRadians(amount), 0, 0));
                                            }).SetPadding(5)
                                        ).SetSpacing(5),
                                        new Button(new Image("iconRight")).OnClick(() =>
                                        {
                                            float amount = 1;
                                            if (KeyboardHandler.IsKeyDown(Keys.LeftShift))
                                            {
                                                amount *= 10;
                                            }
                                            else  if (KeyboardHandler.IsKeyDown(Keys.LeftControl))
                                            {
                                                amount /= 2;
                                            }
                                            _camera.AdjustRotationAroundTarget(new Vector3(0, MathHelper.ToRadians(amount), 0));
                                        }).SetPadding(5)
                                    ).SetAlignment(Alignment.Center).SetSpacing(5)
                                ).SetSpacing(10)
                            )
                        ).SetAlignment(Alignment.Center)
                    ).SetSpacing(5),
                    new VStack(
                        new Label("Distance"),
                        new Label(null, "defaultS").SetTextBinding(_distance),
                        new HStack(
                            new Button(new Image("iconPlus")).OnClick(() =>
                            {
                                if (_orthoMode.Value)
                                {
                                    _messageQueue.Enqueue("Action can not be performed in orthographic mode.");
                                    return;
                                }
                                float amount = 1;
                                if (KeyboardHandler.IsKeyDown(Keys.LeftShift))
                                {
                                    amount *= 10;
                                }
                                else  if (KeyboardHandler.IsKeyDown(Keys.LeftControl))
                                {
                                    amount /= 2;
                                }
                                _camera.AdjustDistance(amount);
                            }).SetPadding(5).SetIsDisabledBinding(_orthoMode),
                            new Button(new Image("iconMinus")).OnClick(() =>
                            {
                                if (_orthoMode.Value)
                                {
                                    _messageQueue.Enqueue("Action can not be performed in orthographic mode.");
                                    return;
                                }
                                float amount = 1;
                                if (KeyboardHandler.IsKeyDown(Keys.LeftShift))
                                {
                                    amount *= 10;
                                }
                                else  if (KeyboardHandler.IsKeyDown(Keys.LeftControl))
                                {
                                    amount /= 2;
                                }
                                _camera.AdjustDistance(-amount);
                            }).SetPadding(5).SetIsDisabledBinding(_orthoMode)
                        ).SetAlignment(Alignment.Center).SetSpacing(5)
                    ).SetSpacing(10),
                    new VStack(
                        new Label("Field Of View"),
                        new Label(null, "defaultS").SetTextBinding(_fieldOfView),
                        new HStack(
                            new Button(new Image("iconPlus")).OnClick(() =>
                            {
                                float amount = 1;
                                if (KeyboardHandler.IsKeyDown(Keys.LeftShift))
                                {
                                    amount *= 10;
                                }
                                else  if (KeyboardHandler.IsKeyDown(Keys.LeftControl))
                                {
                                    amount /= 2;
                                }
                                _camera.AdjustFieldOfView(MathHelper.ToRadians(amount));
                            }).SetPadding(5),
                            new Button(new Image("iconMinus")).OnClick(() =>
                            {
                                float amount = 1;
                                if (KeyboardHandler.IsKeyDown(Keys.LeftShift))
                                {
                                    amount *= 10;
                                }
                                else  if (KeyboardHandler.IsKeyDown(Keys.LeftControl))
                                {
                                    amount /= 2;
                                }
                                _camera.AdjustFieldOfView(-MathHelper.ToRadians(amount));
                            }).SetPadding(5)
                        ).SetAlignment(Alignment.Center).SetSpacing(5)
                    ).SetSpacing(5)
                ).SetPaddingLeft(32).SetSpacing(20),
                new VStack(
                    new VStack(
                        new HStack(
                            new Label("World Settings")
                        )
                    ),
                    new VStack(
                        new Label("Preview"),
                        new HStack(
                            new Button(new Image("iconLeft")).OnClick(() =>
                            {
                                _messageQueue.Enqueue("Action not yet implemented.");
                            }).SetPadding(5).SetIsDisabled(),
                            new Button(new Image("iconRight")).OnClick(() =>
                            {
                                _messageQueue.Enqueue("Action not yet implemented.");
                            }).SetPadding(5).SetIsDisabled()
                        ).SetAlignment(Alignment.Center).SetSpacing(5)
                    ).SetSpacing(10).SetPaddingLeft(32)
                ).SetSpacing(20).SetPaddingTop(32)
            ).SetSpacing(10)
        ).SetSpacing(10).SetPadding(10);
    }

    private void ResetSettings()
    {
        _camera = Camera.GetDefault();
        _camera.SetAsActive();
        _orthoMode.Value = _camera.ProjectionType == CameraProjectionType.Orthographic;
            
        _messageQueue.Enqueue("Loaded default camera settings.", MessageType.Warning);
    }
    
    private void SaveSettingsToFile()
    {
        var result = Dialog.FileSave("bin");

        if (result.IsOk && !string.IsNullOrEmpty(result.Path))
        {
            string filePath = result.Path.EndsWith(".bin", StringComparison.OrdinalIgnoreCase)
                ? result.Path
                : $"{result.Path}.bin";

            try
            {
                GameCameraFile cameraFile = CameraFactory.ToDSPRE(Camera.ActiveCamera);
                File.WriteAllBytes(filePath, cameraFile.ToByteArray());
                _messageQueue.Enqueue($"Saved camera to binary file:\n {filePath}", MessageType.Success);
            }
            catch (Exception ex)
            {
                _messageQueue.Enqueue($"Failed to save camera settings: {ex.Message}", MessageType.Error);
                Console.WriteLine($"Error saving file: {ex}");
            }
        }
        else if (result.IsError)
        {
            _messageQueue.Enqueue($"Error: {result.ErrorMessage}", MessageType.Error);
        }
        else
        {
            _messageQueue.Enqueue("Save operation was canceled.", MessageType.Warning);
        }
    }



    private void LoadSettingsFromFile()
    {
            var result = Dialog.FileOpen("bin");

        if (result.IsOk && !string.IsNullOrEmpty(result.Path))
        {
            try
            {
                var cameraData = File.ReadAllBytes(result.Path);
                GameCameraFile cameraFile = new GameCameraFile(cameraData);

                _camera = CameraFactory.CreateFromDSPRE(
                    (int)cameraFile.distance,
                    cameraFile.vertRot,
                    cameraFile.horiRot,
                    cameraFile.zRot,
                    cameraFile.perspMode == GameCameraFile.ORTHO,
                    cameraFile.fov,
                    (int)cameraFile.nearClip,
                    (int)cameraFile.farClip
                );

                _camera.SetAsActive();
                _orthoMode.Value = _camera.ProjectionType == CameraProjectionType.Orthographic;

                _messageQueue.Enqueue("Loaded camera settings from binary file.", MessageType.Success);
                Console.WriteLine("Loaded camera settings from binary file.");
            }
            catch (Exception ex)
            {
                _messageQueue.Enqueue($"Failed to load camera settings: {ex.Message}", MessageType.Error);
                Console.WriteLine($"Failed to load camera settings: {ex}");
            }
        }
        else if (result.IsError)
        {
            _messageQueue.Enqueue($"Error: {result.ErrorMessage}", MessageType.Error);
        }
        else
        {
            _messageQueue.Enqueue("Load operation was canceled.", MessageType.Warning);
        }
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        _interfaceRenderer = new UserInterfaceRenderer(GraphicsDevice, _spriteBatch, Services);
        
        _messageQueue = new MessageQueue
        {
            Font = _interfaceRenderer.Font,
            Pixel = _interfaceRenderer.Pixel,
            Position = new Vector2(16, GraphicsDevice.Viewport.Height - 16)
        };

        _worldShader = Content.Load<Effect>("Shaders/WorldShader");
        _basicEffect = new AlphaTestEffect(GraphicsDevice);
        _overlay = Texture2D.FromFile(GraphicsDevice,"Assets/overlay_no_shine.png");

        _player = Texture2D.FromFile(GraphicsDevice, "Assets/Sprites/player.png");
        _kage = Texture2D.FromFile(GraphicsDevice, "Assets/Sprites/kage.png");
        
        var chunkJson = File.ReadAllText($@"Assets/0042/ChunkData.json");
        var jChunk = JObject.Parse(chunkJson);
        var chunk = Chunk.Load(jChunk);
        CurrentChunk = chunk;
        
        _model = GameModel.From(GraphicsDevice, GLTFLoader.Load("Assets/0042/0042.glb"));
        Building = GameModel.From(GraphicsDevice, GLTFLoader.Load("Assets/0042/d4_s01.gltf"));
        _animations = TextureAnimationLinker.LoadAnimations(GraphicsDevice, "Assets/Animations/AnimationLink.json");
    }

    protected override void Update(GameTime gameTime)
    {
        if (!IsActive)
        {
            return;
        }
        
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        InputHandler.Update();
        
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        var amount = 10;
        var rad = MathHelper.ToRadians(amount) * delta;
        if (Keyboard.GetState().IsKeyDown(Keys.Left))
        {
            _camera.AdjustRotationAroundTarget(new Vector3(0, -rad, 0));
        }
        if (Keyboard.GetState().IsKeyDown(Keys.Right))
        {
            _camera.AdjustRotationAroundTarget(new Vector3(0, rad, 0));
        }
        if (Keyboard.GetState().IsKeyDown(Keys.Up))
        {
            _camera.AdjustRotationAroundTarget(new Vector3(-rad, 0, 0));
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Down))
        {
            _camera.AdjustRotationAroundTarget(new Vector3(rad, 0, 0));
        }

        const float speed = 1.0f;

        //_target += KeyboardHandler.GetDirection() * speed * delta;
        
        //_target.X = Math.Clamp(_target.X, 0, Chunk.Wx);
        //_target.Z = Math.Clamp(_target.Z, 0, Chunk.Wy);
        
        _camera.CaptureTarget(new Vector3(_target.X * 16, _target.Y * 16, _target.Z * 16));
        _camera.ComputeViewMatrix();

        foreach (var animation in _animations.Values)
        {
            animation.Update(gameTime);
        }

        _interfaceRenderer.CalculateLayout(_node);
        _interfaceRenderer.HandleInput(_node);
        _messageQueue.Update(gameTime);

        _rotation.Value = _camera.Rotation;        
        _distance.Value = _camera.Distance;        
        _fieldOfView.Value = _camera.FieldOfViewY;
        
        //var chunkPlates = CurrentChunk.GetChunkPlateUnderPosition(new Vector3(_target.X, _target.Y, _target.Z));
        //if (chunkPlates.Length > 0)
        //{
        //    _target.Y = chunkPlates[0].GetHeightAt(_target.X, _target.Z) * 16f;
        //}
        
        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        GraphicsDevice.SetRenderTarget(_cameraViewPort);
        GraphicsDevice.Clear(Color.Black);
        
        DrawModel(gameTime, _worldShader, CurrentMesh,
            offset: new Vector3(0 * Chunk.Wx, 0, 0 * Chunk.Wy), alpha: false);
        
        DrawModel(gameTime, _worldShader, CurrentMesh,
            offset: new Vector3(0 * Chunk.Wx, 0, 0 * Chunk.Wy), alpha: true);
                
        DrawModel(gameTime, _worldShader, Building,
            offset: new Vector3(CurrentChunk.Buildings[1].Position.X * 16, CurrentChunk.Buildings[1].Position.Y * 16, CurrentChunk.Buildings[1].Position.Z * 16), alpha: false);
        
        DrawModel(gameTime, _worldShader, Building,
            offset: new Vector3(CurrentChunk.Buildings[1].Position.X * 16, CurrentChunk.Buildings[1].Position.Y * 16, CurrentChunk.Buildings[1].Position.Z * 16), alpha: true);
        
        
        DrawSprite(gameTime, _basicEffect, 1f,  new Vector3(_target.X * 16, _target.Y * 16, _target.Z * 16), Vector3.One * 32, _camera.Rotation, _player);
        DrawSprite(gameTime, _basicEffect, 0.5f,  new Vector3(_target.X * 16, _target.Y * 16, (_target.Z - 0.625f) * 16), Vector3.One * 16,  new Vector3(MathHelper.PiOver2, 0, 0), _kage);
        
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.DarkSlateGray);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        
        _spriteBatch.Draw(_cameraViewPort, new Rectangle(GraphicsDevice.Viewport.Bounds.Center - (_preferredDimensions.ToVector2() / 2).ToPoint(), (_preferredDimensions.ToVector2() / 1).ToPoint()), Color.White);
        //_spriteBatch.Draw(_overlay, Vector2.Zero, Color.White);
        _messageQueue.Draw(_spriteBatch);
        _spriteBatch.End();
        
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _interfaceRenderer.DrawNode(_spriteBatch, gameTime, _node);
        _spriteBatch.End();
        

        base.Draw(gameTime);
    }
    
    private void DrawSprite(GameTime gameTime, AlphaTestEffect effect, float alpha, Vector3 position, Vector3 scale, Vector3 rotation, Texture2D texture)
    {
        
        var worldMatrix = Matrix.CreateScale(scale) *
                          Matrix.CreateRotationX(rotation.X) *
                          Matrix.CreateRotationY(rotation.Y) *
                          Matrix.CreateTranslation(position);

        effect.World = worldMatrix;
        effect.View = _camera.ViewMatrix;
        effect.Projection = _camera.ProjectionMatrix;
        effect.AlphaFunction = CompareFunction.Greater;
        effect.Alpha = alpha;
        effect.Texture = texture;
            
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
    
    /*private void DrawSprite(GameTime gameTime, Effect effect, Vector3 position, Vector3 scale, Vector3 rotation, Texture2D texture)
    {
        // Weltmatrix berechnen
        var worldMatrix = Matrix.CreateScale(scale) *
                          Matrix.CreateRotationX(rotation.X) *
                          Matrix.CreateRotationY(rotation.Y) *
                          Matrix.CreateRotationZ(rotation.Z) *
                          Matrix.CreateTranslation(position);

        // Shader-Parameter setzen
        effect.Parameters["World"].SetValue(worldMatrix);
        effect.Parameters["View"].SetValue(Camera.ActiveCamera.ViewMatrix);
        effect.Parameters["Projection"].SetValue(Camera.ActiveCamera.ProjectionMatrix);
        effect.Parameters["Texture"].SetValue(texture);

        // Render-Setup
        GraphicsDevice.SetVertexBuffer(_vertexBuffer);
        GraphicsDevice.Indices = _indexBuffer;
        GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

        // Rendern mit den Shader-Passes
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, 4);
        }
    }*/

    
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