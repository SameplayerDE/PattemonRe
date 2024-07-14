using System.Reflection.Metadata;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using PatteLib.World;

namespace PatteLib.Data;

public class HeaderManager
{
    
    private Dictionary<int, ChunkHeader> _headers = [];
    public static string RootDirectory = string.Empty;
    
    public void Load(GraphicsDevice graphicsDevice)
    {
        if (string.IsNullOrEmpty(RootDirectory))
        {
            throw new Exception();
        }
        for (var i = 0; i < 592; i++)
        {
            var path = Path.Combine(RootDirectory, $"{i}.json");
            var headerJson = File.ReadAllText(path);
            var jHeader = JObject.Parse(headerJson);
            var header = ChunkHeader.Load(jHeader);
            _headers.Add(header.Id, header);
        }
    }
    
    public bool HasHeaderById(int id)
    {
        return _headers.ContainsKey(id);
    }
    
    public ChunkHeader? GetHeaderById(int id)
    {
        return _headers.GetValueOrDefault(id);
    }
}