using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;

namespace Horror.Graphics;

public struct DataEvent
{
    public string Name;
    public Dictionary<string, object> Data;

    public DataEvent(string name, Dictionary<string, object> data)
    {
        Name = name;
        Data = data;
    }

    public bool HasData => Data?.Count > 0;
}

public struct SpriteAnimationKeyFrame
{
    public int Frame;
    public int TextureIndex;
    public string[] Events;
}

public class SpriteAnimation
{
    private string _path;
    public string Name;
    public Texture2D[] Textures;
    public DataEvent[] Events;
    public Dictionary<string, DataEvent> EventDictionary;
    public SpriteAnimationKeyFrame[] KeyFrames;
    public int Duration;
    public bool HasTextures => Textures?.Length > 0;
    public bool HasEvents => Events?.Length > 0;

    private static readonly Dictionary<string, SpriteAnimation> Loaded = new Dictionary<string, SpriteAnimation>();

    public static SpriteAnimation Load(GraphicsDevice graphicsDevice, string path)
    {
        if (Loaded.TryGetValue(path, out var load))
        {
            return load;
        }

        var result = new SpriteAnimation();
        result._path = path;

        JObject jAnimation = JObject.Parse(File.ReadAllText(path));

        // Parse "name"
        result.Name = jAnimation["name"]?.ToString();

        // Parse "duration"
        result.Duration = jAnimation["duration"]?.ToObject<int>() ?? 0;

        // Parse "textures"
        JArray texturesArray = jAnimation["textures"] as JArray;
        if (texturesArray != null)
        {
            result.Textures = texturesArray.Select(texPath =>
                {
                    string texturePath = texPath.ToString();
                    string combinedPath = Path.IsPathRooted(texturePath) ? texturePath : Path.Combine(Path.GetDirectoryName(path), texturePath); // Assuming .png extension

                    return Texture2D.FromFile(graphicsDevice, combinedPath);
                }).ToArray();
        }

        // Parse "events"
        JArray eventsArray = jAnimation["events"] as JArray;
        result.EventDictionary = new Dictionary<string, DataEvent>();
        if (eventsArray != null)
        {
            result.Events = eventsArray.Select(eventObj =>
                {
                    string eventName = eventObj["name"]?.ToString();
                    Dictionary<string, object> eventData = eventObj["data"]?.ToObject<Dictionary<string, object>>();

                    var dataEvent = new DataEvent(eventName, eventData);

                    result.EventDictionary[eventName] = dataEvent;

                    return dataEvent;
                }).ToArray();
        }

        // Parse "keyFrames"
        JArray keyFramesArray = jAnimation["keyFrames"] as JArray;
        if (keyFramesArray != null)
        {
            var keyFrames = keyFramesArray.Select(keyFrameObj =>
            {
                int frame = keyFrameObj["frame"]?.ToObject<int>() ?? 0;
                int textureIndex = keyFrameObj["texture"]?.ToObject<int>() ?? 0;
                JArray eventsArrayInKeyFrame = keyFrameObj["events"] as JArray;
                string[] eventNames = eventsArrayInKeyFrame?.Select(e => e.ToString()).ToArray();

                return new SpriteAnimationKeyFrame
                {
                    Frame = frame,
                    TextureIndex = textureIndex,
                    Events = eventNames
                };
            }).ToArray();
            
            result.KeyFrames = keyFrames;
        }

        // Store in the Loaded dictionary
        Loaded.Add(path, result);

        return result;
    }
}