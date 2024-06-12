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

            //var gltfFile = GLTFLoader.Load(@"X:\G Stuff");
            //var gltfFile = GLTFLoader.Load(@"C:\Users\asame\Documents\ModelExporter\black2\output_assets\map06_20\map06_20.glb");
            //gltfFile = GLTFLoader.Load(@"C:\Users\asame\Documents\ModelExporter\black2\output_assets\map13_03\map13_03.glb");
            //gltfFile = GLTFLoader.Load(@"C:\Users\asame\Documents\ModelExporter\Platin\output_assets\map05_21c\map05_21c.gltf");
            //var gltfFile = GLTFLoader.Load(@"C:\Users\asame\Documents\ModelExporter\Platin\output_assets\m_comp03_00_00c\m_comp03_00_00c.gltf");
            //gltfFile = GLTFLoader.Load(@"C:\Users\asame\Documents\ModelExporter\Platin\output_assets\ball\ball.gltf");
            //gltfFile = GLTFLoader.Load(@"G:\Opera GX - Downloads\original_anime_girls\scene.gltf");
            //var gltfFile = GLTFLoader.Load(@"C:\Users\asame\Documents\ModelExporter\black2\output_assets\ak_00\ak_00.glb");
            //var gltfFile = GLTFLoader.Load(@"C:\Users\asame\Documents\ModelExporter\Platin\output_assets\psel_all\psel_all.gltf");
            gltfFile = GLTFLoader.Load(@"Content\pkemon_oben");
            gltfFile = GLTFLoader.Load(@"C:\Users\asame\Documents\ModelExporter\black2\output_assets\m_dun3501_01_00\m_dun3501_01_00.glb");
            //gltfFile = GLTFLoader.Load(@"A:\FireFox Download\fortnite-cuddle_team_leader\scene.gltf");
            
            Console.WriteLine(gltfFile.Asset.Version);

            //foreach (var image in gltfFile.Images)
            //{
            //    if (image != null)
            //    {
            //        if (Path.IsPathRooted(image.Uri))
            //        {
            //            loaded.Add(image.Uri, LoadFormFile(image.Uri));
            //        }
            //        else
            //        {
            //            var combinedPath = Path.Combine(Path.GetDirectoryName(gltfFile.Path) ?? string.Empty, image.Uri);
            //            loaded.Add(image.Uri, LoadFormFile(combinedPath));
            //        }
            //    }
            //}

            LoadImages();
            
            foreach (var mesh in gltfFile.Meshes)
            {
                foreach (var primitive in mesh.Primitives)
                {

                    alpha.Add(false);
                    doubleSided.Add(false);

                    _positions.Clear();
                    VertexBuffer vertexBuffer;
                    int primitiveCount = 0;
                    var indicesAccessor = primitive.Indices;
                    float[] indices = null;
                    if (indicesAccessor != null)
                    {
                        primitiveCount = indicesAccessor.Count;
                        indices = AccessorReader.ReadData(indicesAccessor);
                    }

                    var position = new List<Vector3>();
                    var normals = new List<Vector3>();
                    var uvs = new List<Vector2>();
                    foreach (var attribute in primitive.Attributes)
                    {

                        Console.WriteLine(attribute.Key);
                        Console.WriteLine(attribute.Value.Type.Id);

                        var dataAccessor = attribute.Value;
                        var elementCount = dataAccessor.Count;
                        var numberOfComponents = dataAccessor.Type.NumberOfComponents;

                        float[] data;

                        //if (primitive.HasIndices)
                        //{
                        //    data = AccessorReader.ReadDataIndexed(dataAccessor, indicesAccessor);
                        //}
                        //else
                        //{
                        //    data = AccessorReader.ReadData(dataAccessor);
                        //}
                        //for (var i = 0; i < dataAccessor.Count; i++)
                        //{
                        //    if (attribute.Key == "POSITION" && dataAccessor.Type.Id == "VEC3")
                        //    {
                        //        position.Add(new Vector3(
                        //            data[i + 0],
                        //            data[i + 1],
                        //            data[i + 2]
                        //        ));
                        //    }
                        //    else if (attribute.Key == "NORMAL" && dataAccessor.Type.Id == "VEC3")
                        //    {
                        //        normals.Add(new Vector3(
                        //            data[i + 0],
                        //            data[i + 1],
                        //            data[i + 2]
                        //        ));
                        //    }
                        //    else if (attribute.Key == "TEXCOORD_0" && dataAccessor.Type.Id == "VEC2")
                        //    {
                        //        uvs.Add(new Vector2(
                        //            data[i + 0],
                        //            data[i + 1]
                        //        ));
                        //    }
                        //}

                        if (indices != null)
                        {
                            data = AccessorReader.ReadDataIndexed(dataAccessor, indicesAccessor);

                            //var data = AccessorReader.ReadData(dataAccessor);
                            //var indices = AccessorReader.ReadData(indicesAccessor);

                            if (attribute.Key == "POSITION" && dataAccessor.Type.Id == "VEC3")
                            {
                                for (var x = 0; x < data.Length; x += 3)
                                {
                                    position.Add(new Vector3(data[x], data[x + 1], data[x + 2]));
                                }
                            }

                            if (attribute.Key == "NORMAL" && dataAccessor.Type.Id == "VEC3")
                            {
                                for (var x = 0; x < data.Length; x += 3)
                                {
                                    normals.Add(new Vector3(data[x], data[x + 1], data[x + 2]));
                                }
                            }

                            if (attribute.Key == "TEXCOORD_0" && dataAccessor.Type.Id == "VEC2")
                            {
                                for (var x = 0; x < data.Length; x += 2)
                                {
                                    uvs.Add(new Vector2(data[x], data[x + 1]));
                                }
                            }
                        }
                        else
                        {
                            data = AccessorReader.ReadData(dataAccessor);
                            for (var i = 0; i < dataAccessor.Count; i++)
                            {
                                if (attribute.Key == "POSITION" && dataAccessor.Type.Id == "VEC3")
                                {
                                    position.Add(new Vector3(
                                        data[i * numberOfComponents + 0],
                                        data[i * numberOfComponents + 1],
                                        data[i * numberOfComponents + 2]
                                    ));
                                }
                                else if (attribute.Key == "NORMAL" && dataAccessor.Type.Id == "VEC3")
                                {
                                    normals.Add(new Vector3(
                                        data[i * numberOfComponents + 0],
                                        data[i * numberOfComponents + 1],
                                        data[i * numberOfComponents + 2]
                                    ));
                                }
                                else if (attribute.Key == "TEXCOORD_0" && dataAccessor.Type.Id == "VEC2")
                                {
                                    uvs.Add(new Vector2(
                                        data[i * numberOfComponents + 0],
                                        data[i * numberOfComponents + 1]
                                    ));
                                }
                            }
                        }

                    }

                    // Hier fügen wir die gesammelten Positionen, Normalen und Texturkoordinaten in die _positions-Liste ein
                    for (int i = 0; i < position.Count; i++)
                    {
                        _positions.Add(new VertexPositionNormalColorTexture(
                            position[i],
                            Color.White,
                            normals.Count > i ? normals[i] : Vector3.Up,
                            uvs.Count > i ? uvs[i] : Vector2.Zero
                        ));
                    }

                    // Setzen des VertexBuffers
                    _positionsArray = _positions.ToArray();
                    vertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionNormalColorTexture.VertexDeclaration, _positionsArray.Length, BufferUsage.WriteOnly);
                    vertexBuffer.SetData(_positionsArray);
                    _vertexBuffers.Add(vertexBuffer);
                }
            }
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
            proj  = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f, 100f);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.Black);

            // Allowed values for texture wrapping
            const int CLAMP_TO_EDGE = 33071;
            const int MIRRORED_REPEAT = 33648;
            const int REPEAT = 10497;

            foreach (var mesh in gltfFile.Meshes)
            {
                int i = 0;
                foreach (var primitive in mesh.Primitives)
                {
                    GraphicsDevice.SetVertexBuffer(_vertexBuffers[i]);

                    // Set BasicEffect parameters
                    _basicEffect.World = world * Matrix.CreateScale(1f);
                    _basicEffect.View = view;
                    _basicEffect.Projection = proj;

                    if (primitive.Material == null)
                    {
                        continue;
                    }

                    if (primitive.Material.BaseColorTexture == null)
                    {
                        continue;
                    }

                    var sampler = primitive.Material.BaseColorTexture.Sampler;

                    

                    // Setting texture wrapping modes
                    TextureAddressMode wrapS = TextureAddressMode.Wrap;
                    TextureAddressMode wrapT = TextureAddressMode.Wrap;

                    switch (sampler.WrapS)
                    {
                        case CLAMP_TO_EDGE:
                            wrapS = TextureAddressMode.Clamp;
                            break;
                        case MIRRORED_REPEAT:
                            wrapS = TextureAddressMode.Mirror;
                            break;
                        case REPEAT:
                            wrapS = TextureAddressMode.Wrap;
                            break;
                    }

                    switch (sampler.WrapT)
                    {
                        case CLAMP_TO_EDGE:
                            wrapT = TextureAddressMode.Clamp;
                            break;
                        case MIRRORED_REPEAT:
                            wrapT = TextureAddressMode.Mirror;
                            break;
                        case REPEAT:
                            wrapT = TextureAddressMode.Wrap;
                            break;
                    }

                    // Assuming _basicEffect.Texture is of type Texture2D
                    _basicEffect.Texture = _testTexture2D;
                    _basicEffect.Texture = loaded[primitive.Material.BaseColorTexture.Source.Uri];
                    _basicEffect.TextureEnabled = true;
                    _basicEffect.VertexColorEnabled = false;

                    // Apply the wrapping modes to the texture sampler state
                    _basicEffect.GraphicsDevice.SamplerStates[0] = new SamplerState
                    {
                        AddressU = wrapS,
                        AddressV = wrapT,
                        Filter = TextureFilter.Point
                    };

                    // Set alpha rendering mode
                    string alphaMode = primitive.Material.AlphaMode ?? "OPAQUE"; // Default to "OPAQUE" if not specified
                    float alphaCutoff = primitive.Material.AlphaCutoff ?? 0.5f; // Default alpha cutoff value for MASK mode

                    switch (alphaMode)
                    {
                        case "OPAQUE":
                            GraphicsDevice.BlendState = BlendState.Opaque;
                            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                            break;

                        case "MASK":
                        case "BLEND":
                            // For "MASK" mode, configure AlphaTestEffect instead of BasicEffect
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
                                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _vertexBuffers[i].VertexCount);
                            }
                            i++;
                            continue; // Skip the BasicEffect pass since we've used AlphaTestEffect
                            

                       
                    }

                    foreach (var pass in _basicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _vertexBuffers[i].VertexCount);
                    }
                    i++;
                }
            }


            base.Draw(gameTime);

            
        }
    }

}