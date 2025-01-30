using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HxGLTF.Implementation;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;
using PatteLib.World;
using Pattemon.Engine;
using Camera = Pattemon.Engine.Camera;

namespace Pattemon.Scenes;

public class GameplayScene : Scene
{
    public class World
    {
        public const int ChunkWx = 32;
        public const int ChunkWy = 32;

        public static Dictionary<int, Chunk> Chunks = [];

        public Dictionary<(int x, int y), (int chunkId, int headerId, int height)> Combination = [];

        public static World LoadByMatrix(GraphicsDevice graphicsDevice, int matrixId)
        {
            var world = new World();

            var json = File.ReadAllText(@$"Content/WorldData/Matrices/{matrixId}.json");
            var jArray = JArray.Parse(json);
            foreach (var jCombination in jArray)
            {
                (int x, int y) key = (jCombination["x"].Value<int>(), jCombination["y"].Value<int>());
                (int chunkId, int headerId, int height) value = (int.Parse(jCombination["mapId"].ToString()),
                    int.Parse(jCombination["headerId"].ToString()), jCombination["height"].Value<int>());
                world.Combination.Add(key, value);

                if (!Chunks.ContainsKey(value.chunkId))
                {
                    var chunkJson = File.ReadAllText($@"Content/WorldData/Chunks/{value.chunkId}.json");
                    var jChunk = JObject.Parse(chunkJson);
                    var chunk = Chunk.Load(graphicsDevice, jChunk);
                    chunk.Load(graphicsDevice);
                    Chunks.Add(chunk.Id, chunk);
                }
            }
            return world;
        }

        public Chunk GetChunkAtPosition(Vector3 position)
        {
            try
            {
                var chunkX = (int)position.X / ChunkWx;
                var chunkY = (int)position.Z / ChunkWy;
            
            
                if (!Combination.TryGetValue((chunkX, chunkY), out var tuple))
                {
                    throw new KeyNotFoundException($"Chunk at ({chunkX}, {chunkY}) not found in Combination dictionary.");
                }

                var chunkId = tuple.chunkId;

                if (!Chunks.TryGetValue(chunkId, out var chunk))
                {
                    throw new KeyNotFoundException($"Chunk with ID {chunkId} not found in Chunks dictionary.");
                }

                if (!chunk.IsLoaded || chunk.Model == null)
                {
                    throw new InvalidOperationException($"Chunk {chunkId} is not loaded or has a null model.");
                }

                return chunk;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetChunkAtPosition: {ex.Message}");
                return null; // Rückgabewert für Fehlerfall
            }
        }
        
        public int GetHeightAt(Vector3 position)
        {
            try
            {
                var chunkX = (int)position.X / ChunkWx;
                var chunkY = (int)position.Z / ChunkWy;
            
            
                if (!Combination.TryGetValue((chunkX, chunkY), out var tuple))
                {
                    throw new KeyNotFoundException($"Chunk at ({chunkX}, {chunkY}) not found in Combination dictionary.");
                }

                var chunkId = tuple.chunkId;

                if (!Chunks.TryGetValue(chunkId, out var chunk))
                {
                    throw new KeyNotFoundException($"Chunk with ID {chunkId} not found in Chunks dictionary.");
                }

                if (!chunk.IsLoaded || chunk.Model == null)
                {
                    throw new InvalidOperationException($"Chunk {chunkId} is not loaded or has a null model.");
                }

                return tuple.height;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetChunkAtPosition: {ex.Message}");
                return 0; // Rückgabewert für Fehlerfall
            }
        }

        public byte CheckTileCollision(Vector3 position)
        {
            try
            {
                var chunkX = (int)position.X / ChunkWx;
                var chunkY = (int)position.Z / ChunkWy;

                if (!Combination.TryGetValue((chunkX, chunkY), out var tuple))
                {
                    throw new KeyNotFoundException($"Chunk at ({chunkX}, {chunkY}) not found in Combination dictionary.");
                }

                var chunkId = tuple.chunkId;

                if (!Chunks.TryGetValue(chunkId, out var chunk))
                {
                    throw new KeyNotFoundException($"Chunk with ID {chunkId} not found in Chunks dictionary.");
                }

                if (!chunk.IsLoaded || chunk.Model == null)
                {
                    throw new InvalidOperationException($"Chunk {chunkId} is not loaded or has a null model.");
                }

                var cellX = (int)(position.X % ChunkWx);
                var cellY = (int)(position.Z % ChunkWy);
            
                if (cellX < 0 || cellX >= chunk.Collision.GetLength(1) || cellY < 0 ||
                    cellY >= chunk.Collision.GetLength(0))
                {
                    throw new IndexOutOfRangeException(
                        $"Cell coordinates ({cellX}, {cellY}) are out of bounds for chunk {chunkId}.");
                }

                return chunk.Collision[cellY, cellX];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CheckTileCollision: {ex.Message}");
                return 0x00;
            }
        }

        public byte CheckTileType(Vector3 position)
        {
            try
            {
                var chunkX = (int)position.X / ChunkWx;
                var chunkY = (int)position.Z / ChunkWy;

                if (!Combination.TryGetValue((chunkX, chunkY), out var tuple))
                {
                    throw new KeyNotFoundException($"Chunk at ({chunkX}, {chunkY}) not found in Combination dictionary.");
                }

                var chunkId = tuple.chunkId;

                if (!Chunks.TryGetValue(chunkId, out var chunk))
                {
                    throw new KeyNotFoundException($"Chunk with ID {chunkId} not found in Chunks dictionary.");
                }

                if (!chunk.IsLoaded || chunk.Model == null)
                {
                    throw new InvalidOperationException($"Chunk {chunkId} is not loaded or has a null model.");
                }

                var cellX = (int)(position.X % ChunkWx);
                var cellY = (int)(position.Z % ChunkWy);

                if (cellX < 0 || cellX >= chunk.Type.GetLength(1) || cellY < 0 || cellY >= chunk.Type.GetLength(0))
                {
                    throw new IndexOutOfRangeException(
                        $"Cell coordinates ({cellX}, {cellY}) are out of bounds for chunk {chunkId}.");
                }

                return chunk.Type[cellY, cellX];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CheckTileType: {ex.Message}");
                return 0x00;
            }
        }

        public ChunkPlate[] GetChunkPlateAtPosition(Vector3 position)
        {
            var chunk = GetChunkAtPosition(position);
            if (chunk == null)
            {
                return [];
            }

            if (chunk.Plates.Count == 0)
            {
                return [];
            }

            var x = (int)position.X;
            var y = (int)position.Y;
            var z = (int)position.Z;

            var result = new List<ChunkPlate>();

            foreach (var plate in chunk.Plates.Where(plate => plate.Z == z))
            {
                var minX = plate.X;
                var minY = plate.Y;
                var maxX = minX + plate.Wx;
                var maxY = minY + plate.Wy;

                if (x >= minX && x < maxX && y >= minY && y < maxY)
                {
                    result.Add(plate);
                }
            }

            return result.ToArray();
        }

        public ChunkPlate[] GetChunkPlateUnderPosition(Vector3 position)
        {
            var chunk = GetChunkAtPosition(position);
            if (chunk == null)
            {
                return [];
            }

            if (chunk.Plates.Count == 0)
            {
                return [];
            }


            var localX = (int)position.X % ChunkWx;
            var localY = (int)position.Z % ChunkWy;
            var localZ = position.Y;

            Console.WriteLine(localZ);

            return chunk.GetChunkPlateUnderPosition(new Vector3(localX, localZ, localY));
        }
    }
    
    private Effect _worldShader;
    private Effect _buildingShader;
    private Camera _camera;
    private World _world;
    
    private Texture2D _bottomDummy;
    private Vector3 _spawn = new Vector3(4, 0, 6);

    private float _rotation = 0f;
    
    public GameplayScene(string name, Game game, string contentDirectory = "Content") : base(name, game, contentDirectory)
    {
    }

    public override bool Load()
    {
        Building.RootDirectory = @"A:\ModelExporter\Platin\output_assets";
        Chunk.RootDirectory = @"A:\ModelExporter\Platin\overworldmaps";
        
        _bottomDummy = Content.Load<Texture2D>("BottomScreen");
        _worldShader = Content.Load<Effect>("Shaders/WorldShader");
        _buildingShader = Content.Load<Effect>("Shaders/BuildingShader");
        _world = World.LoadByMatrix(GraphicsDevice, 129);
        //_world = World.LoadByMatrix(GraphicsDevice, 0);
        return true;
    }

    public override bool Init()
    {
        _camera = Camera.CameraLookMap[4];
        Camera.ActiveCamera = _camera;
        return true;
    }

    public override bool Update(GameTime gameTime, float delta)
    {
        if (Child != null)
        {
            if (Child.Update(gameTime, delta))
            {
                Child.Exit();
                Child = null;
            }
            return false;
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.Escape))
        {
            // ToDo: open engine settings
        }
        
        if (KeyboardHandler.IsKeyDownOnce(Keys.X))
        {
            Child = SceneManager.GetScene(Scene.PlayerMenu);
            Child.SetManager(SceneManager);
            Child.Load();
            Child.Init();
        }
        
        if (KeyboardHandler.IsKeyDownOnce(Keys.Right))
        {
            _spawn.X++;
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.Left))
        {
            _spawn.X--;
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.Up))
        {
            _spawn.Z--;
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.Down))
        {
            _spawn.Z++;
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.PageUp))
        {
            _spawn.Y++;
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.PageDown))
        {
            _spawn.Y--;
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.U))
        {
            _spawn.X += 0.5f;
            _spawn.Z += 0.75f;
        }
        if (KeyboardHandler.IsKeyDownOnce(Keys.Space))
        {
            _spawn.Y = _world.GetHeightAt(_spawn);
        }

        _camera.SetAsActive();
        _camera.CaptureTarget(ref _spawn);
        _camera.ComputeViewMatrix();
        
        _rotation += delta;
        return false;
    }

    protected override void Draw2D(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
        RenderCore.SetBottomScreen();
        GraphicsDevice.Clear(Color.Black);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        spriteBatch.Draw(_bottomDummy, Vector2.Zero, Color.White);
        spriteBatch.End();

        Child?.Draw(spriteBatch, gameTime, delta);
    }

    protected override void Draw3D(GameTime gameTime, float delta)
    {
        RenderCore.SetTopScreen();
        GraphicsDevice.Clear(Color.Black);
        DrawWorldSmart(gameTime, _world, _spawn);

        Child?.Draw(null, gameTime, delta);
    }

    public override void Exit()
    {
        Unload();
    }

    private void DrawWorldSmart(GameTime gameTime, World world, Vector3 target, int renderDistance = 1)
    {
        int startX = -renderDistance;
        int endX = renderDistance;
        int startY = -renderDistance;
        int endY = renderDistance;

        // Ein Loop für opake und transparente Modelle
        foreach (var alpha in new[] { false, true })
        {
            for (int dx = startX; dx <= endX; dx++)
            {
                for (int dy = startY; dy <= endY; dy++)
                {
                    DrawChunk(gameTime, world, target, dx, dy, alpha);
                }
            }
        }
    }

    private void DrawChunk(GameTime gameTime, World world, Vector3 target, int dx, int dy, bool alpha)
    {
        var chunkX = (int)target.X / World.ChunkWx + dx;
        var chunkY = (int)target.Z / World.ChunkWy + dy;

        if (!_world.Combination.TryGetValue((chunkX, chunkY), out var tuple))
        {
            return;
        }

        var chunkId = tuple.chunkId;

        if (World.Chunks.TryGetValue(chunkId, out var chunk))
        {
            if (chunk.IsLoaded)
            {
                Vector3 offset = new Vector3(chunkX * World.ChunkWx, tuple.height / 2f, chunkY * World.ChunkWy) +
                                 new Vector3(World.ChunkWx, 0, World.ChunkWy) / 2;

                DrawModel(gameTime, _worldShader, chunk.Model, alpha, offset);

                foreach (var building in chunk.Buildings)
                {
                    DrawModel(gameTime, _buildingShader, building.Model, alpha, offset);
                }
            }
        }
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