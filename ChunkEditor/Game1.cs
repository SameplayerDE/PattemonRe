using System;
using System.Collections.Generic;
using System.IO;
using HxGLTF.Implementation;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;
using PatteLib.World;
using TextCopy;

namespace ChunkEditor
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Effect _effect;
        private Camera _previewCamera;
        private Camera _camera;
        private SpriteFont _font;
        private SpriteFont _consoleFont;
        private RenderTarget2D _chunkView;

        private List<Chunk> _chunks = new List<Chunk>();
        private int _currentChunk = 0;

        private bool _showCollisions;
        private bool _showTypes;
        private bool _showPlates;
        
        private Texture2D _gridTexture;
        private const int CellSize = 16;
        private const int ChunkSize = 32;
        private int _currCellX;
        private int _currCellY;

        private int _currentHeight = 0;
        private int _ax = 0;
        private int _ay = 0;
        
        //Editor

        //Right
        private int _prevRClickX;
        private int _prevRClickY;
        private int _currRClickX;
        private int _currRClickY;
        
        //Left
        private int _prevLClickX;
        private int _prevLClickY;
        private int _currLClickX;
        private int _currLClickY;
        
        //Plates

#nullable enable
        private ChunkPlate? _selectedPlate = null;
        
        private ConsoleState _currentConsoleState;
        private ConsoleState _previousConsoleState;
        
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            Building.RootDirectory = @"A:\ModelExporter\Platin\output_assets";
            Chunk.RootDirectory = @"A:\ModelExporter\Platin\overworldmaps";
            
            var chunkSize = 32;
            
            //Main Camera
            _camera = new Camera(GraphicsDevice);
            _camera.SetOrthographicSize(GraphicsDevice.Viewport.Bounds.Size.ToVector2());
            
            //Preview Camera
            _previewCamera = new Camera(GraphicsDevice);
            _previewCamera.SetOrthographicSize(chunkSize, chunkSize);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _effect = Content.Load<Effect>("WorldShader");
            _chunkView = new RenderTarget2D(GraphicsDevice, ChunkSize * CellSize, ChunkSize * CellSize, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24Stencil8);
            _font = Content.Load<SpriteFont>("Font");
            _consoleFont = Content.Load<SpriteFont>("ConsoleFont");
            
            _gridTexture = new Texture2D(GraphicsDevice, 1, 1);
            _gridTexture.SetData(new[] { Color.White });

            for (int i = 0; i < 200; i++)
            {
                var chunkJson = File.ReadAllText($@"A:\Coding\Survival\TestRender\Content\WorldData\Chunks\{i}.json");
                var jChunk = JObject.Parse(chunkJson);
                var chunk = Chunk.Load(GraphicsDevice, jChunk);
                chunk.Load(GraphicsDevice);
                _chunks.Add(chunk);
                Console.WriteLine("Loaded Chunk " + i);
            }

            // Position the camera to look from above
            var cameraPosition = new Vector3(0, 50, 0);  // Position the camera above the chunk
            _previewCamera.Teleport(cameraPosition);
            _previewCamera.Rotate(new Vector3(-MathHelper.PiOver2, 0, 0)); // Rotate the camera to look down
        }

        protected override void Update(GameTime gameTime)
        {
            if (!IsActive)
            {
                return;
            }
    
            _previousConsoleState = _currentConsoleState;
            _currentConsoleState = VirtualConsole.GetState();
            
            if (!GraphicsDevice.Viewport.Bounds.Contains(Mouse.GetState().Position))
            {
                return;
            }

            if (!_currentConsoleState.IsActive)
            {

                KeyboardHandler.Update(gameTime);
                MouseHandler.Update(gameTime);

                _currCellX = Mouse.GetState().X / CellSize;
                _currCellY = Mouse.GetState().Y / CellSize;

                if (MouseHandler.IsButtonDownOnce(MouseButton.Right))
                {
                    _prevRClickX = _currRClickX;
                    _prevRClickY = _currRClickY;
                    _currRClickX = Mouse.GetState().X;
                    _currRClickY = Mouse.GetState().Y;

                    Console.WriteLine($"----");
                    Console.WriteLine($"Rx: {_currRClickX / CellSize}");
                    Console.WriteLine($"Ry: {_currRClickY / CellSize}");
                }

                if (MouseHandler.IsButtonDownOnce(MouseButton.Left))
                {
                    _prevLClickX = _currLClickX;
                    _prevLClickY = _currLClickY;
                    _currLClickX = Mouse.GetState().X;
                    _currLClickY = Mouse.GetState().Y;

                    Console.WriteLine($"----");
                    Console.WriteLine($"Lx: {_currLClickX / CellSize}");
                    Console.WriteLine($"Ly: {_currLClickY / CellSize}");
                }

                if (KeyboardHandler.IsKeyDownOnce(Keys.Space))
                {
                    var x = _currLClickX / CellSize;
                    var y = _currLClickY / CellSize;
                    var z = _currentHeight;
                    var wx = Math.Abs(_currRClickX / CellSize - _currLClickX / CellSize) + 1;
                    var wy = Math.Abs(_currRClickY / CellSize - _currLClickY / CellSize) + 1;
                    var ax = _ax;
                    var ay = _ay;

                    var plateInfo = new ChunkPlate()
                    {
                        X = x,
                        Y = y,
                        Z = z,
                        Wx = wx,
                        Wy = wy,
                        Ax = ax,
                        Ay = ay
                    };

                    _chunks[_currentChunk].Plates.Add(plateInfo);


                    var plateInfoJson =
                        Newtonsoft.Json.JsonConvert.SerializeObject(plateInfo, Newtonsoft.Json.Formatting.Indented);
                    ClipboardService.SetText(plateInfoJson);
                    Console.WriteLine(plateInfoJson);
                }

                if (KeyboardHandler.IsKeyDownOnce(Keys.R))
                {
                    var chunkJson =
                        File.ReadAllText(
                            $@"A:\Coding\Survival\TestRender\Content\WorldData\Chunks\{_currentChunk}.json");
                    var jChunk = JObject.Parse(chunkJson);
                    var chunk = Chunk.Load(GraphicsDevice, jChunk);
                    chunk.Load(GraphicsDevice);
                    _chunks[_currentChunk] = chunk;
                    Console.WriteLine("Loaded Chunk " + _currentChunk);
                }

                if (MouseHandler.GetMouseWheelValueDelta() > 0)
                {
                    _currentHeight++;
                    //_camera.ZoomIn(0.25f);
                }
                else if (MouseHandler.GetMouseWheelValueDelta() < 0)
                {
                    _currentHeight--;
                    //_camera.ZoomOut(0.25f);
                }

                if (KeyboardHandler.IsKeyDownOnce(Keys.Up))
                {
                    _ax = -45;
                    _ay = 0;
                }

                if (KeyboardHandler.IsKeyDownOnce(Keys.Left))
                {
                    _ax = 0;
                    _ay = -45;
                }

                if (KeyboardHandler.IsKeyDownOnce(Keys.Right))
                {
                    _ax = 0;
                    _ay = 45;
                }

                if (KeyboardHandler.IsKeyDownOnce(Keys.Down))
                {
                    _ax = 45;
                    _ay = 0;
                }

                if (KeyboardHandler.IsKeyDownOnce(Keys.Enter))
                {
                    _ax = 0;
                    _ay = 0;
                }

                Console.WriteLine("--------");
                Console.WriteLine("Z: " + _currentHeight);
                Console.WriteLine("Chunk: " + _currentChunk);
                Console.WriteLine("Ax: " + _ax);
                Console.WriteLine("Ay: " + _ay);

                if (MouseHandler.IsButtonDown(MouseButton.Middle))
                {
                    var delta = MouseHandler.GetMouseDelta().ToVector2() / _camera.Zoom;
                    _camera.Position -= new Vector3(delta.X, delta.Y, 0);
                }

                //Toggle Rendering
                if (KeyboardHandler.IsKeyDownOnce(Keys.P))
                {
                    _showPlates = !_showPlates;
                }

                if (KeyboardHandler.IsKeyDownOnce(Keys.C))
                {
                    _showCollisions = !_showCollisions;
                }

                if (KeyboardHandler.IsKeyDownOnce(Keys.T))
                {
                    _showTypes = !_showTypes;
                }

                if (KeyboardHandler.IsKeyDownOnce(Keys.PageUp))
                {
                    _currentChunk += 1;
                    if (_currentChunk >= _chunks.Count)
                    {
                        _currentChunk = 0;
                    }
                }

                if (KeyboardHandler.IsKeyDownOnce(Keys.PageDown))
                {
                    _currentChunk -= 1;
                    if (_currentChunk < 0)
                    {
                        _currentChunk = 0;
                    }
                }

                if (KeyboardHandler.IsKeyDown(Keys.LeftControl))
                {
                    if (KeyboardHandler.IsKeyDownOnce(Keys.S))
                    {
                        var chunk = _chunks[_currentChunk];
                        var jChunk =
                            Chunk.Save(
                                chunk); // Annahme: Du hast eine Methode ToJObject() implementiert, um den Chunk zu serialisieren
                        var chunkJson = jChunk.ToString();
                        File.WriteAllText(
                            $@"A:\Coding\Survival\TestRender\Content\WorldData\Chunks\{_currentChunk}.json", chunkJson);
                        Console.WriteLine("Saved Chunk " + _currentChunk);
                    }

                    if (KeyboardHandler.IsKeyDownOnce(Keys.L))
                    {
                        string fileName =
                            $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 6)}.png";
                        using var stream = File.Create(fileName);
                        _chunkView.SaveAsPng(stream, 32 * 16, 32 * 16);
                    }
                }

                if (KeyboardHandler.IsKeyDownOnce(Keys.Q))
                {
                    _chunks[_currentChunk].Plates.Clear();
                }

                _previewCamera.Update();
                _camera.Update();

            }
            
            VirtualConsole.Update(gameTime);

            base.Update(gameTime);
        }
        
        private Color GetPlateColor(float ax, float ay)
        {
            if (ax == 0 && ay == 0)
            {
                return Color.Purple; // no angle
            }
            else if (ax == -45 && ay == 0)
            {
                return Color.Red; // up
            }
            else if (ax == 45 && ay == 0)
            {
                return Color.Blue; // down
            }
            else if (ax == 0 && ay == -45)
            {
                return Color.Yellow; // left
            }
            else if (ax == 0 && ay == 45)
            {
                return Color.Green; // right
            }
            else
            {
                return Color.White; // default color for other angles
            }
        }
        
        private void DrawPreview(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_chunkView);
            GraphicsDevice.Clear(Color.CornflowerBlue);
            DrawModel(gameTime, _effect, _chunks[_currentChunk].Model);
            foreach (var building in _chunks[_currentChunk].Buildings)
            {
                DrawModel(gameTime, _effect, building.Model);
            }
            DrawModel(gameTime, _effect, _chunks[_currentChunk].Model, alpha: true);
            foreach (var building in _chunks[_currentChunk].Buildings)
            {
                DrawModel(gameTime, _effect, building.Model, alpha: true);
            }
            
            /*_spriteBatch.Begin();
            if (_showPlates)
            {
                foreach (var plate in _chunks[_currentChunk].Plates)
                {
                    // Berechne die Position relativ zur Kamera und mit Zoom
                    var x = plate.X * _camera.Zoom; // X
                    var y = plate.Y * _camera.Zoom; // Y (assuming Z is height and needs to be subtracted for proper alignment)
                    var wx = plate.Wx * _camera.Zoom; // Width X
                    var wy = plate.Wy * _camera.Zoom; // Width Y
                    var ax = plate.Ax; // Angle X
                    var ay = plate.Ay; // Angle Y

                    // Convert to screen coordinates
                    var screenX = (int)(x * CellSize);
                    var screenY = (int)(y * CellSize);
                    var screenWx = (int)(wx * CellSize);
                    var screenWy = (int)(wy * CellSize);

                    // Plate bounds
                    var plateBounds = new Rectangle(screenX, screenY, screenWx, screenWy);
                    var plateColor = GetPlateColor(ax, ay);

                    if (_selectedPlate == plate)
                    {
                        DrawPlate(_spriteBatch, _gridTexture, plateBounds, Color.Green * 0.5f, Color.White);
                    }
                    else
                    {

                        DrawPlate(_spriteBatch, _gridTexture, plateBounds, plateColor * 0.5f, Color.White);
                        _spriteBatch.DrawString(_font, $"{plate.Z}", plateBounds.Center.ToVector2(), Color.White);
                    }
                }
            }

            if (_showCollisions)
            {
                for (var y = 0; y < _chunks[_currentChunk].Collision.GetLength(0); y++)
                {
                    for (var x = 0; x < _chunks[_currentChunk].Collision.GetLength(1); x++)
                    {
                        var collisionId = _chunks[_currentChunk].Collision[y, x];
                        if (collisionId == 0x80)
                        {
                            _spriteBatch.Draw(_gridTexture, new Rectangle(x * CellSize, y * CellSize, 16, 16), Color.Red * 0.5f);
                        }

                        //_spriteBatch.DrawString(_font, $"{collisionId:x2}", new Vector2(x * CellSize, y * CellSize), Color.Red * 0.5f);
                    }
                }
            }

            _spriteBatch.End();*/
            
            GraphicsDevice.SetRenderTarget(null);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            DrawPreview(gameTime);

            _spriteBatch.Begin(samplerState: SamplerState.PointWrap);
            _spriteBatch.Draw(_chunkView, new Vector2(-1, -1), Color.White);
            _spriteBatch.End();
            
            _spriteBatch.Begin();
            //Collisions

            if (_showCollisions)
            {
                for (var y = 0; y < _chunks[_currentChunk].Collision.GetLength(0); y++)
                {
                    for (var x = 0; x < _chunks[_currentChunk].Collision.GetLength(1); x++)
                    {
                        var collisionId = _chunks[_currentChunk].Collision[y, x];
                        if (collisionId == (int)ChunkTileCollision.Blocked)
                        {
                            _spriteBatch.Draw(_gridTexture, new Rectangle(x * CellSize, y * CellSize, 16, 16), Color.Red * 0.5f);
                        }
                        else if (collisionId == (int)ChunkTileCollision.GrassSound)
                        {
                            _spriteBatch.Draw(_gridTexture, new Rectangle(x * CellSize, y * CellSize, 16, 16), Color.Green * 0.5f);
                        }

                        //_spriteBatch.DrawString(_font, $"{collisionId:x2}", new Vector2(x * CellSize, y * CellSize), Color.Red * 0.5f);
                    }
                }
            }

            //Types
            
            if (_showTypes)
            {
                for (var y = 0; y < _chunks[_currentChunk].Type.GetLength(0); y++)
                {
                    for (var x = 0; x < _chunks[_currentChunk].Type.GetLength(1); x++)
                    {
                        var typeId = _chunks[_currentChunk].Type[y, x];
                        _spriteBatch.Draw(_gridTexture, new Rectangle(x * CellSize, y * CellSize, 16, 16), Color.Black * 0.3f);
                        _spriteBatch.DrawString(_font, $"{typeId:x2}", new Vector2(x * CellSize, y * CellSize), Color.White);
                    }
                }
            }
            
            //Plates

            if (_showPlates)
            {
                foreach (var plate in _chunks[_currentChunk].Plates)
                {
                    // Berechne die Position relativ zur Kamera und mit Zoom
                    var x = plate.X * _camera.Zoom; // X
                    var y = plate.Y * _camera.Zoom; // Y (assuming Z is height and needs to be subtracted for proper alignment)
                    var wx = plate.Wx * _camera.Zoom; // Width X
                    var wy = plate.Wy * _camera.Zoom; // Width Y
                    var ax = plate.Ax; // Angle X
                    var ay = plate.Ay; // Angle Y

                    // Convert to screen coordinates
                    var screenX = (int)(x * CellSize);
                    var screenY = (int)(y * CellSize);
                    var screenWx = (int)(wx * CellSize);
                    var screenWy = (int)(wy * CellSize);

                    // Plate bounds
                    var plateBounds = new Rectangle(screenX, screenY, screenWx, screenWy);
                    var plateColor = GetPlateColor(ax, ay);

                    if (_selectedPlate == plate)
                    {
                        DrawPlate(_spriteBatch, _gridTexture, plateBounds, Color.Green * 0.5f, Color.White);
                    }
                    else
                    {
                       
                        DrawPlate(_spriteBatch, _gridTexture, plateBounds, plateColor * 0.5f, Color.White);
                        _spriteBatch.DrawString(_font, $"{plate.Z}", plateBounds.Center.ToVector2(), Color.White);
                    }
                }
            }
            
            //Selection
            
            _spriteBatch.Draw(_gridTexture, new Rectangle(_currCellX * CellSize, _currCellY * CellSize, (int)(16 * _camera.Zoom), (int)(16 * _camera.Zoom)), Color.Red * 0.5f);
            _spriteBatch.End();
            
            DrawConsole(_consoleFont);
            
            base.Draw(gameTime);
        }

        private void DrawConsole(SpriteFont font)
        {
            if (!_currentConsoleState.IsActive)
            {
                return;
            }
        
            _spriteBatch.Begin();

            DrawConsoleLine(font);
            DrawConsoleLog(font);
            DrawConsoleSuggestions(font);
        
            _spriteBatch.End();
        }

        private void DrawConsoleLine(SpriteFont font)
        {
            // background on whole screen
            _spriteBatch.Draw(_gridTexture, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.Black * 0.50f);
            // background for input line
            _spriteBatch.Draw(_gridTexture, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, font.LineSpacing), Color.Black * 0.75f);
            // current content of input
            _spriteBatch.DrawString(font, _currentConsoleState.Content, Vector2.Zero, Color.White);
        
            // calculate position of cursor
            var textBeforeCursor = _currentConsoleState.Content[..Math.Min(_currentConsoleState.Cursor, _currentConsoleState.Content.Length)];
            var cursorPosition = font.MeasureString(textBeforeCursor);
            var cursorRect = new Rectangle((int)cursorPosition.X,0,2,font.LineSpacing);
            // cursor
            _spriteBatch.Draw(_gridTexture, cursorRect, Color.White);
        }

        private void DrawConsoleLog(SpriteFont font)
        {
            // history and log
            float offsetY = font.LineSpacing;
        
            for (var i = 0; i < Math.Min(_currentConsoleState.Log.Count, 10); i++)
            {
                var commandIndex = _currentConsoleState.Log.Count - 1 - i;
                if (commandIndex < 0)
                {
                    break;
                }
                var logContent = _currentConsoleState.Log[commandIndex];
                var position = new Vector2(0, offsetY * (1 + i));
                var primaryColor = Color.White;
                _spriteBatch.DrawString(font, logContent, position, primaryColor);
            }
        }

        private void DrawConsoleSuggestions(SpriteFont font)
        {
            var suggestions = _currentConsoleState.Suggestions;
            if (suggestions != null)
            {
                int index = 0;
                foreach (var suggestion in suggestions)
                {
                    var position = new Vector2(font.MeasureString( _currentConsoleState.Content).X, font.LineSpacing * (1 + index));
                    var primaryColor = Color.White;
                    _spriteBatch.Draw(_gridTexture, new Rectangle((int)font.MeasureString( _currentConsoleState.Content).X, (int)position.Y, (int)font.MeasureString(suggestion).X, font.LineSpacing), null, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0f);
                    _spriteBatch.DrawString(font, suggestion, position, primaryColor, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
                    index++;
                }
            }
        }
        
        private void DrawPlate(SpriteBatch spriteBatch, Texture2D texture, Rectangle bounds, Color color, Color borderColor, int borderThickness = 2)
        {
            // Render Inner
            spriteBatch.Draw(texture, bounds, color);

            // Render Border

            // Top Border
            spriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y, bounds.Width, borderThickness), borderColor);

            // Bottom Border
            spriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y + bounds.Height - borderThickness, bounds.Width, borderThickness), borderColor);

            // Left Border
            spriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y, borderThickness, bounds.Height), borderColor);

            // Right Border
            spriteBatch.Draw(texture, new Rectangle(bounds.X + bounds.Width - borderThickness, bounds.Y, borderThickness, bounds.Height), borderColor);
        }
        
        private void DrawModel(GameTime gameTime, Effect effect, GameModel model, bool alpha = false, Vector3 offset = default)
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

        private void DrawNode(GameTime gameTime, Effect effect, GameModel model, GameNode node, bool alpha = false, Vector3 offset = default)
        {
            if (node.HasMesh)
            {
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
            effect.Parameters["View"].SetValue(_previewCamera.View);
            effect.Parameters["Projection"].SetValue(_previewCamera.Projection);
            effect.Parameters["SkinningEnabled"]?.SetValue(false);

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

            if (primitive.Material != null)
            {
                var material = primitive.Material;

                effect.Parameters["BaseColorFactor"]?.SetValue(material.BaseColorFactor.ToVector4());
                effect.Parameters["AlphaCutoff"]?.SetValue(material.AlphaCutoff);

                if (material.HasTexture)
                {
                    effect.Parameters["TextureEnabled"]?.SetValue(true);
                    effect.Parameters["TextureDimensions"]?.SetValue(material.BaseTexture.Texture.Bounds.Size.ToVector2());
                    effect.Parameters["Texture"]?.SetValue(material.BaseTexture.Texture);
                    effect.Parameters["TextureAnimation"]?.SetValue(false);

                    GraphicsDevice.SamplerStates[0] = material.BaseTexture.Sampler.SamplerState;
                }
            }
        }
    }
}