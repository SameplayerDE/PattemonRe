using System;
using System.Collections.Generic;
using System.Linq;
using HxGLTF;
using HxGLTF.Monogame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;

namespace PatteLib.World;

public class Building
{
    public static string RootDirectory = string.Empty;
    public Vector3 Position;
    public string BuildingName;
    public int BuildingId;
    public GameModel Model;
    
    public bool IsLoaded => Model != null;
    
    public void Load(GraphicsDevice graphicsDevice)
    {
        if (string.IsNullOrEmpty(RootDirectory))
        {
            throw new Exception();
        }
        var gltfFile = GLTFLoader.Load(@$"{RootDirectory}\{BuildingName}\{BuildingName}.gltf");
        var model = GameModel.From(graphicsDevice, gltfFile);
        model.TranslateTo(Position);
        model.Scale /= 16;
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
    
    public JToken Save()
    {
        var jBuilding = new JObject
        {
            ["name"] = BuildingName,
            ["id"] = BuildingId,
            ["x"] = Position.X,
            ["y"] = Position.Y,
            ["z"] = Position.Z
        };

        return jBuilding;
    }
    
    public static JToken Save(Building building)
    {
        var jBuilding = new JObject
        {
            ["name"] = building.BuildingName,
            ["id"] = building.BuildingId,
            ["x"] = building.Position.X,
            ["y"] = building.Position.Y,
            ["z"] = building.Position.Z
        };

        return jBuilding;
    }
    
}

public class Chunk
{
    public const int Wx = 32;
    public const int Wy = 32;
    
    public int Id;
    public string Name;
    public List<Building> Buildings = [];
    public byte[,] Collision = new byte[Wy, Wx];
    public byte[,] Type = new byte[Wy, Wx];
    public List<ChunkPlate> Plates = [];

    public void Unload()
    {
        foreach (var building in Buildings)
        {
            building.Unload();
        }
    }
    
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
    
    public ChunkPlate GetNearestChunkPlate(Vector3 position)
    {
        if (Plates.Count == 0)
        {
            return null;
        }

        var localX = (int)position.X;
        var localY = (int)position.Z;
        var localZ = position.Y;

        ChunkPlate nearestPlate = null;
        double nearestDistance = double.MaxValue;

        foreach (var plate in Plates)
        {
            var minX = plate.X;
            var minY = plate.Y;
            var maxX = minX + plate.Wx;
            var maxY = minY + plate.Wy;

            if (localX >= minX && localX < maxX && localY >= minY && localY < maxY)
            {
                var zDistance = Math.Abs(localZ - plate.Z);
                
                if (zDistance < nearestDistance)
                {
                    nearestPlate = plate;
                    nearestDistance = zDistance;
                }
            }
        }

        return nearestPlate;
    }

    
    public ChunkPlate[] GetChunkPlateUnderPosition(Vector3 position)
    {
        if (Plates.Count == 0)
        {
            return new ChunkPlate[0];
        }
    
        var localX = (int)position.X;
        var localY = (int)position.Z;
        var localZ = position.Y;
        const double tolerance = 0.5;

        var result = new List<ChunkPlate>();

        foreach (var plate in Plates)
        {
            var minX = plate.X;
            var minY = plate.Y;
            var maxX = minX + plate.Wx;
            var maxY = minY + plate.Wy;
            
            if (localX >= minX && localX < maxX && localY >= minY && localY < maxY)
            {
                if (localZ >= plate.Z - tolerance)
                {
                    result.Add(plate);
                }
            }
        }
    
        result.Sort((p1, p2) => p2.Z.CompareTo(p1.Z));
        return result.ToArray();
    }
    
    public static Chunk Load(JToken jChunk)
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