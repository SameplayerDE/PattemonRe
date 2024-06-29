using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;

namespace TestRender;

public class World
{
    public static Dictionary<string, Chunk> Chunks = [];
    public static Dictionary<string, ChunkHeader> Headers = [];
    public static bool IsDataFetched = false;
    
    public Dictionary<(int x, int y), (string chunkId, string headerId, int height)> Combination = [];

    public static World Load(GraphicsDevice graphicsDevice, string mapId)
    {
        var world = new World();
        
        var json = File.ReadAllText(@$"Content/WorldData/{mapId}.json");
        var jArray = JArray.Parse(json);
        foreach (var jCombination in jArray)
        {
            (int x, int y) key = (jCombination["x"].Value<int>(), jCombination["y"].Value<int>());
            (string chunkId, string headerId, int height) value = (jCombination["chunkId"].ToString(), jCombination["headerId"].ToString(), jCombination["height"].Value<int>());
            world.Combination.Add(key, value);
        }
        
        if (!IsDataFetched)
        {
            json = File.ReadAllText(@"Content/WorldData/Chunks.json");
            jArray = JArray.Parse(json);

            foreach (var jChunk in jArray)
            {
                var chunk = Chunk.Load(graphicsDevice, jChunk);
                chunk.Load(graphicsDevice);
                Chunks.Add(chunk.Id, chunk);
            }

            Headers = ChunkHeader.Load(@"Content/WorldData/Headers.json");

            IsDataFetched = true;
        }

        
        return world;
    }
    
}