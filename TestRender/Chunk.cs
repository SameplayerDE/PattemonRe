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
    public string BuildingName;
    public int BuildingId;
    public GameModel Model;
    
    public bool IsLoaded => Model != null;

    public void Load(GraphicsDevice graphicsDevice)
    {
        var gltfFile = GLTFLoader.Load(@$"A:\ModelExporter\Platin\output_assets\{BuildingName}\{BuildingName}.gltf");
        var model = GameModel.From(graphicsDevice, gltfFile);
        model.TranslateTo(Position);
        Model = model;
    }

    public void Unload()
    {
        Model?.Dispose();
        Model = null;
    }

    public static Building From(JToken jBuilding)
    {
        var building = new Building();
                    
        var buildingNameToken = jBuilding["name"];
        if (buildingNameToken == null)
        {
            throw new Exception();
        }
        building.BuildingName = buildingNameToken.ToString();
        
        var buildingIdToken = jBuilding["id"];
        if (buildingIdToken == null)
        {
            throw new Exception();
        }
        building.BuildingId = buildingIdToken.Value<int>();
                    
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

        return building;
    }
}

public class Chunk
{
    public int Id;
    public string Name;
    public List<Building> Buildings = [];
    public byte[,] Collision = new byte[32, 32];
    public byte[,] Type = new byte[32, 32];
    public List<ChunkPlate> Plates = [];
    public GameModel Model;

    public bool IsLoaded => Model != null;

    public void Load(GraphicsDevice graphicsDevice)
    {
        foreach (var building in Buildings)
        {
            building.Load(graphicsDevice);
        }
    
        var basePath = @"A:\ModelExporter\Platin\overworldmaps\";
        string folderName = Id.ToString("D4");

        string[] extensions = new[] { ".glb", ".gltf" };
        string filePath = null;

        foreach (var ext in extensions)
        {
            string fileName = $"{folderName}{ext}";
            filePath = Path.Combine(basePath, folderName, fileName);

            if (File.Exists(filePath))
            {
                break;
            }

            filePath = null;
        }

        if (filePath != null)
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
    
    /// <summary>
    /// Überprüft die Kollision einer Zelle an den angegebenen Koordinaten.
    /// </summary>
    /// <param name="x">Die X-Koordinate der Zelle.</param>
    /// <param name="y">Die Y-Koordinate der Zelle.</param>
    /// <returns>Der Kollisionswert der Zelle.</returns>
    public byte CheckCellCollision(int x, int y)
    {
        try
        {
            if (x < 0 || x >= Collision.GetLength(1) || y < 0 || y >= Collision.GetLength(0))
            {
                throw new ArgumentOutOfRangeException("Zellenkoordinaten sind außerhalb des gültigen Bereichs.");
            }
            return Collision[y, x];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler in CheckCellCollision: {ex.Message}");
            return 0x00;
        }
    }

    /// <summary>
    /// Überprüft den Typ einer Zelle an den angegebenen Koordinaten.
    /// </summary>
    /// <param name="x">Die X-Koordinate der Zelle.</param>
    /// <param name="y">Die Y-Koordinate der Zelle.</param>
    /// <returns>Der Typwert der Zelle.</returns>
    public byte CheckCellType(int x, int y)
    {
        try
        {
            if (x < 0 || x >= Type.GetLength(1) || y < 0 || y >= Type.GetLength(0))
            {
                throw new ArgumentOutOfRangeException("Zellenkoordinaten sind außerhalb des gültigen Bereichs.");
            }
            return Type[y, x];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler in CheckCellType: {ex.Message}");
            return 0x00;
        }
    }
    
    public static Chunk Load(GraphicsDevice graphicsDevice, JToken jChunk)
    {
        var chunk = new Chunk();
        
        var chunkIdToken = jChunk["mapId"];
        if (chunkIdToken == null)
        {
            throw new Exception();
        }
        chunk.Id = chunkIdToken.Value<int>();
        
        var chunkNameToken = jChunk["mapName"];
        if (chunkNameToken == null)
        {
            throw new Exception();
        }
        chunk.Name = chunkNameToken.ToString();
        
        var chunkPermissionsToken = jChunk["permissions"];
        if (chunkPermissionsToken != null)
        {
            #region Collision
            var collisionsToken = chunkPermissionsToken["collisions"];
            if (collisionsToken != null)
            {
                var collisions = collisionsToken.ToObject<byte[][]>();

                for (var y = 0; y < collisions.Length; y++)
                {
                    var array = collisions[y];
                    for (var x = 0; x < array.Length; x++)
                    {
                        chunk.Collision[y, x] = array[x];
                    }
                }
            }
            #endregion
            
            #region Type
            var typesToken = chunkPermissionsToken["types"];
            if (typesToken != null)
            {
                var types = typesToken.ToObject<byte[][]>();

                for (var y = 0; y < types.Length; y++)
                {
                    var array = types[y];
                    for (var x = 0; x < array.Length; x++)
                    {
                        chunk.Type[y, x] = array[x];
                    }
                }
            }
            #endregion

            #region Plates

            var platesToken = chunkPermissionsToken["plates"];
            if (platesToken != null)
            {
                foreach (var jPlate in platesToken)
                {
                    if (jPlate is not { HasValues: true })
                    {
                        continue;
                    }
                    var chunkPlate = ChunkPlate.From(jPlate);
                    chunk.Plates.Add(chunkPlate);
                }
            }

            #endregion
        }
        
        var chunkBuildingsToken = jChunk["buildings"];
        if (chunkBuildingsToken != null)
        {
            foreach (var jBuilding in chunkBuildingsToken)
            {
                if (jBuilding is not { HasValues: true })
                {
                    continue;
                }
            
                var building = Building.From(jBuilding);
                chunk.Buildings.Add(building);
            }
        }
        
        
        return chunk;
    }
}