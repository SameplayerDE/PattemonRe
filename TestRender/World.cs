using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;

namespace TestRender;

public class World
{
    public static Dictionary<int, Chunk> Chunks = [];
    public static Dictionary<string, ChunkHeader> Headers = [];
    public static bool IsDataFetched = false;
    
    public Dictionary<(int x, int y), (int chunkId, string headerId, int height)> Combination = [];

    public static World Load(GraphicsDevice graphicsDevice, string mapId)
    {
        var world = new World();
        
        var json = File.ReadAllText(@$"Content/WorldData/{mapId}.json");
        var jArray = JArray.Parse(json);
        foreach (var jCombination in jArray)
        {
            (int x, int y) key = (jCombination["x"].Value<int>(), jCombination["y"].Value<int>());
            (int chunkId, string headerId, int height) value = (int.Parse(jCombination["chunkId"].ToString()), jCombination["headerId"].ToString(), jCombination["height"].Value<int>());
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
            Headers = ChunkHeader.Load(@"Content/WorldData/Headers.json");
            IsDataFetched = true;
        }
        
        return world;
    }
    
    public Chunk GetChunkAtPosition(Vector3 position)
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
}