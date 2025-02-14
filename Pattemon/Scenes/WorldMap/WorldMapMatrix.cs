using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Pattemon.Scenes.WorldMap;

public class WorldMapMatrix
{
    private Dictionary<(int x, int y), WorldMapMatrixEntry> _entries = [];

    public static WorldMapMatrix LoadFromFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("The file could not be found.", path);
        }

        var fileContent = File.ReadAllText(path);
        var jArray = JArray.Parse(fileContent);

        // Determine Width and Height based on the data
        int maxX = 0;
        int maxY = 0;
        foreach (var jCombination in jArray)
        {
            int x = jCombination["x"].Value<int>();
            int y = jCombination["y"].Value<int>();
            maxX = Math.Max(maxX, x);
            maxY = Math.Max(maxY, y);
        }

        var matrixData = new WorldMapMatrix();
        
        foreach (var jCombination in jArray)
        {
            int x = jCombination["x"].Value<int>();
            int y = jCombination["y"].Value<int>();
            var cellData = new WorldMapMatrixEntry()
            {
               Name = jCombination["name"].Value<string>()
            };
            matrixData._entries[(x, y)] = cellData;
        }

        return matrixData;
    }
    
    public WorldMapMatrixEntry Get(int x, int y)
    {
        if (_entries.ContainsKey((x, y)))
        {
            return _entries[(x, y)];
        }
        return WorldMapMatrixEntry.Empty;
    }
    
}