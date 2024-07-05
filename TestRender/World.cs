using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json.Linq;

namespace TestRender;

public class World
{

    public const int ChunkWx = 32;
    public const int ChunkWy = 32;
    
    public static Dictionary<int, Chunk> Chunks = [];
    public static Dictionary<int, ChunkHeader> Headers = [];
    public static bool IsDataFetched = false;
    
    public Dictionary<(int x, int y), (int chunkId, int headerId, int height)> Combination = [];

    public static World Load(GraphicsDevice graphicsDevice, int mapId)
    {
        var world = new World();
        
        var json = File.ReadAllText(@$"Content/WorldData/Matrices/{mapId}.json");
        var jArray = JArray.Parse(json);
        foreach (var jCombination in jArray)
        {
            (int x, int y) key = (jCombination["x"].Value<int>(), jCombination["y"].Value<int>());
            (int chunkId, int headerId, int height) value = (int.Parse(jCombination["mapId"].ToString()), int.Parse(jCombination["headerId"].ToString()), jCombination["height"].Value<int>());
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
        
        if (!IsDataFetched)
        {
            for (int i = 0; i < 592; i++)
            {
                var headerJson = File.ReadAllText($@"Content/WorldData/Headers/{i}.json");
                var jHeader = JObject.Parse(headerJson);
                var header = ChunkHeader.Load(jHeader);
                Headers.Add(header.Id, header);
            }

            IsDataFetched = true;
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
    
    public byte CheckTileCollision(Vector3 position)
    {
        try
        {
            var chunkX = (int)position.X / 512;
            var chunkY = (int)position.Z / 512;

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

            var cellX = (int)(position.X % 512) / 16;
            var cellY = (int)(position.Z % 512) / 16;

            if (cellX < 0 || cellX >= chunk.Collision.GetLength(1) || cellY < 0 || cellY >= chunk.Collision.GetLength(0))
            {
                throw new IndexOutOfRangeException($"Cell coordinates ({cellX}, {cellY}) are out of bounds for chunk {chunkId}.");
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
            var chunkX = (int)position.X / 512;
            var chunkY = (int)position.Z / 512;

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

            var cellX = (int)(position.X % 512) / 16;
            var cellY = (int)(position.Z % 512) / 16;

            if (cellX < 0 || cellX >= chunk.Type.GetLength(1) || cellY < 0 || cellY >= chunk.Type.GetLength(0))
            {
                throw new IndexOutOfRangeException($"Cell coordinates ({cellX}, {cellY}) are out of bounds for chunk {chunkId}.");
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
        var localZ = (int)position.Y;

        var result = new List<ChunkPlate>();

        foreach (var plate in chunk.Plates)
        {
            var minX = plate.X;
            var minY = plate.Y;
            var maxX = minX + plate.Wx;
            var maxY = minY + plate.Wy;

            if (localX >= minX && localX < maxX && localY >= minY && localY < maxY)
            {
                if (localZ >= plate.Z)
                {
                    result.Add(plate);
                }
            }
        }
        
        result.Sort((p1, p2) => p2.Z.CompareTo(p1.Z));
        return result.ToArray();
    }
}