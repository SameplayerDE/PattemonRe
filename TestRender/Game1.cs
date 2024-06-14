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
using System.Xml.Linq;

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
        private Effect _effect;

        private Camera _camera;
        
        private Texture2D _testTexture2D;

        private float AnimationTimer = 0.00f;
        
        private BasicEffect _basicEffect;
        private GameModel gameModel;
        private GameModel gameModel2;

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

        

        protected override void Initialize()
        {
            _basicEffect = new BasicEffect(GraphicsDevice);
            
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;
            
            _camera = new Camera(GraphicsDevice);

            GLTFFile gltfFile;
            //gltfFile = GLTFLoader.Load(@"Content\pkemon_oben");
            //gltfFile = GLTFLoader.Load(@"Content\Cube.glb");
            gltfFile = GLTFLoader.Load(@"A:\ModelExporter\black2\output_assets\m_dun3501_01_00\m_dun3501_01_00.glb");
            gameModel2 = GameModel.From(GraphicsDevice, gltfFile);
            //gltfFile = GLTFLoader.Load(@"A:\ModelExporter\black2\output_assets\badgegate_02\badgegate_02.glb");
            gltfFile = GLTFLoader.Load(@"Content\Simple");
            //gltfFile = GLTFLoader.Load(@"Content\helm");
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

        // Function to convert quaternion to Euler angles
        public Vector3 ToEulerAngles(Quaternion q)
        {
            Vector3 angles;

            // Roll (x-axis rotation)
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // Pitch (y-axis rotation)
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
                angles.Y = (float)Math.CopySign(Math.PI / 2, sinp); // Use 90 degrees if out of range
            else
                angles.Y = (float)Math.Asin(sinp);

            // Yaw (z-axis rotation)
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
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

            // animation


            AnimationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds / 2;


            if (gameModel.HasAnimations)
            {
                var animation = gameModel.Animations.First().Value;
                foreach (var channel in animation.Channels)
                {
                    var targetPath = channel.Target.Path;

                    var valuesPerElement = targetPath switch
                    {
                        "rotation" => 4,
                        "translation" or "scale" => 3,
                        _ => 0
                    };

                    if (valuesPerElement > 0)
                    {
                        var sampler = animation.Samplers[channel.SamplerIndex];
                        float[] timeStamps = sampler.Input;

                        //if (AnimationTimer > timeStamps.Last())
                        //{
                        //    AnimationTimer = 0;
                        //}
                        
                        int prevIndex = -1;
                        int nextIndex = -1;

                        // Find the indices for the surrounding keyframes
                        for (int i = 0; i < timeStamps.Length; i++)
                        {
                            if (timeStamps[i] <= AnimationTimer)
                            {
                                prevIndex = i;
                            }

                            if (timeStamps[i] > AnimationTimer)
                            {
                                nextIndex = i;
                                break;
                            }
                        }

                        if (nextIndex == -1)
                        {
                            nextIndex = 0;
                        }

                        if (sampler.InterpolationAlgorithm == InterpolationAlgorithm.Step)
                        {
                            // For step interpolation, just use the previous index
                            if (prevIndex >= 0)
                            {
                                var data = new float[valuesPerElement];
                                for (int i = 0; i < valuesPerElement; i++)
                                {
                                    int offset = prevIndex * valuesPerElement;
                                    data[i] = sampler.Output[offset + i];
                                }

                                if (targetPath == "rotation")
                                {
                                    Quaternion rotation = new Quaternion(data[0], data[1], data[2], data[3]);
                                    gameModel.Nodes[channel.Target.NodeIndex].Rotate(rotation);
                                }
                            }
                        }
                        else if (sampler.InterpolationAlgorithm == InterpolationAlgorithm.Linear)
                        {
                            // For linear interpolation, use both prev and next indices
                            if (prevIndex >= 0 && nextIndex >= 0)
                            {
                                var prevTimeStamp = timeStamps[prevIndex];
                                var nextTimeStamp = timeStamps[nextIndex];
                                float t;

                                // Sicherstellen, dass t im Bereich von 0 bis 1 liegt
                                if (nextTimeStamp > prevTimeStamp)
                                {
                                    t = (AnimationTimer - prevTimeStamp) / (nextTimeStamp - prevTimeStamp);
                                    t = MathHelper.Clamp(t, 0f, 1f); // Clamp auf den Bereich von 0 bis 1
                                }
                                else
                                {
                                    t = 0f; // Fall, wenn prevTimeStamp == nextTimeStamp (selten, aber möglich)
                                }

                                Console.WriteLine("Time inter: " + t);

                                var prevData = new float[valuesPerElement];
                                var nextData = new float[valuesPerElement];
                                for (int i = 0; i < valuesPerElement; i++)
                                {
                                    int offsetPrev = prevIndex * valuesPerElement;
                                    int offsetNext = nextIndex * valuesPerElement;
                                    prevData[i] = sampler.Output[offsetPrev + i];
                                    nextData[i] = sampler.Output[offsetNext + i];
                                }
                                
                                if (targetPath == "rotation")
                                {
                                    // Use Slerp for smoother quaternion interpolation
                                    Quaternion prevRotation = new Quaternion(prevData[0], prevData[1], prevData[2], prevData[3]);
                                    Quaternion nextRotation = new Quaternion(nextData[0], nextData[1], nextData[2], nextData[3]);
                                    Quaternion rotation = Quaternion.Slerp(prevRotation, nextRotation, t);
                                    // Apply the rotation
                                    gameModel.Nodes[channel.Target.NodeIndex].Rotate(rotation);

                                }
                                else if (targetPath == "translation")
                                {
                                    
                                    // Use Slerp for smoother quaternion interpolation
                                    Vector3 prev = new Vector3(prevData[0], prevData[1], prevData[2]);
                                    Vector3 next = new Vector3(nextData[0], nextData[1], nextData[2]);
                                    Vector3 translation = Vector3.Lerp(prev, next, t);
                                    gameModel.Nodes[channel.Target.NodeIndex].Translate(translation);
                                    
                                }
                                else if (targetPath == "scale")
                                {
                                    // Use Slerp for smoother quaternion interpolation
                                    Vector3 prev = new Vector3(prevData[0], prevData[1], prevData[2]);
                                    Vector3 next = new Vector3(nextData[0], nextData[1], nextData[2]);
                                    Vector3 scale = Vector3.Lerp(prev, next, t);
                                    gameModel.Nodes[channel.Target.NodeIndex].Resize(scale);
                                }
                            }
                        }
                    }

                    if (AnimationTimer > animation.Duration)
                    {
                        AnimationTimer = 0;
                    }
                    
                }

                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                base.Update(gameTime);
            }
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            foreach (var scene in gameModel.Scenes)
            {
                foreach (var nodeIndex in scene.Nodes)
                {
                    var node = gameModel.Nodes[nodeIndex];
                    DrawNode(node);
                }
            }
            
            foreach (var scene in gameModel2.Scenes)
            {
                foreach (var nodeIndex in scene.Nodes)
                {
                    var node = gameModel2.Nodes[nodeIndex];
                    DrawNode(node);
                }
            }

            base.Draw(gameTime);
        }

        private void DrawNode(GameNode node)
        {
            if (node.HasMesh)
            {
                //DrawMesh(node, gameModel.Meshes[node.MeshIndex]);
                DrawMesh(node, node.Mesh);
            }
            
            if (node.HasChildren)
            {
                foreach (var child in node.Children)
                {
                    DrawNode(node.Model.Nodes[child]);
                }
            }
        }

        private void DrawMesh(GameNode node, GameMesh mesh)
{
    foreach (var primitive in mesh.Primitives)
    {
        GraphicsDevice.SetVertexBuffer(primitive.VertexBuffer);

        // Set BasicEffect parameters
        _basicEffect.World = node.GlobalTransform * Matrix.CreateScale(0.5f);
        _basicEffect.View = _camera.View;
        _basicEffect.Projection = _camera.Projection;
        
        // Check if there is a material assigned
        if (primitive.Material != null)
        {
            // Check if there is a base texture assigned
            if (primitive.Material.BaseTexture != null)
            {
                _basicEffect.Texture = primitive.Material.BaseTexture.Texture;
                _basicEffect.TextureEnabled = true;
                _basicEffect.VertexColorEnabled = false; // Disable vertex colors when using textures

                // Set texture sampler state
                GraphicsDevice.SamplerStates[0] = primitive.Material.BaseTexture.Sampler.SamplerState;
            }
            else
            {
                _basicEffect.TextureEnabled = false;
                _basicEffect.VertexColorEnabled = true; // Enable vertex colors if no texture is used
            }
        }
        else
        {
            _basicEffect.TextureEnabled = false;
            _basicEffect.VertexColorEnabled = true; // Enable vertex colors if no material is assigned
        }

        GraphicsDevice.BlendState = BlendState.AlphaBlend;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        

        // Apply the effect and draw the primitives
        foreach (var pass in _basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, primitive.VertexBuffer.VertexCount / 3);
        }
    }
}


    }

}