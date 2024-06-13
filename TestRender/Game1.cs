using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HxGLTF;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using System.Reflection;
using Survival.Rendering;
using Newtonsoft.Json.Linq;
using TestRender;

namespace TestRendering
{
    public class Game1 : Game
    {

        public static GameWindow GameWindow;


        private RenderTarget2D _dsScreen;
        
        private GraphicsDeviceManager _graphicsDeviceManager;
        private SpriteBatch _spriteBatch;

        private List<VertexPositionNormalColorTexture> _positions = new List<VertexPositionNormalColorTexture>();
        private VertexPositionNormalColorTexture[] _positionsArray;
        private Matrix world, view, proj;
        private Effect _effect;
        private VertexBuffer _vertexBuffer;
        private List<VertexBuffer> _vertexBuffers = new List<VertexBuffer>();

        private Camera _camera;
        
        private Texture2D _testTexture2D;
        private Dictionary<string, Texture2D> loaded = new Dictionary<string, Texture2D>();
        private List<bool> alpha = new List<bool>();
        private List<bool> doubleSided = new List<bool>();

        private BasicEffect _basicEffect;
        private GLTFFile gltfFile;
        private GameModel gameModel;

        public readonly static BlendState AlphaSubtractive = new BlendState
        {
            ColorSourceBlend = Blend.One,
            AlphaSourceBlend = Blend.One,			    
            ColorDestinationBlend = Blend.Zero,
            AlphaDestinationBlend = Blend.Zero
        };
        
        public Game1()
        {
            // ReSharper disable once HeapView.ObjectAllocation.Evident
            _graphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);

            _graphicsDeviceManager.PreferredBackBufferHeight = 720;
            _graphicsDeviceManager.PreferredBackBufferWidth = 1280;
            
            _graphicsDeviceManager.ApplyChanges();

            GameWindow = Window;
            
            
        }
        
        
        protected Texture2D LoadFormFile(string path)
        {
            var fileStream = new FileStream(path, FileMode.Open);
            var result = Texture2D.FromStream(GraphicsDevice, fileStream);
            fileStream.Dispose();
            return result;
        }

        private void LoadImages()
        {
            if (!gltfFile.HasImages)
            {
                return;
            }
            foreach (var image in gltfFile.Images)
            {
                if (image != null)
                {
                    byte[] imageData = null;
                    if (!string.IsNullOrEmpty(image.Uri))
                    {
                        if (Path.IsPathRooted(image.Uri))
                        {
                            loaded.Add(image.Uri, LoadFormFile(image.Uri));
                        }
                        else
                        {
                            var combinedPath = Path.Combine(Path.GetDirectoryName(gltfFile.Path) ?? string.Empty, image.Uri);
                            loaded.Add(image.Uri, LoadFormFile(combinedPath));
                        }
                    }
                    else if (image.BufferView != null)
                    {
                        var bufferView = image.BufferView;
                        var buffer = bufferView.Buffer; // Annahme: Implementierung von Buffer
                        imageData = new byte[bufferView.ByteLength];
                        Array.Copy(buffer.Bytes, bufferView.ByteOffset, imageData, 0, bufferView.ByteLength);
                    }
                    else
                    {
                        throw new Exception("Image URI und BufferView sind beide null.");
                    }

                    if (imageData != null && imageData.Length > 0)
                    {
                        loaded.Add(image.Name, Texture2D.FromStream(GraphicsDevice, new MemoryStream(imageData)));
                    }
                }
            }
        }

        protected override void Initialize()
        {
            _basicEffect = new BasicEffect(GraphicsDevice);
            // _dsScreen = new RenderTarget2D(GraphicsDevice, 256, 192, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            _dsScreen = new RenderTarget2D(GraphicsDevice, 1280, 720, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;
            
            _camera = new Camera(GraphicsDevice);

            //gltfFile = GLTFLoader.Load(@"Content\pkemon_oben");
            gltfFile = GLTFLoader.Load(@"C:\Users\asame\Documents\ModelExporter\black2\output_assets\m_dun3501_01_00\m_dun3501_01_00.glb");
            //gltfFile = GLTFLoader.Load(@"C:\Users\asame\Documents\ModelExporter\black2\output_assets\badgegate_02\badgegate_02.glb");
            //gltfFile = GLTFLoader.Load(@"Content\map01_22c\map01_22c.glb");
            gameModel = GameModel.From(GraphicsDevice, gltfFile);

            Console.WriteLine(gltfFile.Asset.Version);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // ReSharper disable once HeapView.ObjectAllocation.Evident
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _effect = Content.Load<Effect>("DiffuseShader");
            _testTexture2D = Content.Load<Texture2D>("1");
        }

        protected override void Update(GameTime gameTime)
        {
            if (!IsActive)
            {
                return;
            }
            
            Vector3 Direction = new Vector3();
            
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                Direction += Vector3.Forward;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                Direction += Vector3.Left;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                Direction += Vector3.Right;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                Direction += Vector3.Backward;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                Direction -= Vector3.Down;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.E))
            {
                Direction -= Vector3.Up;
            }
            
            _camera.Move(-Direction * (float)gameTime.ElapsedGameTime.TotalSeconds * 5f);
            
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
            
            _camera.Update(gameTime);
            
            world = Matrix.CreateTranslation(Vector3.Zero) * Matrix.CreateScale(0.05f);
            view = _camera.View;
            proj  = _camera.Projection;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.Black);


            int i = 0;
            foreach (var primitive in gameModel.Root.Mesh.Primitives)
            {
                GraphicsDevice.SetVertexBuffer(primitive.VertexBuffer);

                // Set BasicEffect parameters
                _basicEffect.World = world * Matrix.CreateScale(1f);
                _basicEffect.View = view;
                _basicEffect.Projection = proj;
                //_basicEffect.EnableDefaultLighting();

                if (primitive.Material == null)
                {
                    continue;
                }
                
                if (primitive.Material.BaseTexture == null)
                {
                    _basicEffect.TextureEnabled = false;
                    continue;
                }

                var sampler = primitive.Material.BaseTexture.Sampler;
                _basicEffect.Texture = primitive.Material.BaseTexture.Texture;
                _basicEffect.TextureEnabled = true;
                _basicEffect.VertexColorEnabled = false;

                _basicEffect.GraphicsDevice.SamplerStates[0] = new SamplerState
                {
                    AddressU = sampler.WrapS,
                    AddressV = sampler.WrapT,
                    Filter = TextureFilter.Point
                };

                string alphaMode = primitive.Material.AlphaMode; // Default to "OPAQUE" if not specified
                float alphaCutoff = primitive.Material.AlphaCutoff; // Default alpha cutoff value for MASK mode

                switch (alphaMode)
                {
                    case "OPAQUE":
                        GraphicsDevice.BlendState = BlendState.Opaque;
                        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                        break;

                    case "MASK":
                    case "BLEND":
                        var alphaTestEffect = new AlphaTestEffect(GraphicsDevice)
                        {
                            World = _basicEffect.World,
                            View = _basicEffect.View,
                            Projection = _basicEffect.Projection,
                            DiffuseColor = _basicEffect.DiffuseColor,
                            AlphaFunction = CompareFunction.Greater,
                            ReferenceAlpha = (int)(alphaCutoff * 255),
                            Texture = _basicEffect.Texture
                        };

                        GraphicsDevice.BlendState = BlendState.AlphaBlend;
                        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                        // Apply passes and draw primitives
                        foreach (var pass in alphaTestEffect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, primitive.VertexBuffer.VertexCount);
                        }
                        i++;
                        continue; // Skip the BasicEffect pass since we've used AlphaTestEffect

                }

                foreach (var pass in _basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, primitive.VertexBuffer.VertexCount);
                }
                i++;
            }
            base.Draw(gameTime);
        }
    }

}