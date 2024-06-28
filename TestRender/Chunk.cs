using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using HxGLTF;
using HxGLTF.Implementation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static System.Globalization.NumberStyles;

namespace TestRender;

public class Chunk
{
    public ChunkHeader Header;
    public string Id;
    public int X, Y;
    public int Height;
    public GameModel Terrain;
    public List<GameModel> Buildings = [];

    public static List<Chunk> Load(GraphicsDevice graphicsDevice)
    {

        var result = new List<Chunk>();
        
        var mapFileMatrix = Utils.ReadMatrix(@"Content\MapFileMatrix.csv");
        var mapHeaderMatrix = Utils.ReadMatrix(@"Content\MapHeaderMatrix.csv");
        var mapHeightMatrix = ReadNumberMatrixFromFile(@"Content\MapHeightMatrix.csv");
        var mapObjects = ReadModelMatrix(graphicsDevice, @"Content\MapObjectMatrix.csv");
        var chunkHeaders = ChunkHeader.Load(@"Content\Header0411.json");
        
        int maxChunksX = 128;
        int maxChunksY = 128;
            
        string basePath = @"A:\ModelExporter\Platin\overworldmaps\";
        //basePath = @"A:\ModelExporter\black2\output_assets\";
        //basePath = @"A:\ModelExporter\heartgold\output_assets\";

        for (int x = 0; x <= maxChunksX; x++)
        {
            for (int y = 0; y <= maxChunksY; y++)
            {
                if (y >= mapFileMatrix.GetLength(0) || x >= mapFileMatrix.GetLength(1))
                {
                    continue;
                }
                string folderName = mapFileMatrix[y, x];
                string headerName = mapHeaderMatrix[y, x];
                string fileName = $"{folderName}.glb";
                string filePath = Path.Combine(basePath, folderName, fileName);

                if (File.Exists(filePath))
                {
                    var chunkHeight = mapHeightMatrix[y, x];
                    var gltfFile = GLTFLoader.Load(filePath);
                    var gameModel = GameModel.From(graphicsDevice, gltfFile);
                    gameModel.Translation = new Vector3(x * 512, chunkHeight * 8, y * 512);

                    var chunk = new Chunk();
                    chunk.Header = chunkHeaders.GetValueOrDefault(headerName);

                    if (chunk.Header != null)
                    {
                        
                    }
                    
                    chunk.Id = folderName;
                    chunk.X = x;
                    chunk.Y = y;
                    chunk.Id = folderName;
                    chunk.Height = chunkHeight;
                    chunk.Terrain = gameModel;
                    //add only corrects mopdels to this chunk
   
                    result.Add(chunk);
                }
            }
        }
        
        return result;
    }
    
    static int[,] ReadNumberMatrixFromFile(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            int rows = lines.Length;
            int cols = lines[0].Split(',').Length;

            int[,] matrix = new int[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                string[] values = lines[i].Split(',');
                for (int j = 0; j < cols; j++)
                {
                    if (!string.IsNullOrEmpty(values[j]))
                    {
                        if (int.TryParse(values[j], out int number))
                        {
                            matrix[i, j] = number;
                        }
                        else
                        {
                            // Handle invalid number format here if needed
                            matrix[i, j] = 0; // Default value if conversion fails
                        }
                    }
                    else
                    {
                        // Handle empty string or null case
                        matrix[i, j] = 0; // Default value if string is empty or null
                    }
                }
            }

            return matrix;
        }
        
    static List<GameModel> ReadModelMatrix(GraphicsDevice graphicsDevice, string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);

            // Initialisiere die Liste von GameModel-Objekten
            List<GameModel> models = new List<GameModel>();

            for (int i = 0; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                if (values.Length == 6)
                {
                    if (int.TryParse(values[0], out int cx) &&
                        int.TryParse(values[1], out int cy) &&
                        float.TryParse(values[3], Float, CultureInfo.InvariantCulture, out float x) &&
                        float.TryParse(values[4], Float, CultureInfo.InvariantCulture, out float y) &&
                        float.TryParse(values[5], Float, CultureInfo.InvariantCulture, out float z))
                    {
                        var gltfFile = GLTFLoader.Load(@$"A:\ModelExporter\Platin\output_assets\{values[2]}\{values[2]}.gltf");
                        var model = GameModel.From(graphicsDevice, gltfFile);
                        //model.TranslateTo(new Vector3(x, y, z) * 16);
                        model.TranslateTo(new Vector3(cx * 512, 0, cy * 512) + new Vector3(x, y, z) * 16);
                        models.Add(model);
                    }
                }
            }

            return models;
        }
    
}