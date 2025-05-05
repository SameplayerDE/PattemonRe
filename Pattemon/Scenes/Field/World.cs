using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using PatteLib.World;

namespace Pattemon.Scenes.Field;

public class World
    {
        public const int ChunkWx = 32;
        public const int ChunkWy = 32;
        
        public static Queue<int> PendingChunkIds = new();
        public static Dictionary<int, Chunk> Chunks = [];

        public Dictionary<(int x, int y), (int chunkId, int headerId, int height)> Combination = [];

        private (int x, int y)? _lastLoadedCenter = null;
        
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

                if (!Chunks.ContainsKey(value.chunkId) && !PendingChunkIds.Contains(value.chunkId))
                {
                    //PendingChunkIds.Enqueue(value.chunkId);
                   //var chunkJson = File.ReadAllText($@"Content/WorldData/Chunks/{value.chunkId}.json");
                   //var jChunk = JObject.Parse(chunkJson);
                   //var chunk = Chunk.Load(graphicsDevice, jChunk);
                   //chunk.Load(graphicsDevice);
                   //Chunks.Add(chunk.Id, chunk);
                }
            }
            return world;
        }

        public static async Task LoadNextChunkAsync(GraphicsDevice graphicsDevice)
        {
            if (PendingChunkIds.Count == 0)
                return;

            int chunkId = PendingChunkIds.Dequeue();
            if (Chunks.ContainsKey(chunkId))
                return;

            // Laden im Hintergrund
            var chunkJson = await File.ReadAllTextAsync($"Content/WorldData/Chunks/{chunkId}.json");
            var jChunk = JObject.Parse(chunkJson);
            var chunk = Chunk.Load(graphicsDevice, jChunk);

            // GPU-Kram synchron
            await Task.Run(() =>
            {
                chunk.Load(graphicsDevice);
                lock (Chunks)
                {
                    Chunks[chunk.Id] = chunk;
                }
            });
        }
        
        public void UpdateChunkLoadingOnMove(Vector3 position, int radius)
        {
            int currentX = (int)(position.X / ChunkWx);
            int currentY = (int)(position.Z / ChunkWy);

            if (_lastLoadedCenter.HasValue && _lastLoadedCenter.Value.x == currentX && _lastLoadedCenter.Value.y == currentY)
                return; // Spieler ist im gleichen Chunk → nix machen

            _lastLoadedCenter = (currentX, currentY);
            EnqueueChunksAround(position, radius);
        }
        
        public void EnqueueChunksAround(Vector3 position, int radius)
        {
            int cx = (int)(position.X / ChunkWx);
            int cy = (int)(position.Z / ChunkWy);

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    var key = (cx + dx, cy + dy);
                    if (Combination.TryGetValue(key, out var tuple))
                    {
                        int chunkId = tuple.chunkId;
                        if (!Chunks.ContainsKey(chunkId) && !PendingChunkIds.Contains(chunkId))
                        {
                            PendingChunkIds.Enqueue(chunkId);
                        }
                    }
                }
            }
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

            return chunk.GetChunkPlateUnderPosition(new Vector3(localX, localZ, localY));
        }
    }