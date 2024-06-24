using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using HxGLTF;
using HxGLTF.Implementation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TestRendering;

namespace TestRender
{
    public class Game1 : Game
    {
        private KeyboardState _prev;
        private KeyboardState _curr;
        
        private GraphicsDeviceManager _graphicsDeviceManager;
        private SpriteBatch _spriteBatch;
        
        private Effect _effect;

        private Camera _camera;

        private Hero _hero;
        private List<GameModel> gameModels = [];
        private SpriteFont _font;

        private RenderTarget2D _screen;
        
        string[,] _mapFileMatrix = new string[30, 30];
        int[,] _mapHeightMatrix = new int[30, 30];
        
        static string[,] ReadMatrixFromFile(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            int rows = lines.Length;
            int cols = lines[0].Split(',').Length;

            string[,] matrix = new string[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                string[] values = lines[i].Split(',');
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = string.IsNullOrEmpty(values[j]) ? "" : values[j];
                }
            }

            return matrix;
        }
        
        static int[,] ReadNumberMatrixFromFile(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            int rows = lines.Length;
            int cols = lines[0].Split(',').Length;

            int[,] matrix = new int[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                string[] values = lines[i].Split(',');
                for (int j = 0; j < cols; j++)
                {
                    if (!string.IsNullOrEmpty(values[j]))
                    {
                        if (int.TryParse(values[j], out int number))
                        {
                            matrix[i, j] = number;
                        }
                        else
                        {
                            // Handle invalid number format here if needed
                            matrix[i, j] = 0; // Default value if conversion fails
                        }
                    }
                    else
                    {
                        // Handle empty string or null case
                        matrix[i, j] = 0; // Default value if string is empty or null
                    }
                }
            }

            return matrix;
        }

        
        public Game1()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);

            _graphicsDeviceManager.PreferredBackBufferHeight = 720;
            _graphicsDeviceManager.PreferredBackBufferWidth = 1280;
            
            _graphicsDeviceManager.ApplyChanges();
        }

        protected override void Initialize()
        {
            _screen = new RenderTarget2D(GraphicsDevice, 1280 / 1, 720 / 1, false,
                GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            _camera = new Camera(GraphicsDevice);

            _mapFileMatrix = ReadMatrixFromFile(@"Content\MapFileMatrix.csv");
            _mapHeightMatrix = ReadNumberMatrixFromFile(@"Content\MapHeightMatrix.csv");
            
            _hero = new Hero();
            
            
            int maxChunksX = 128;
            int maxChunksY = 128;

            string basePath = @"A:\ModelExporter\Platin\overworldmaps\";
            //basePath = @"A:\ModelExporter\black2\output_assets\";
            //basePath = @"A:\ModelExporter\heartgold\output_assets\";

            for (int x = 1; x <= maxChunksX; x++)
            {
                for (int y = 1; y <= maxChunksY; y++)
                {
                    if (y >= _mapFileMatrix.GetLength(0) || x >= _mapFileMatrix.GetLength(1))
                    {
                        continue;
                    }
                    string folderName = _mapFileMatrix[y, x];
                    string fileName = $"{folderName}.glb";
                    string filePath = Path.Combine(basePath, folderName, fileName);

                    bool result = File.Exists(filePath);
    
                    if (result)
                    {
                        var chunkHeight = _mapHeightMatrix[y, x];
                        var gltfFile = GLTFLoader.Load(filePath);
                        var gameModel = GameModel.From(GraphicsDevice, gltfFile);
                        gameModels.Add(gameModel);
                        gameModel.Translation = new Vector3(x * 512, chunkHeight * 16, y * 512);
                    }
                }
            }
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _hero.LoadContent(GraphicsDevice, Content);
            // ReSharper disable once HeapView.ObjectAllocation.Evident
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _effect = Content.Load<Effect>("PBRShader");
            _effect.Parameters["Bones"].SetValue(new Matrix[64]); 
            _font = Content.Load<SpriteFont>("Font");
            
            _camera.Teleport(new Vector3(3, 0, 27) * 512);
        }

        protected override void Update(GameTime gameTime)
        {

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            if (!IsActive)
            {
                return;
            }

            _prev = _curr;
            _curr = Keyboard.GetState();

            
            var direction = new Vector3();

            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                direction += Vector3.Forward;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                direction += Vector3.Left;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                direction += Vector3.Right;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                direction += Vector3.Backward;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                direction -= Vector3.Down;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.E))
            {
                direction -= Vector3.Up;
            }
            
            if (Keyboard.GetState().IsKeyDown(Keys.O))
            {
                _camera.EnableMix = true;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                _camera.EnableMix = false;
            }

            if (Keyboard.GetState().IsKeyUp(Keys.LeftShift))
            {
                
                direction *= 1000;
                _camera.Move(-direction * delta);

                if (Keyboard.GetState().IsKeyDown(Keys.Up))
                {
                    _camera.RotateX(-1);
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                {
                    _camera.RotateX(1);
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Left))
                {
                    _camera.RotateY(1);
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Right))
                {
                    _camera.RotateY(-1);
                }
            }
           //else
           //{
           //    _camera.RotateTo(new Vector3(MathHelper.ToRadians(45), MathHelper.ToRadians(180), 0));
           //    _camera.Teleport(_hero.Position + (Vector3.Backward * 16 * 10) + new Vector3(0, 16 * 10, 0));
           //}
            
            
            _hero.Update(gameTime);
            
            _camera.Update(gameTime);
            

            //Console.WriteLine(_camera.Position);

            foreach (var model in gameModels)
            {
                model.Update(gameTime);
            }
            
            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_screen);
            GraphicsDevice.Clear(Color.Black);
            
            DrawModel(_hero._model);
            foreach (var model in gameModels)
            {
                DrawModel(model);
            }
            GraphicsDevice.SetRenderTarget(null);
            
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_screen, GraphicsDevice.Viewport.Bounds, Color.White);
            _spriteBatch.DrawString(_font, _camera.Rotation.ToString(), Vector2.Zero, Color.White);
            _spriteBatch.DrawString(_font, _camera.Rotation.ToString(), new Vector2(0, _font.LineSpacing), Color.Black);
            _spriteBatch.End();
            
            base.Draw(gameTime);
        }

        private void DrawModel(GameModel model)
        {
            foreach (var scene in model.Scenes)
            {
                foreach (var nodeIndex in scene.Nodes)
                {
                    var node = model.Nodes[nodeIndex];
                    DrawNode(model, node);
                }
            }
        }

        private void DrawNode(GameModel model, GameNode node)
        {
            
            //skinnig here?
            
            if (node.HasMesh)
            {
                //DrawMesh(node, gameModel.Meshes[node.MeshIndex]);
                DrawMesh(model, node, node.Mesh);
            }
            
            if (node.HasChildren)
            {
                foreach (var child in node.Children)
                {
                    DrawNode(model, node.Model.Nodes[child]);
                }
            }
        }

        private void DrawMesh(GameModel model, GameNode node, GameMesh mesh)
        {
            var alphaMode = 0;
            
            var worldMatrix = Matrix.CreateScale(model.Scale) *
                              Matrix.CreateFromQuaternion(model.Rotation) *
                              Matrix.CreateTranslation(model.Translation);
            
            _effect.Parameters["World"].SetValue(node.GlobalTransform * worldMatrix);
            _effect.Parameters["View"].SetValue(_camera.View);
            _effect.Parameters["Projection"].SetValue(_camera.Projection);

            if (model.IsPlaying)
            {
                if (model.Skins is { Length: > 0 })
                {
                    var skin = model.Skins[0];
                    if (skin.JointMatrices.Length > 128)
                    {
                        _effect.Parameters["SkinningEnabled"]?.SetValue(false);
                        // throw new Exception();
                    }
                    else
                    {
                        _effect.Parameters["Bones"]?.SetValue(skin.JointMatrices);
                        _effect.Parameters["NumberOfBones"]?.SetValue(skin.JointMatrices.Length);
                        _effect.Parameters["SkinningEnabled"]?.SetValue(true);
                    }
                }
                else
                {
                    _effect.Parameters["SkinningEnabled"]?.SetValue(false);
                }
            }
            else
            {
                _effect.Parameters["SkinningEnabled"]?.SetValue(false);
            }

            foreach (var primitive in mesh.Primitives)
            {
                GraphicsDevice.SetVertexBuffer(primitive.VertexBuffer);
                
                _effect.Parameters["TextureEnabled"]?.SetValue(false);
                _effect.Parameters["NormalMapEnabled"]?.SetValue(false);
                _effect.Parameters["OcclusionMapEnabled"]?.SetValue(false);
                _effect.Parameters["EmissiveTextureEnabled"]?.SetValue(false);
                
                if (primitive.Material != null)
                {
                    var material = primitive.Material;
                    
                    _effect.Parameters["EmissiveColorFactor"].SetValue(material.EmissiveFactor.ToVector4());
                    _effect.Parameters["BaseColorFactor"].SetValue(material.BaseColorFactor.ToVector4());
                    _effect.Parameters["AlphaCutoff"].SetValue(material.AlphaCutoff);

                    switch (material.AlphaMode)
                    {
                        case "OPAQUE":
                            alphaMode = 0;
                            break;
                        case "MASK":
                            alphaMode = 1;
                            break;
                        case "BLEND":
                            continue;
                            break;
                    }
                    
                    _effect.Parameters["AlphaMode"].SetValue(alphaMode);
                    
                    if (material.HasTexture)
                    {
                        _effect.Parameters["TextureEnabled"].SetValue(true);
                        _effect.Parameters["Texture"].SetValue(primitive.Material.BaseTexture.Texture);
                        
                        GraphicsDevice.SamplerStates[0] = primitive.Material.BaseTexture.Sampler.SamplerState;
                    }
                    
                    //if (material.HasNormalMap)
                    //{
                    //    //_effect.Parameters["NormalMapEnabled"]?.SetValue(true);
                    //    //_effect.Parameters["NormalMap"]?.SetValue(primitive.Material.NormalMap.Texture);
                    //    
                    //    //GraphicsDevice.SamplerStates[1] = primitive.Material.NormalMap.Sampler.SamplerState;
                    //}
                    
                    if (material.HasEmissiveTexture)
                    {
                        _effect.Parameters["EmissiveTextureEnabled"].SetValue(true);
                        _effect.Parameters["EmissiveTexture"].SetValue(primitive.Material.EmissiveTexture.Texture);
                        
                        GraphicsDevice.SamplerStates[3] = primitive.Material.EmissiveTexture.Sampler.SamplerState;
                    }
                }
                
                foreach (var pass in _effect.Techniques[Math.Max(alphaMode - 1, 0)].Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, primitive.VertexBuffer.VertexCount / 3);
                }
            }
            
            foreach (var primitive in mesh.Primitives)
            {
                GraphicsDevice.SetVertexBuffer(primitive.VertexBuffer);

                _effect.Parameters["TextureEnabled"]?.SetValue(false);
                _effect.Parameters["NormalMapEnabled"]?.SetValue(false);
                _effect.Parameters["OcclusionMapEnabled"]?.SetValue(false);
                _effect.Parameters["EmissiveTextureEnabled"]?.SetValue(false);

                if (primitive.Material != null)
                {
                    var material = primitive.Material;

                    //_effect.Parameters["EmissiveColorFactor"].SetValue(material.EmissiveFactor.ToVector4());
                    _effect.Parameters["BaseColorFactor"].SetValue(material.BaseColorFactor.ToVector4());
                    _effect.Parameters["AlphaCutoff"].SetValue(material.AlphaCutoff);

                    switch (material.AlphaMode)
                    {
                        case "OPAQUE":
                            continue;
                        case "MASK":
                            continue;
                        case "BLEND":
                            alphaMode = 2;
                            break;
                    }

                    _effect.Parameters["AlphaMode"].SetValue(alphaMode);

                    if (material.HasTexture)
                    {
                        _effect.Parameters["TextureEnabled"].SetValue(true);
                        _effect.Parameters["Texture"].SetValue(primitive.Material.BaseTexture.Texture);

                        GraphicsDevice.SamplerStates[0] = primitive.Material.BaseTexture.Sampler.SamplerState;
                    }

                    //if (material.HasEmissiveTexture)
                    //{
                    //    _effect.Parameters["EmissiveTextureEnabled"].SetValue(true);
                    //    _effect.Parameters["EmissiveTexture"].SetValue(primitive.Material.EmissiveTexture.Texture);
                    //
                    //    GraphicsDevice.SamplerStates[3] = primitive.Material.EmissiveTexture.Sampler.SamplerState;
                    //}
                }

                foreach (var pass in _effect.Techniques[Math.Max(alphaMode - 1, 0)].Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, primitive.VertexBuffer.VertexCount / 3);
                }
            }
            
        }
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public struct Constants
    {
        public Matrix World;
        public Matrix View;
        public Matrix Projection;

        public float AlphaCutoff;
        public int AlphaMode;

        public bool TextureEnabled;
        public bool NormalMapEnabled;
        public bool OcclusionMapEnabled;
        public bool EmissiveTextureEnabled;

        public Vector4 BaseColorFactor;
        public Vector4 EmissiveColorFactor;

        public bool SkinningEnabled;
        public Matrix[] Bones;
    }
    
}