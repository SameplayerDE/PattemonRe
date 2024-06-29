using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using HxGLTF;
using HxGLTF.Implementation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;

namespace TestRender;

public class Building
{
    public Vector3 Position;
    public string BuildingId;
    public GameModel Model;
    
    public bool IsLoaded => Model != null;

    public void Load(GraphicsDevice graphicsDevice)
    {
        var gltfFile = GLTFLoader.Load(@$"A:\ModelExporter\Platin\output_assets\{BuildingId}\{BuildingId}.gltf");
        var model = GameModel.From(graphicsDevice, gltfFile);
        model.TranslateTo(Position * 16);
        Model = model;
    }

    public void Unload()
    {
        Model?.Dispose();
        Model = null;
    }
}

public class Chunk
{
    //public ChunkHeader Header;
    public string Id;
    public List<Building> Buildings;
    public int[,] Collision = new int[32, 32];
    //public int X, Y;
    //public int Height;
    public GameModel Model;
    //public List<GameModel> Buildings = [];

    public bool IsLoaded => Model != null;

    public void Load(GraphicsDevice graphicsDevice)
    {
        foreach (var building in Buildings)
        {
            building.Load(graphicsDevice);
        }
        
        var basePath = @"A:\ModelExporter\Platin\overworldmaps\";
        string folderName = Id;
        string fileName = $"{folderName}.glb";
        string filePath = Path.Combine(basePath, folderName, fileName);
        
        if (File.Exists(filePath))
        {
            var gltfFile = GLTFLoader.Load(filePath);
            var gameModel = GameModel.From(graphicsDevice, gltfFile);
            Model = gameModel;
        }
    }

    public void Unload()
    {
        foreach (var building in Buildings)
        {
            building.Unload();
        }

        Model?.Dispose();
        Model = null;
    }
    
    public static Chunk Load(GraphicsDevice graphicsDevice, JToken jChunk)
    {
        var chunk = new Chunk();
        
        var chunkIdToken = jChunk["chunkId"];
        if (chunkIdToken == null)
        {
            throw new Exception();
        }
        chunk.Id = chunkIdToken.ToString();
        
        var chunkPermissionsToken = jChunk["permissions"];
        if (chunkPermissionsToken != null)
        {
            var collisionsToken = chunkPermissionsToken["collisions"];
            if (collisionsToken != null)
            {
                var collisions = collisionsToken.ToObject<string[][]>();

                for (var y = 0; y < collisions.Length; y++)
                {
                    var array = collisions[y];
                    for (var x = 0; x < array.Length; x++)
                    {
                        var item = array[x];

                        if (int.TryParse(item, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int value))
                        {
                            chunk.Collision[y, x] = value;
                        }
                        else
                        {
                            chunk.Collision[y, x] = 0;
                        }
                    }
                }
            }
        }
        
        var chunkBuildingsToken = jChunk["buildings"];
        if (chunkBuildingsToken == null)
        {
            throw new Exception();
        }
        chunk.Buildings = [];

        foreach (var jBuilding in chunkBuildingsToken)
        {
            if (jBuilding is not { HasValues: true })
            {
                continue;
            }
            
            var building = new Building();
                    
            var buildingIdToken = jBuilding["id"];
            if (buildingIdToken == null)
            {
                throw new Exception();
            }
            building.BuildingId = buildingIdToken.ToString();
                    
            var buildingXToken = jBuilding["x"];
            if (buildingXToken == null)
            {
                throw new Exception();
            }
            building.Position.X = buildingXToken.Value<float>();
                    
            var buildingYToken = jBuilding["y"];
            if (buildingYToken == null)
            {
                throw new Exception();
            }
            building.Position.Y = buildingYToken.Value<float>();
                    
            var buildingZToken = jBuilding["z"];
            if (buildingZToken == null)
            {
                throw new Exception();
            }
            building.Position.Z = buildingZToken.Value<float>();
            
            chunk.Buildings.Add(building);
        }
        
        
        
        return chunk;
    }
}